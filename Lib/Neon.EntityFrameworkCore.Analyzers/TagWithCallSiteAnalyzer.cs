//-----------------------------------------------------------------------------
// FILE:	    TagWithCallSiteAnalyzer.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:	Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Neon.EntityFrameworkCore.Analyzers
{
    /// <summary>
    /// Reports Entity Framework Core queries that are executed without having been tagged by
    /// <c>TagWithCallSite()</c>, so that the SQL reaching the database carries the source file
    /// and line that produced it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The rule only fires where the tag can actually be applied: the query has to be rooted at
    /// a <c>DbSet&lt;T&gt;</c> (or <c>DbContext.Set&lt;T&gt;()</c>) so that EF Core owns the
    /// translation, the compilation has to reference an EF Core that offers
    /// <c>TagWithCallSite()</c>, and the execution must not sit inside an expression tree, where
    /// the tag belongs on the enclosing query instead.  Anything the analyzer cannot follow back
    /// to a query root — a query handed in as an <c>IQueryable&lt;T&gt;</c> parameter, for
    /// instance — is left alone rather than guessed at.
    /// </para>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TagWithCallSiteAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Untagged EF Core query diagnostic ID.
        /// </summary>
        public const string DiagnosticId = "NEONEFC0001";

        /// <summary>
        /// Set this global analyzer option to <c>true</c> to accept a <c>TagWith()</c> call in
        /// place of <c>TagWithCallSite()</c>.
        /// </summary>
        public const string AllowTagWithOption = "neon_efcore_tag_with_call_site_allow_tag_with";

        private const string QueryableExtensionsMetadataName = "Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions";
        private const string RelationalExtensionsMetadataName = "Microsoft.EntityFrameworkCore.RelationalQueryableExtensions";
        private const string LinqQueryableMetadataName       = "System.Linq.Queryable";
        private const string LinqEnumerableMetadataName      = "System.Linq.Enumerable";
        private const string DbSetMetadataName               = "Microsoft.EntityFrameworkCore.DbSet`1";
        private const string DbContextMetadataName           = "Microsoft.EntityFrameworkCore.DbContext";
        private const string QueryableMetadataName           = "System.Linq.IQueryable`1";
        private const string EnumerableMetadataName          = "System.Collections.Generic.IEnumerable`1";
        private const string ExpressionMetadataName          = "System.Linq.Expressions.Expression`1";
        private const string TagWithCallSiteMethodName       = "TagWithCallSite";
        private const string TagWithMethodName               = "TagWith";
        private const string SetMethodName                   = "Set";

        // Guards against pathological (or cyclic) source that would otherwise let the chain walk
        // run away.  Real query chains are nowhere near this long.

        private const int MaxChainLength = 64;

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id:                 DiagnosticId,
            title:              "EF Core query should be tagged with its call site",
            messageFormat:      "EF Core query executed by '{0}' is not tagged with its call site",
            category:           "Usage",
            defaultSeverity:    DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description:        "Call TagWithCallSite() on EF Core queries so the generated SQL carries the source file and line it came from, which is what makes a slow query captured in database logs traceable back to the code that issued it.");

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private static void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            var symbols = EfCoreSymbols.From(context.Compilation);

            // Nothing to say when the compilation isn't using an EF Core that can be tagged.

            if (symbols == null)
            {
                return;
            }

            context.RegisterOperationAction(c => AnalyzeInvocation(c, symbols), OperationKind.Invocation);
            context.RegisterOperationAction(c => AnalyzeForEachLoop(c, symbols), OperationKind.Loop);
        }

        /// <summary>
        /// Handles the query executing operators: <c>ToListAsync()</c>, <c>FirstOrDefaultAsync()</c>,
        /// <c>ExecuteDeleteAsync()</c>, <c>AsEnumerable()</c> and friends.
        /// </summary>
        private static void AnalyzeInvocation(OperationAnalysisContext context, EfCoreSymbols symbols)
        {
            var invocation = (IInvocationOperation)context.Operation;

            if (!IsQueryExecution(invocation, symbols))
            {
                return;
            }

            ReportWhenUntagged(context, symbols, GetSource(invocation), invocation.Syntax.GetLocation(), invocation.TargetMethod.Name);
        }

        /// <summary>
        /// Handles <c>foreach</c> and <c>await foreach</c> over a query, which executes it just as
        /// surely as any of the operators do.
        /// </summary>
        private static void AnalyzeForEachLoop(OperationAnalysisContext context, EfCoreSymbols symbols)
        {
            var loop = context.Operation as IForEachLoopOperation;

            if (loop == null)
            {
                return;
            }

            var collection = Unwrap(loop.Collection);

            if (collection == null || !symbols.IsQueryable(collection.Type))
            {
                return;
            }

            ReportWhenUntagged(context, symbols, collection, loop.Collection.Syntax.GetLocation(), "foreach");
        }

        private static void ReportWhenUntagged(
            OperationAnalysisContext context,
            EfCoreSymbols symbols,
            IOperation source,
            Location location,
            string executionName)
        {
            if (source == null || IsInsideExpressionTree(context.Operation, symbols))
            {
                return;
            }

            var allowTagWith = ReadAllowTagWithOption(context.Options.AnalyzerConfigOptionsProvider.GlobalOptions);

            if (AnalyzeChain(source, symbols, allowTagWith) != ChainResult.Untagged)
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, location, executionName));
        }

        /// <summary>
        /// Determines whether <paramref name="invocation"/> is a LINQ operator that forces the
        /// query to run.  Composable operators (<c>Where()</c>, <c>Include()</c>,
        /// <c>AsNoTracking()</c>, ...) hand back another <see cref="System.Linq.IQueryable{T}"/>
        /// and so are not execution points; everything else applied to a queryable is.
        /// </summary>
        private static bool IsQueryExecution(IInvocationOperation invocation, EfCoreSymbols symbols)
        {
            var method = invocation.TargetMethod;

            // Instance members of DbSet<T> — Add(), Find(), ToString() — are not query operators
            // and cannot carry a tag.  Operators we don't recognize are left alone too, since a
            // hand written IQueryable<T> helper may well tag the query itself.

            if (!method.IsExtensionMethod || !symbols.IsKnownOperator(method))
            {
                return false;
            }

            // The receiver has to be unwrapped before its type is asked for: an operator declared
            // over IEnumerable<T> — ToList() and friends bind to System.Linq.Enumerable even when
            // applied to a DbSet<T> — sees the receiver through an implicit conversion.

            var source = Unwrap(GetSource(invocation));

            return source != null
                && symbols.IsQueryable(source.Type)
                && !symbols.IsQueryable(method.ReturnType);
        }

        private enum ChainResult
        {
            /// <summary>
            /// The chain could not be traced back to an EF Core query root, so the analyzer has
            /// no business reporting on it.
            /// </summary>
            NotEfCoreQuery,

            /// <summary>
            /// The chain is already tagged.
            /// </summary>
            Tagged,

            /// <summary>
            /// The chain reaches a query root without ever being tagged.
            /// </summary>
            Untagged
        }

        /// <summary>
        /// Walks a query chain from its execution point back towards its root, looking for a tag
        /// on the way.
        /// </summary>
        private static ChainResult AnalyzeChain(IOperation source, EfCoreSymbols symbols, bool allowTagWith)
        {
            var current = Unwrap(source);

            for (int i = 0; i < MaxChainLength && current != null; i++)
            {
                switch (current)
                {
                    case IInvocationOperation invocation:

                        var method = invocation.TargetMethod;

                        if (IsTag(method, symbols, allowTagWith))
                        {
                            return ChainResult.Tagged;
                        }

                        if (IsSetMethod(method, symbols))
                        {
                            return ChainResult.Untagged;
                        }

                        // Stop at anything that isn't a LINQ or EF Core operator: a hand written
                        // helper could be tagging the query on our behalf and we have no way to
                        // see that from here.

                        if (!symbols.IsKnownOperator(method))
                        {
                            return ChainResult.NotEfCoreQuery;
                        }

                        var next = Unwrap(GetSource(invocation));

                        if (next == null || !(symbols.IsQueryable(next.Type) || symbols.IsEnumerable(next.Type)))
                        {
                            return ChainResult.NotEfCoreQuery;
                        }

                        current = next;
                        break;

                    case IPropertyReferenceOperation property:

                        return symbols.IsDbSet(property.Type) ? ChainResult.Untagged : ChainResult.NotEfCoreQuery;

                    case IFieldReferenceOperation field:

                        return symbols.IsDbSet(field.Type) ? ChainResult.Untagged : ChainResult.NotEfCoreQuery;

                    case IParameterReferenceOperation parameter:

                        return symbols.IsDbSet(parameter.Type) ? ChainResult.Untagged : ChainResult.NotEfCoreQuery;

                    case ILocalReferenceOperation local:

                        var value = ResolveLocal(local);

                        if (value == null)
                        {
                            return ChainResult.NotEfCoreQuery;
                        }

                        current = Unwrap(value);
                        break;

                    default:

                        return ChainResult.NotEfCoreQuery;
                }
            }

            return ChainResult.NotEfCoreQuery;
        }

        /// <summary>
        /// Returns the receiver an operator was applied to.  Extension methods carry it as their
        /// first argument whether they were written in reduced form or not.
        /// </summary>
        private static IOperation GetSource(IInvocationOperation invocation)
        {
            if (invocation.TargetMethod.IsExtensionMethod)
            {
                return invocation.Arguments.Length > 0 ? invocation.Arguments[0].Value : null;
            }

            return invocation.Instance;
        }

        /// <summary>
        /// Follows a local back to the expression it was assigned, when that assignment is
        /// unambiguous.  A local written more than once tells us nothing reliable, so it ends the
        /// walk instead.
        /// </summary>
        private static IOperation ResolveLocal(ILocalReferenceOperation local)
        {
            var root = local.Parent;

            while (root?.Parent != null)
            {
                root = root.Parent;
            }

            if (root == null)
            {
                return null;
            }

            IOperation resolved = null;

            foreach (var operation in root.Descendants())
            {
                IOperation candidate = null;

                switch (operation)
                {
                    case IVariableDeclaratorOperation declarator
                        when SymbolEqualityComparer.Default.Equals(declarator.Symbol, local.Local):

                        candidate = declarator.Initializer?.Value;
                        break;

                    case ISimpleAssignmentOperation assignment
                        when Unwrap(assignment.Target) is ILocalReferenceOperation target
                             && SymbolEqualityComparer.Default.Equals(target.Local, local.Local):

                        candidate = assignment.Value;
                        break;
                }

                if (candidate == null)
                {
                    continue;
                }

                if (resolved != null)
                {
                    return null;    // assigned more than once
                }

                resolved = candidate;
            }

            return resolved;
        }

        /// <summary>
        /// Determines whether an operation sits inside an expression tree.  A query composed
        /// inside a projection lambda is translated as part of its parent query, so the tag
        /// belongs on the parent and reporting here would just be noise.
        /// </summary>
        private static bool IsInsideExpressionTree(IOperation operation, EfCoreSymbols symbols)
        {
            for (var parent = operation.Parent; parent != null; parent = parent.Parent)
            {
                if (parent.Type is INamedTypeSymbol named
                    && SymbolEqualityComparer.Default.Equals(named.ConstructedFrom, symbols.Expression))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsTag(IMethodSymbol method, EfCoreSymbols symbols, bool allowTagWith)
        {
            if (!SymbolEqualityComparer.Default.Equals(method.ContainingType, symbols.QueryableExtensions))
            {
                return false;
            }

            return method.Name == TagWithCallSiteMethodName
                || (allowTagWith && method.Name == TagWithMethodName);
        }

        /// <summary>
        /// Determines whether a method is <c>DbContext.Set&lt;T&gt;()</c>, which roots a query
        /// just as a <c>DbSet&lt;T&gt;</c> property does.
        /// </summary>
        private static bool IsSetMethod(IMethodSymbol method, EfCoreSymbols symbols)
        {
            return method.Name == SetMethodName
                && !method.IsExtensionMethod
                && symbols.IsDbSet(method.ReturnType)
                && symbols.DerivesFromDbContext(method.ContainingType);
        }

        private static IOperation Unwrap(IOperation operation)
        {
            while (true)
            {
                switch (operation)
                {
                    case IConversionOperation conversion:

                        operation = conversion.Operand;
                        break;

                    case IParenthesizedOperation parenthesized:

                        operation = parenthesized.Operand;
                        break;

                    default:

                        return operation;
                }
            }
        }

        private static bool ReadAllowTagWithOption(AnalyzerConfigOptions options)
        {
            return options.TryGetValue(AllowTagWithOption, out var value)
                && bool.TryParse(value.Trim(), out var allow)
                && allow;
        }

        /// <summary>
        /// The EF Core symbols the analyzer needs, resolved once per compilation.
        /// </summary>
        private sealed class EfCoreSymbols
        {
            public INamedTypeSymbol QueryableExtensions { get; private set; }
            public INamedTypeSymbol DbSet { get; private set; }
            public INamedTypeSymbol DbContext { get; private set; }
            public INamedTypeSymbol Queryable { get; private set; }
            public INamedTypeSymbol Enumerable { get; private set; }
            public INamedTypeSymbol Expression { get; private set; }

            /// <summary>
            /// The types declaring the LINQ and EF Core query operators the analyzer understands.
            /// </summary>
            public ImmutableArray<INamedTypeSymbol> KnownOperatorContainers { get; private set; }

            /// <summary>
            /// Resolves the symbols, returning <c>null</c> when the compilation references no
            /// EF Core, or an EF Core predating <c>TagWithCallSite()</c>.  Either way there is
            /// nothing the analyzer could sensibly ask the user to do.
            /// </summary>
            public static EfCoreSymbols From(Compilation compilation)
            {
                var queryableExtensions = compilation.GetTypeByMetadataName(QueryableExtensionsMetadataName);
                var dbSet               = compilation.GetTypeByMetadataName(DbSetMetadataName);
                var dbContext           = compilation.GetTypeByMetadataName(DbContextMetadataName);
                var queryable           = compilation.GetTypeByMetadataName(QueryableMetadataName);
                var enumerable          = compilation.GetTypeByMetadataName(EnumerableMetadataName);
                var expression          = compilation.GetTypeByMetadataName(ExpressionMetadataName);

                if (queryableExtensions == null || dbSet == null || dbContext == null || queryable == null || enumerable == null)
                {
                    return null;
                }

                if (!queryableExtensions.GetMembers(TagWithCallSiteMethodName).OfType<IMethodSymbol>().Any())
                {
                    return null;
                }

                var knownOperatorContainers = new[]
                {
                    queryableExtensions,
                    compilation.GetTypeByMetadataName(RelationalExtensionsMetadataName),
                    compilation.GetTypeByMetadataName(LinqQueryableMetadataName),
                    compilation.GetTypeByMetadataName(LinqEnumerableMetadataName)
                };

                return new EfCoreSymbols()
                {
                    QueryableExtensions = queryableExtensions,
                    DbSet = dbSet,
                    DbContext = dbContext,
                    Queryable = queryable,
                    Enumerable = enumerable,
                    Expression = expression,

                    KnownOperatorContainers = knownOperatorContainers
                        .Where(type => type != null)
                        .ToImmutableArray()
                };
            }

            /// <summary>
            /// Determines whether a method is one of the LINQ or EF Core query operators.
            /// </summary>
            public bool IsKnownOperator(IMethodSymbol method)
            {
                return method.ContainingType != null
                    && KnownOperatorContainers.Any(
                        container => SymbolEqualityComparer.Default.Equals(container, method.ContainingType));
            }

            public bool IsQueryable(ITypeSymbol type) => Implements(type, Queryable);

            public bool IsEnumerable(ITypeSymbol type) => Implements(type, Enumerable);

            public bool IsDbSet(ITypeSymbol type)
            {
                for (var current = type as INamedTypeSymbol; current != null; current = current.BaseType)
                {
                    if (SymbolEqualityComparer.Default.Equals(current.ConstructedFrom, DbSet))
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool DerivesFromDbContext(ITypeSymbol type)
            {
                for (var current = type as INamedTypeSymbol; current != null; current = current.BaseType)
                {
                    if (SymbolEqualityComparer.Default.Equals(current, DbContext))
                    {
                        return true;
                    }
                }

                return false;
            }

            private static bool Implements(ITypeSymbol type, INamedTypeSymbol definition)
            {
                var named = type as INamedTypeSymbol;

                if (named == null)
                {
                    return false;
                }

                if (SymbolEqualityComparer.Default.Equals(named.ConstructedFrom, definition))
                {
                    return true;
                }

                return named.AllInterfaces.Any(
                    interfaceType => SymbolEqualityComparer.Default.Equals(interfaceType.ConstructedFrom, definition));
            }
        }
    }
}