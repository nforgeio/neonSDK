//-----------------------------------------------------------------------------
// FILE:        SharedUtilities.cs
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
using System.Linq;

using System.Reflection;

using Microsoft.CodeAnalysis;

namespace Neon.Roslyn
{
    public static class SharedUtilities
    {
        public static IList<System.Reflection.CustomAttributeData> GetCustomAttributesData(ISymbol symbol, MetadataLoadContext metadataLoadContext)
        {
            List<System.Reflection.CustomAttributeData> attributes = default;

            foreach (var a in symbol.GetAttributes())
            {
                attributes ??= new();
                attributes.Add(new RoslynCustomAttributeData(a, metadataLoadContext));
            }

            return (IList<System.Reflection.CustomAttributeData>)attributes ?? Array.Empty<System.Reflection.CustomAttributeData>();
        }

        public static System.Reflection.MethodAttributes GetMethodAttributes(IMethodSymbol method)
        {
            System.Reflection.MethodAttributes attributes = default;

            if (method.IsAbstract)
            {
                attributes |= System.Reflection.MethodAttributes.Abstract | System.Reflection.MethodAttributes.Virtual;
            }

            if (method.IsStatic)
            {
                attributes |= System.Reflection.MethodAttributes.Static;
            }

            if (method.IsVirtual || method.IsOverride)
            {
                attributes |= System.Reflection.MethodAttributes.Virtual;
            }

            switch (method.DeclaredAccessibility)
            {
                case Accessibility.Public:
                    attributes |= System.Reflection.MethodAttributes.Public;
                    break;
                case Accessibility.Private:
                    attributes |= System.Reflection.MethodAttributes.Private;
                    break;
                case Accessibility.Internal:
                    attributes |= System.Reflection.MethodAttributes.Assembly;
                    break;
            }

            if (method.MethodKind != MethodKind.Ordinary)
            {
                attributes |= System.Reflection.MethodAttributes.SpecialName;
            }

            return attributes;
        }

        public static bool MatchBindingFlags(System.Reflection.BindingFlags bindingFlags, ITypeSymbol thisType, ISymbol symbol)
        {
            var isPublic               = (symbol.DeclaredAccessibility & Accessibility.Public) == Accessibility.Public;
            var isNonProtectedInternal = (symbol.DeclaredAccessibility & Accessibility.ProtectedOrInternal) == 0;
            var isStatic               = symbol.IsStatic;
            var isInherited            = !SymbolEqualityComparer.Default.Equals(thisType, symbol.ContainingType);

            // TODO: REVIEW precomputing binding flags
            // From https://github.com/dotnet/runtime/blob/9ec7fc21862f3446c6c6f7dcfff275942e3884d3/src/coreclr/System.Private.CoreLib/src/System/RuntimeType.CoreCLR.cs#L2058

            //var symbolBindingFlags = ComputeBindingFlags(isPublic, isStatic, isInherited);

            //if (symbol is ITypeSymbol && !isStatic)
            //{
            //    symbolBindingFlags &= ~BindingFlags.Instance;
            //}

            // The below logic is a mishmash of copied logic from the following

            // https://github.com/dotnet/runtime/blob/9ec7fc21862f3446c6c6f7dcfff275942e3884d3/src/coreclr/System.Private.CoreLib/src/System/RuntimeType.CoreCLR.cs#L2261

            // filterFlags ^= BindingFlags.DeclaredOnly;

            // https://github.com/dotnet/runtime/blob/9ec7fc21862f3446c6c6f7dcfff275942e3884d3/src/coreclr/System.Private.CoreLib/src/System/RuntimeType.CoreCLR.cs#L2153

            //if ((filterFlags & symbolBindingFlags) != symbolBindingFlags)
            //{
            //    return false;
            //}

            // Filter by Public & Private
            if (isPublic)
            {
                if ((bindingFlags & System.Reflection.BindingFlags.Public) == 0)
                {
                    return false;
                }
            }
            else
            {
                if ((bindingFlags & System.Reflection.BindingFlags.NonPublic) == 0)
                {
                    return false;
                }
            }

            // Filter by DeclaredOnly
            if ((bindingFlags & System.Reflection.BindingFlags.DeclaredOnly) != 0 && isInherited)
            {
                return false;
            }

            if (symbol is not ITypeSymbol)
            {
                if (isStatic)
                {
                    if ((bindingFlags & System.Reflection.BindingFlags.FlattenHierarchy) == 0 && isInherited)
                    {
                        return false;
                    }

                    if ((bindingFlags & System.Reflection.BindingFlags.Static) == 0)
                    {
                        return false;
                    }
                }
                else
                {
                    if ((bindingFlags & System.Reflection.BindingFlags.Instance) == 0)
                    {
                        return false;
                    }
                }
            }

            // @Asymmetry - Internal, inherited, instance, non -protected, non-virtual, non-abstract members returned
            //              iff BindingFlags !DeclaredOnly, Instance and Public are present except for fields
            if (((bindingFlags & System.Reflection.BindingFlags.DeclaredOnly) == 0) &&        // DeclaredOnly not present
                 isInherited &&                                            // Is inherited Member

                isNonProtectedInternal &&                                 // Is non-protected internal member
                ((bindingFlags & System.Reflection.BindingFlags.NonPublic) != 0) &&           // BindingFlag.NonPublic present

                (!isStatic) &&                                              // Is instance member
                ((bindingFlags & System.Reflection.BindingFlags.Instance) != 0))              // BindingFlag.Instance present
            {
                if (symbol is not IMethodSymbol method)
                {
                    return false;
                }

                if (!method.IsVirtual && !method.IsAbstract)
                {
                    return false;
                }
            }

            return true;
        }

        public static System.Reflection.BindingFlags ComputeBindingFlags(System.Reflection.MemberInfo member)
        {
            if (member is System.Reflection.PropertyInfo p)
            {
                return ComputeBindingFlags(p.GetMethod ?? p.SetMethod);
            }

            var (isPublic, isStatic) = member switch
            {
                System.Reflection.FieldInfo f => (f.IsPublic, f.IsStatic),
                System.Reflection.MethodInfo m => (m.IsPublic, m.IsStatic),
                _            => throw new NotSupportedException()
            };

            var isInherited = member.ReflectedType != member.DeclaringType;

            return ComputeBindingFlags(isPublic, isStatic, isInherited);
        }

        private static System.Reflection.BindingFlags ComputeBindingFlags(bool isPublic, bool isStatic, bool isInherited)
        {
            System.Reflection.BindingFlags bindingFlags = isPublic ? System.Reflection.BindingFlags.Public : System.Reflection.BindingFlags.NonPublic;

            if (isInherited)
            {
                // We arrange things so the DeclaredOnly flag means "include inherited members"
                bindingFlags |= System.Reflection.BindingFlags.DeclaredOnly;

                if (isStatic)
                {
                    bindingFlags |= System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy;
                }
                else
                {
                    bindingFlags |= System.Reflection.BindingFlags.Instance;
                }
            }
            else
            {
                if (isStatic)
                {
                    bindingFlags |= System.Reflection.BindingFlags.Static;
                }
                else
                {
                    bindingFlags |= System.Reflection.BindingFlags.Instance;
                }
            }

            return bindingFlags;
        }

        public static IEnumerable<object> GetActualConstuctorParams(this AttributeData attributeData)
        {
            foreach (var arg in attributeData.ConstructorArguments)
            {
                if (arg.Kind == TypedConstantKind.Array)
                {
                    // Assume they are strings, but the array that we get from this
                    // should actually be of type of the objects within it, be it strings or ints
                    // This is definitely possible with reflection, I just don't know how exactly. 
                    // TODO
                    yield return arg.Values.Select(a => a.Value).OfType<string>().ToArray();
                }
                else
                {
                    yield return arg.Value;
                }
            }
        }

        public static IEnumerable<object> GetActualConstuctorParams(this System.Reflection.CustomAttributeData attributeData)
        {
            foreach (var arg in attributeData.ConstructorArguments)
            {
                if (arg.ArgumentType.IsArray)
                {
                    // Assume they are strings, but the array that we get from this
                    // should actually be of type of the objects within it, be it strings or ints
                    // This is definitely possible with reflection, I just don't know how exactly. 
                    // TODO
                    yield return arg.Value.ToString();
                }
                else
                {
                    yield return arg.Value;
                }
            }
        }
    }
}
