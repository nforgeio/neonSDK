//-----------------------------------------------------------------------------
// FILE:        RoslynFieldInfo.cs
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
    public class RoslynFieldInfo : FieldInfo
    {
        private readonly IFieldSymbol        field;
        private readonly MetadataLoadContext metadataLoadContext;
        private FieldAttributes?             attributes;

        public RoslynFieldInfo(IFieldSymbol parameter, MetadataLoadContext metadataLoadContext)
        {
            this.field               = parameter;
            this.metadataLoadContext = metadataLoadContext;
        }

        public IFieldSymbol FieldSymbol => field;

        public override FieldAttributes Attributes
        {
            get
            {
                if (!attributes.HasValue)
                {
                    attributes = default(FieldAttributes);

                    if (field.IsStatic)
                    {
                        attributes |= FieldAttributes.Static;
                    }

                    if (field.IsReadOnly)
                    {
                        attributes |= FieldAttributes.InitOnly;
                    }

                    switch (field.DeclaredAccessibility)
                    {
                        case Accessibility.Public:
                            attributes |= FieldAttributes.Public;
                            break;
                        case Accessibility.Private:
                            attributes |= FieldAttributes.Private;
                            break;
                        case Accessibility.Protected:
                            attributes |= FieldAttributes.Family;
                            break;
                    }
                }

                return attributes.Value;
            }
        }

        public override RuntimeFieldHandle FieldHandle => throw new NotSupportedException();

        public override Type FieldType => field.Type.AsType(metadataLoadContext);

        public override Type DeclaringType => field.ContainingType.AsType(metadataLoadContext);

        public override string Name => field.Name;

        public override Type ReflectedType => throw new NotImplementedException();

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotSupportedException();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotSupportedException();
        }

        public override object GetValue(object obj)
        {
            throw new NotSupportedException();
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return SharedUtilities.GetCustomAttributesData(field, metadataLoadContext);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotSupportedException();
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override string ToString() => field.ToString();
    }
}
