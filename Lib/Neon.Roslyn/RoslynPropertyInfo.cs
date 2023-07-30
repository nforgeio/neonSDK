//-----------------------------------------------------------------------------
// FILE:        RoslynPropertyInfo.cs
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
    public class RoslynPropertyInfo : PropertyInfo
    {
        private readonly IPropertySymbol     property;
        private readonly MetadataLoadContext metadataLoadContext;

        public RoslynPropertyInfo(IPropertySymbol property, MetadataLoadContext metadataLoadContext)
        {
            this.property            = property;
            this.metadataLoadContext = metadataLoadContext;
        }

        public IPropertySymbol PropertySymbol => property;

        public override PropertyAttributes Attributes => PropertyAttributes.None;

        public override IEnumerable<CustomAttributeData> CustomAttributes 
        {
            get
            {
                return SharedUtilities.GetCustomAttributesData(property, metadataLoadContext);
            }
        }


        public override bool CanRead => property.GetMethod != null;

        public override bool CanWrite => property.SetMethod != null;

        public override Type PropertyType => property.Type.AsType(metadataLoadContext);

        public override Type DeclaringType => property.ContainingType.AsType(metadataLoadContext);

        public override string Name => property.Name;

        public override Type ReflectedType => throw new NotImplementedException();

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotSupportedException();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotSupportedException();
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return property.GetMethod.AsMethodInfo(metadataLoadContext);
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            List<ParameterInfo> parameters = default;

            foreach (var p in property.Parameters)
            {
                parameters ??= new();
                parameters.Add(p.AsParameterInfo(metadataLoadContext));
            }

            return parameters?.ToArray() ?? Array.Empty<ParameterInfo>();
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return property.SetMethod.AsMethodInfo(metadataLoadContext);
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
