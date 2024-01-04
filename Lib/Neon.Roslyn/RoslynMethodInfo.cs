//-----------------------------------------------------------------------------
// FILE:        RoslynMethodInfo.cs
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
using System.Reflection;

using Microsoft.CodeAnalysis;

namespace Neon.Roslyn
{
    public class RoslynMethodInfo : MethodInfo
    {
        private readonly IMethodSymbol       method;
        private readonly MetadataLoadContext metadataLoadContext;

        public RoslynMethodInfo(IMethodSymbol method, MetadataLoadContext metadataLoadContext)
        {
            this.method              = method;
            this.metadataLoadContext = metadataLoadContext;

            Attributes = SharedUtilities.GetMethodAttributes(method);
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw new NotImplementedException();

        public override MethodAttributes Attributes { get; }

        public override RuntimeMethodHandle MethodHandle => throw new NotSupportedException();

        public override Type DeclaringType => method.ContainingType.AsType(metadataLoadContext);

        public override Type ReturnType => method.ReturnType.AsType(metadataLoadContext);

        public override string Name => method.Name;

        public override bool IsGenericMethod => method.IsGenericMethod;

        public override Type ReflectedType => throw new NotImplementedException();

        public IMethodSymbol MethodSymbol => method;

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return SharedUtilities.GetCustomAttributesData(method, metadataLoadContext);
        }

        public override MethodInfo GetBaseDefinition()
        {
            var method = this.method;

            // Walk until we find the base definition for this method
            while (method.OverriddenMethod is not null)
            {
                method = method.OverriddenMethod;
            }

            if (method.Equals(this.method, SymbolEqualityComparer.Default))
            {
                return this;
            }

            return method.AsMethodInfo(metadataLoadContext);
        }

        public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
        {
            var typeSymbols = new ITypeSymbol[typeArguments.Length];

            for (int i = 0; i < typeSymbols.Length; i++)
            {
                typeSymbols[i] = metadataLoadContext.ResolveType(typeArguments[i]).GetTypeSymbol();
            }

            return method.Construct(typeSymbols).AsMethodInfo(metadataLoadContext);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotSupportedException();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotSupportedException();
        }

        public override Type[] GetGenericArguments()
        {
            List<Type> typeArguments = default;

            foreach (var t in method.TypeArguments)
            {
                typeArguments ??= new();
                typeArguments.Add(t.AsType(metadataLoadContext));
            }

            return typeArguments?.ToArray() ?? Array.Empty<Type>();
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            throw new NotImplementedException();
        }

        public override ParameterInfo[] GetParameters()
        {
            List<ParameterInfo> parameters = default;

            foreach (var p in method.Parameters)
            {
                parameters ??= new();
                parameters.Add(p.AsParameterInfo(metadataLoadContext));
            }

            return parameters?.ToArray() ?? Array.Empty<ParameterInfo>();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotSupportedException();
        }

        public override string ToString() => method.ToString();
    }
}
