//-----------------------------------------------------------------------------
// FILE:        RoslynConstructorInfo.cs
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
using System.Globalization;
using System.Reflection;

using Microsoft.CodeAnalysis;

namespace Neon.Roslyn
{
    internal class RoslynConstructorInfo : ConstructorInfo
    {
        private readonly IMethodSymbol       ctor;
        private readonly MetadataLoadContext metadataLoadContext;

        public RoslynConstructorInfo(IMethodSymbol ctor, MetadataLoadContext metadataLoadContext)
        {
            this.ctor                = ctor;
            this.metadataLoadContext = metadataLoadContext;
            Attributes               = SharedUtilities.GetMethodAttributes(ctor);
        }

        public override Type DeclaringType => ctor.ContainingType.AsType(metadataLoadContext);

        public override MethodAttributes Attributes { get; }

        public override RuntimeMethodHandle MethodHandle => throw new NotSupportedException();

        public override string Name => ctor.Name;

        public override Type ReflectedType => throw new NotImplementedException();

        public override bool IsGenericMethod => ctor.IsGenericMethod;

        public override Type[] GetGenericArguments()
        {
            var typeArguments = new List<Type>();

            foreach (var t in ctor.TypeArguments)
            {
                typeArguments.Add(t.AsType(metadataLoadContext));
            }
            return typeArguments.ToArray();
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return SharedUtilities.GetCustomAttributesData(ctor, metadataLoadContext);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotSupportedException();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotSupportedException();
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            throw new NotImplementedException();
        }

        public override ParameterInfo[] GetParameters()
        {
            List<ParameterInfo> parameters = default;

            foreach (var p in ctor.Parameters)
            {
                parameters ??= new();
                parameters.Add(p.AsParameterInfo(metadataLoadContext));
            }
            return parameters?.ToArray() ?? Array.Empty<ParameterInfo>();
        }

        public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }
    }
}
