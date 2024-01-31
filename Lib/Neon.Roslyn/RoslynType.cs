//-----------------------------------------------------------------------------
// FILE:        RoslynType.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis;

namespace Neon.Roslyn
{
    public class RoslynType : Type
    {
        private readonly ITypeSymbol         typeSymbol;
        private readonly MetadataLoadContext metadataLoadContext;
        private readonly bool                isByRef;
        private TypeAttributes?              typeAttributes;

        public RoslynType(ITypeSymbol typeSymbol, MetadataLoadContext metadataLoadContext, bool isByRef = false)
        {
            this.typeSymbol          = typeSymbol;
            this.metadataLoadContext = metadataLoadContext;
            this.isByRef             = isByRef;
        }

        public override Assembly Assembly => typeSymbol.ContainingAssembly.AsAssembly(metadataLoadContext);

        public override string AssemblyQualifiedName => throw new NotImplementedException();

        public override Type BaseType => typeSymbol.BaseType.AsType(metadataLoadContext);

        public override string FullName => Namespace is null ? Name : Namespace + "." + Name;

        public override Guid GUID => Guid.Empty;

        public override Module Module => throw new NotImplementedException();

        public override string Namespace
        {
            get
            {
                if (typeSymbol.ContainingType != null)
                {
                    return typeSymbol.ContainingType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)) is { Length: > 0 } ns ? ns : null;
                }
                else
                {
                    return typeSymbol.ContainingNamespace?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)) is { Length: > 0 } ns ? ns : null;
                }
            }
        }

        public override Type UnderlyingSystemType => this;

        public override string Name => ArrayTypeSymbol is { } ar ? ar.ElementType.MetadataName + "[]" : typeSymbol.MetadataName;

        public override bool IsGenericType => NamedTypeSymbol?.IsGenericType ?? false;

        private INamedTypeSymbol NamedTypeSymbol => typeSymbol as INamedTypeSymbol;

        private IArrayTypeSymbol ArrayTypeSymbol => typeSymbol as IArrayTypeSymbol;

        public override bool IsGenericTypeDefinition => IsGenericType && SymbolEqualityComparer.Default.Equals(NamedTypeSymbol, NamedTypeSymbol.ConstructedFrom);

        public override bool IsGenericParameter => typeSymbol.TypeKind == TypeKind.TypeParameter;

        public ITypeSymbol TypeSymbol => typeSymbol;

        public override bool IsEnum => typeSymbol.TypeKind == TypeKind.Enum;

        public override bool IsConstructedGenericType => NamedTypeSymbol?.IsUnboundGenericType == false;

        public override Type DeclaringType => typeSymbol.ContainingType?.AsType(metadataLoadContext);

        public override int GetArrayRank()
        {
            return ArrayTypeSymbol.Rank;
        }

        public override Type[] GetGenericArguments()
        {
            if (NamedTypeSymbol is null)
            {
                return Array.Empty<Type>();
            }

            var args = new List<Type>();

            foreach (var item in NamedTypeSymbol.TypeArguments)
            {
                args.Add(item.AsType(metadataLoadContext));
            }

            return args.ToArray();
        }

        public override Type GetGenericTypeDefinition()
        {
            return NamedTypeSymbol?.ConstructedFrom.AsType(metadataLoadContext) ?? throw new NotSupportedException();
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return SharedUtilities.GetCustomAttributesData(typeSymbol, metadataLoadContext);
        }

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            if (NamedTypeSymbol is null)
            {
                return Array.Empty<ConstructorInfo>();
            }

            List<ConstructorInfo> ctors = default;

            foreach (var c in NamedTypeSymbol.Constructors)
            {
                if (!SharedUtilities.MatchBindingFlags(bindingAttr, typeSymbol, c))
                {
                    continue;
                }

                ctors ??= new();
                ctors.Add(c.AsConstructorInfo(metadataLoadContext));
            }

            return ctors?.ToArray() ?? Array.Empty<ConstructorInfo>();
        }

        public override Type MakeByRefType()
        {
            return new RoslynType(typeSymbol, metadataLoadContext, isByRef: true);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotSupportedException();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotSupportedException();
        }

        public override Type MakeArrayType()
        {
            return metadataLoadContext.Compilation.CreateArrayTypeSymbol(typeSymbol).AsType(metadataLoadContext);
        }

        public override Type MakeGenericType(params Type[] typeArguments)
        {
            if (!IsGenericTypeDefinition)
            {
                throw new NotSupportedException();
            }

            var typeSymbols = new ITypeSymbol[typeArguments.Length];
            for (int i = 0; i < typeArguments.Length; i++)
            {
                typeSymbols[i] = metadataLoadContext.ResolveType(typeArguments[i]).GetTypeSymbol();
            }

            return NamedTypeSymbol.Construct(typeSymbols).AsType(metadataLoadContext);
        }

        public override Type GetElementType()
        {
            return ArrayTypeSymbol?.ElementType.AsType(metadataLoadContext);
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            foreach (var symbol in typeSymbol.GetMembers())
            {
                if (symbol is not IFieldSymbol fieldSymbol)
                {
                    continue;
                }

                if (!SharedUtilities.MatchBindingFlags(bindingAttr, typeSymbol, symbol))
                {
                    continue;
                }

                return fieldSymbol.AsFieldInfo(metadataLoadContext);
            }

            return null;
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            List<FieldInfo> fields = default;

            foreach (var symbol in typeSymbol.GetMembers())
            {
                if (symbol is not IFieldSymbol fieldSymbol)
                {
                    continue;
                }

                if (!SharedUtilities.MatchBindingFlags(bindingAttr, typeSymbol, symbol))
                {
                    continue;
                }

                fields ??= new();
                fields.Add(fieldSymbol.AsFieldInfo(metadataLoadContext));
            }

            return fields?.ToArray() ?? Array.Empty<FieldInfo>();
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            foreach (var i in typeSymbol.Interfaces)
            {
                if (i.Name.Equals(name, comparison))
                {
                    return i.AsType(metadataLoadContext);
                }
            }
            return null;
        }

        public override Type[] GetInterfaces()
        {
            List<Type> interfaces = default;
            foreach (var i in typeSymbol.Interfaces)
            {
                interfaces ??= new();
                interfaces.Add(i.AsType(metadataLoadContext));
            }
            return interfaces?.ToArray() ?? Array.Empty<Type>();
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            List<MemberInfo> members = null;

            foreach (var t in typeSymbol.BaseTypes())
            {
                foreach (var symbol in t.GetMembers())
                {
                    if (!SharedUtilities.MatchBindingFlags(bindingAttr, typeSymbol, symbol))
                    {
                        continue;
                    }

                    MemberInfo member = symbol switch
                    {
                        IFieldSymbol    f                   => f.AsFieldInfo(metadataLoadContext),
                        IPropertySymbol p                   => p.AsPropertyInfo(metadataLoadContext),
                        IMethodSymbol   c when c.MethodKind == MethodKind.Constructor => c.AsConstructorInfo(metadataLoadContext),
                        IMethodSymbol   m                   => m.AsMethodInfo(metadataLoadContext),
                        _                                   => null
                    };

                    if (member is null)
                    {
                        continue;
                    }

                    members ??= new();
                    members.Add(member);
                }
            }

            // https://github.com/dotnet/runtime/blob/9ec7fc21862f3446c6c6f7dcfff275942e3884d3/src/coreclr/System.Private.CoreLib/src/System/RuntimeType.CoreCLR.cs#L2693-L2694
            bindingAttr &= ~BindingFlags.Static;
            foreach (var type in GetNestedTypes(bindingAttr))
            {
                if (type.IsInterface)
                {
                    continue;
                }

                members ??= new();
                members.Add(type);
            }

            return members?.ToArray() ?? Array.Empty<MemberInfo>();
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            List<MethodInfo> methods = null;

            foreach (var type in typeSymbol.BaseTypes())
            {
                foreach (var member in type.GetMembers())
                {
                    if (member is not IMethodSymbol method || method.MethodKind == MethodKind.Constructor)
                    {
                        continue;
                    }

                    if (!SharedUtilities.MatchBindingFlags(bindingAttr, typeSymbol, method))
                    {
                        continue;
                    }

                    methods ??= new();
                    methods.Add(method.AsMethodInfo(metadataLoadContext));
                }
            }

            return methods?.ToArray() ?? Array.Empty<MethodInfo>();
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            foreach (var type in typeSymbol.GetTypeMembers(name))
            {
                if (!SharedUtilities.MatchBindingFlags(bindingAttr, typeSymbol, type))
                {
                    continue;
                }

                return type.AsType(metadataLoadContext);
            }
            return null;
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            List<Type> nestedTypes = default;
            foreach (var type in typeSymbol.GetTypeMembers())
            {
                if (!SharedUtilities.MatchBindingFlags(bindingAttr, typeSymbol, type))
                {
                    continue;
                }

                nestedTypes ??= new();
                nestedTypes.Add(type.AsType(metadataLoadContext));
            }
            return nestedTypes?.ToArray() ?? Array.Empty<Type>();
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            List<PropertyInfo> properties = default;
            foreach (var t in typeSymbol.BaseTypes())
            {
                foreach (var symbol in t.GetMembers())
                {
                    if (symbol is not IPropertySymbol property)
                    {
                        continue;
                    }

                    if (!SharedUtilities.MatchBindingFlags(bindingAttr, typeSymbol, symbol))
                    {
                        continue;
                    }

                    properties ??= new();
                    properties.Add(new RoslynPropertyInfo(property, metadataLoadContext));
                }
            }
            return properties?.ToArray() ?? Array.Empty<PropertyInfo>();
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            throw new NotSupportedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotSupportedException();
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            if (!typeAttributes.HasValue)
            {
                typeAttributes = default(TypeAttributes);

                if (typeSymbol.IsAbstract)
                {
                    typeAttributes |= TypeAttributes.Abstract;
                }

                if (typeSymbol.TypeKind == TypeKind.Interface)
                {
                    typeAttributes |= TypeAttributes.Interface;
                }

                if (typeSymbol.IsSealed)
                {
                    typeAttributes |= TypeAttributes.Sealed;
                }

                bool isNested = typeSymbol.ContainingType != null;

                switch (typeSymbol.DeclaredAccessibility)
                {
                    case Accessibility.NotApplicable:
                    case Accessibility.Private:
                        typeAttributes |= isNested ? TypeAttributes.NestedPrivate : TypeAttributes.NotPublic;
                        break;
                    case Accessibility.ProtectedAndInternal:
                        typeAttributes |= isNested ? TypeAttributes.NestedFamANDAssem : TypeAttributes.NotPublic;
                        break;
                    case Accessibility.Protected:
                        typeAttributes |= isNested ? TypeAttributes.NestedFamily : TypeAttributes.NotPublic;
                        break;
                    case Accessibility.Internal:
                        typeAttributes |= isNested ? TypeAttributes.NestedAssembly : TypeAttributes.NotPublic;
                        break;
                    case Accessibility.ProtectedOrInternal:
                        typeAttributes |= isNested ? TypeAttributes.NestedFamORAssem : TypeAttributes.NotPublic;
                        break;
                    case Accessibility.Public:
                        typeAttributes |= isNested ? TypeAttributes.NestedPublic : TypeAttributes.Public;
                        break;
                }
            }

            return typeAttributes.Value;
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            // TODO: Use callConvention and modifiers

            //var comparison = (bindingAttr & BindingFlags.IgnoreCase) == BindingFlags.IgnoreCase
            //    ? StringComparison.OrdinalIgnoreCase
            //    : StringComparison.Ordinal;

            foreach (var member in typeSymbol.GetMembers())
            {
                if (member is not IMethodSymbol method || method.MethodKind != MethodKind.Constructor)
                {
                    // Only methods that are constructors
                    continue;
                }

                if (!SharedUtilities.MatchBindingFlags(bindingAttr, typeSymbol, member))
                {
                    continue;
                }

                var valid = true;

                if (types is { Length: > 0 })
                {
                    var parameterCount = types.Length;

                    // Compare parameter types
                    if (parameterCount != method.Parameters.Length)
                    {
                        continue;
                    }

                    for (int i = 0; i < parameterCount; i++)
                    {
                        var parameterType = types[i];
                        var parameterTypeSymbol = metadataLoadContext.ResolveType(parameterType)?.GetTypeSymbol();

                        if (parameterTypeSymbol is null)
                        {
                            valid = false;
                            break;
                        }

                        if (!method.Parameters[i].Type.Equals(parameterTypeSymbol, SymbolEqualityComparer.Default))
                        {
                            valid = false;
                            break;
                        }
                    }
                }

                if (valid)
                {
                    return method.AsConstructorInfo(metadataLoadContext);
                }
            }

            return null;
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            // TODO: Use callConvention and modifiers

            var comparison = (bindingAttr & BindingFlags.IgnoreCase) == BindingFlags.IgnoreCase
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            foreach (var member in typeSymbol.GetMembers())
            {
                if (member is not IMethodSymbol method || method.MethodKind == MethodKind.Constructor)
                {
                    // Only methods that are not constructors
                    continue;
                }

                if (!SharedUtilities.MatchBindingFlags(bindingAttr, typeSymbol, member))
                {
                    continue;
                }

                if (!method.Name.Equals(name, comparison))
                {
                    continue;
                }

                var valid = true;

                if (types is { Length: > 0 })
                {
                    var parameterCount = types.Length;

                    // Compare parameter types
                    if (parameterCount != method.Parameters.Length)
                    {
                        continue;
                    }

                    for (int i = 0; i < parameterCount; i++)
                    {
                        var parameterType = types[i];
                        var parameterTypeSymbol = metadataLoadContext.ResolveType(parameterType)?.GetTypeSymbol();

                        if (parameterTypeSymbol is null)
                        {
                            valid = false;
                            break;
                        }

                        if (!method.Parameters[i].Type.Equals(parameterTypeSymbol, SymbolEqualityComparer.Default))
                        {
                            valid = false;
                            break;
                        }
                    }
                }

                if (valid)
                {
                    return method.AsMethodInfo(metadataLoadContext);
                }
            }

            return null;
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            StringComparison comparison = (bindingAttr & BindingFlags.IgnoreCase) == BindingFlags.IgnoreCase
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            foreach (var symbol in typeSymbol.GetMembers())
            {
                if (symbol is not IPropertySymbol property)
                {
                    continue;
                }

                if (!SharedUtilities.MatchBindingFlags(bindingAttr, typeSymbol, symbol))
                {
                    continue;
                }

                if (!property.Name.Equals(name, comparison))
                {
                    continue;
                }

                var roslynReturnType = metadataLoadContext.ResolveType(returnType);

                if (roslynReturnType?.Equals(property.Type) == false)
                {
                    continue;
                }

                if (types is { Length: > 0 })
                {
                    var parameterCount = types.Length;

                    // Compare parameter types
                    if (parameterCount != property.Parameters.Length)
                    {
                        continue;
                    }
                }
                // TODO: Use parameters

                return property.AsPropertyInfo(metadataLoadContext);

            }
            return null;
        }

        protected override bool HasElementTypeImpl()
        {
            return ArrayTypeSymbol is not null;
        }

        protected override bool IsArrayImpl()
        {
            return ArrayTypeSymbol is not null;
        }

        protected override bool IsByRefImpl() => isByRef;

        protected override bool IsCOMObjectImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool IsPointerImpl()
        {
            return typeSymbol.Kind == SymbolKind.PointerType;
        }

        protected override bool IsPrimitiveImpl()
        {
            // Is IsPrimitive
            // https://github.com/dotnet/runtime/blob/55e95c80a7d7ec9d7bbbd5ad434604a1dc33e19c/src/libraries/System.Reflection.MetadataLoadContext/src/System/Reflection/TypeLoading/Types/RoType.TypeClassification.cs#L85

            return typeSymbol.SpecialType switch
            {
                SpecialType.System_Boolean => true,
                SpecialType.System_Char    => true,
                SpecialType.System_SByte   => true,
                SpecialType.System_Byte    => true,
                SpecialType.System_Int16   => true,
                SpecialType.System_UInt16  => true,
                SpecialType.System_Int32   => true,
                SpecialType.System_UInt32  => true,
                SpecialType.System_Int64   => true,
                SpecialType.System_UInt64  => true,
                SpecialType.System_Single  => true,
                SpecialType.System_Double  => true,
                SpecialType.System_String  => true,
                SpecialType.System_IntPtr  => true,
                SpecialType.System_UIntPtr => true,
                _                          => false
            };
        }

        public override string ToString()
        {
            return typeSymbol.ToString();
        }

        public override bool IsAssignableFrom(Type c)
        {
            var otherTypeSymbol = c switch
            {
                RoslynType rt => rt.typeSymbol,
                Type t when metadataLoadContext.ResolveType(t) is RoslynType rt => rt.typeSymbol,
                _ => null
            };

            if (otherTypeSymbol is null)
            {
                return false;
            }

            return otherTypeSymbol.AllInterfaces.Contains(typeSymbol, SymbolEqualityComparer.Default) ||
                   (otherTypeSymbol is INamedTypeSymbol ns && ns.BaseTypes().Contains(typeSymbol, SymbolEqualityComparer.Default));
        }

        public bool IsAssignableTo(Type c)
        {
            var otherTypeSymbol = c switch
            {
                RoslynType rt => rt.typeSymbol,
                Type t when metadataLoadContext.ResolveType(t) is RoslynType rt => rt.typeSymbol,
                _ => null
            };

            if (otherTypeSymbol is null)
            {
                return false;
            }

            return otherTypeSymbol.AllInterfaces.Contains(typeSymbol, SymbolEqualityComparer.Default) ||
                   (otherTypeSymbol is INamedTypeSymbol ns && ns.BaseTypes().Contains(typeSymbol, SymbolEqualityComparer.Default));
        }

        public override int GetHashCode()
        {
            return SymbolEqualityComparer.Default.GetHashCode(typeSymbol);
        }

        public override bool Equals(object o)
        {
            var otherTypeSymbol = o switch
            {
                RoslynType  rt                                                          => rt.typeSymbol,
                Type        t when metadataLoadContext.ResolveType(t) is RoslynType rt => rt.typeSymbol,
                ITypeSymbol ts                                                          => ts,
                _                                                                       => null
            };

            return typeSymbol.Equals(otherTypeSymbol, SymbolEqualityComparer.Default);
        }

        public override bool Equals(Type o)
        {
            var otherTypeSymbol = o switch
            {
                RoslynType rt                                                          => rt.typeSymbol,
                Type       t when metadataLoadContext.ResolveType(t) is RoslynType rt => rt.typeSymbol,
                _                                                                      => null
            };
            return typeSymbol.Equals(otherTypeSymbol, SymbolEqualityComparer.Default);
        }
    }
}
