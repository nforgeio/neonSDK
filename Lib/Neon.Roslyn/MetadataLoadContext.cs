//-----------------------------------------------------------------------------
// FILE:        MetadataLoadContext.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Neon.Roslyn
{
    public class MetadataLoadContext
    {
        private readonly Compilation                           compilation;
        private readonly ConcurrentDictionary<ISymbol, object> cache = new(SymbolEqualityComparer.Default);

        public MetadataLoadContext(Compilation compilation)
        {
            this.compilation = compilation;
        }

        public Assembly Assembly => compilation.Assembly.AsAssembly(this);

        internal Compilation Compilation => compilation;

        public Type ResolveType(TypeSyntax node)
        {
            var semanticModel = compilation.GetSemanticModel(node.SyntaxTree);
            return ResolveType(semanticModel.GetTypeInfo(node).Type);
        }

        public Type ResolveType(string fullyQualifiedMetadataName)
        {
            return compilation.GetTypeByMetadataName(fullyQualifiedMetadataName)?.AsType(this);
        }

        public Type ResolveType(ISymbol symbol)
        {
            var result = compilation.GetTypeByMetadataName(symbol.GetFullMetadataName());

            return result?.AsType(this);
        }

        public Type ResolveType<T>() => ResolveType(typeof(T));

        public Type ResolveType(Type type)
        {
            if (type is RoslynType)
            {
                return type;
            }

            var resolvedType = compilation.GetTypeByMetadataName(type.FullName);

            if (resolvedType is not null)
            {
                return resolvedType.AsType(this);
            }

            if (type.IsArray)
            {
                var typeSymbol = compilation.GetTypeByMetadataName(type.GetElementType().FullName);
                if (typeSymbol is null)
                {
                    return null;
                }

                return compilation.CreateArrayTypeSymbol(typeSymbol).AsType(this);
            }

            if (type.IsGenericType)
            {
                var openGenericTypeSymbol = compilation.GetTypeByMetadataName(type.GetGenericTypeDefinition().FullName);
                if (openGenericTypeSymbol is null)
                {
                    return null;
                }

                return openGenericTypeSymbol.AsType(this).MakeGenericType(type.GetGenericArguments());
            }

            return null;
        }

        public TMember GetOrCreate<TMember>(ISymbol symbol) where TMember : class
        {
            if (symbol is null)
            {
                return null;
            }

            return (TMember)cache.GetOrAdd(symbol, s => s switch
            {
                ITypeSymbol      t                   => new RoslynType(t, this),
                IFieldSymbol     f                   => new RoslynFieldInfo(f, this),
                IPropertySymbol  p                   => new RoslynPropertyInfo(p, this),
                IMethodSymbol    c when c.MethodKind == MethodKind.Constructor => new RoslynConstructorInfo(c, this),
                IMethodSymbol    m                   => new RoslynMethodInfo(m, this),
                IParameterSymbol param               => new RoslynParameterInfo(param, this),
                IAssemblySymbol  a                   => new RoslynAssembly(a, this),
                _                                    => null
            });
        }

        public TMember ResolveMember<TMember>(TMember memberInfo) where TMember : MemberInfo
        {
            return memberInfo switch
            {
                RoslynFieldInfo    f => (TMember)(object)f,
                RoslynMethodInfo   m => (TMember)(object)m,
                RoslynPropertyInfo p => (TMember)(object)p,
                MethodInfo         m => (TMember)(object)ResolveType(m.ReflectedType)?.GetMethod(m.Name, SharedUtilities.ComputeBindingFlags(m), binder: null, types: m.GetParameters().Select(t => t.ParameterType).ToArray(), modifiers: null),
                PropertyInfo       p => (TMember)(object)ResolveType(p.ReflectedType)?.GetProperty(p.Name, SharedUtilities.ComputeBindingFlags(p), binder: null, returnType: p.PropertyType, types: p.GetIndexParameters().Select(t => t.ParameterType).ToArray(), modifiers: null),
                FieldInfo          f => (TMember)(object)ResolveType(f.ReflectedType)?.GetField(f.Name, SharedUtilities.ComputeBindingFlags(f)),
                _                    => null
            };
        }
    }
}