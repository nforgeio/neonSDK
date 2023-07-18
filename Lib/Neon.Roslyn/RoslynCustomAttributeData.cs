//-----------------------------------------------------------------------------
// FILE:        RoslynCustomAttributeData.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis;

namespace Neon.Roslyn
{
    internal class RoslynCustomAttributeData : CustomAttributeData
    {
        public RoslynCustomAttributeData(AttributeData a, MetadataLoadContext metadataLoadContext)
        {
            if (a.AttributeConstructor is null)
            {
                throw new InvalidOperationException();
            }

            var namedArguments = new List<CustomAttributeNamedArgument>();

            foreach (var na in a.NamedArguments)
            {
                var member = a.AttributeClass.BaseTypes().SelectMany(t => t.GetMembers(na.Key)).First();

                MemberInfo memberInfo = member switch
                {
                    IPropertySymbol property                  => property.AsPropertyInfo(metadataLoadContext),
                    IFieldSymbol    field                     => field.AsFieldInfo(metadataLoadContext),
                    IMethodSymbol   ctor when ctor.MethodKind == MethodKind.Constructor => ctor.AsConstructorInfo(metadataLoadContext),
                    IMethodSymbol   method                    => method.AsMethodInfo(metadataLoadContext),
                    ITypeSymbol     typeSymbol                => typeSymbol.AsType(metadataLoadContext),
                    _                                         => null,
                };

                if (memberInfo is not null)
                {
                    namedArguments.Add(new CustomAttributeNamedArgument(memberInfo, na.Value.Value));
                }
            }

            var constructorArguments = new List<CustomAttributeTypedArgument>();

            foreach (var ca in a.ConstructorArguments)
            {
                if (ca.Kind == TypedConstantKind.Error)
                {
                    continue;
                }

                object value = ca.Kind == TypedConstantKind.Array ? ca.Values : ca.Value;

                constructorArguments.Add(new CustomAttributeTypedArgument(ca.Type.AsType(metadataLoadContext), value));
            }
            Constructor          = a.AttributeConstructor.AsConstructorInfo(metadataLoadContext);
            NamedArguments       = namedArguments;
            ConstructorArguments = constructorArguments;
        }

        public override ConstructorInfo Constructor { get; }

        public override IList<CustomAttributeNamedArgument> NamedArguments { get; }

        public override IList<CustomAttributeTypedArgument> ConstructorArguments { get; }
    }
}
