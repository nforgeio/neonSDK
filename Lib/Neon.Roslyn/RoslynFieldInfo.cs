//-----------------------------------------------------------------------------
// FILE:        RoslynFieldInfo.cs
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
    internal class RoslynFieldInfo : FieldInfo
    {
        private readonly IFieldSymbol        _field;
        private readonly MetadataLoadContext _metadataLoadContext;
        private FieldAttributes?             _attributes;

        public RoslynFieldInfo(IFieldSymbol parameter, MetadataLoadContext metadataLoadContext)
        {
            _field               = parameter;
            _metadataLoadContext = metadataLoadContext;
        }

        public IFieldSymbol FieldSymbol => _field;

        public override FieldAttributes Attributes
        {
            get
            {
                if (!_attributes.HasValue)
                {
                    _attributes = default(FieldAttributes);

                    if (_field.IsStatic)
                    {
                        _attributes |= FieldAttributes.Static;
                    }

                    if (_field.IsReadOnly)
                    {
                        _attributes |= FieldAttributes.InitOnly;
                    }

                    switch (_field.DeclaredAccessibility)
                    {
                        case Accessibility.Public:
                            _attributes |= FieldAttributes.Public;
                            break;
                        case Accessibility.Private:
                            _attributes |= FieldAttributes.Private;
                            break;
                        case Accessibility.Protected:
                            _attributes |= FieldAttributes.Family;
                            break;
                    }
                }

                return _attributes.Value;
            }
        }

        public override RuntimeFieldHandle FieldHandle => throw new NotSupportedException();

        public override Type FieldType => _field.Type.AsType(_metadataLoadContext);

        public override Type DeclaringType => _field.ContainingType.AsType(_metadataLoadContext);

        public override string Name => _field.Name;

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
            return SharedUtilities.GetCustomAttributesData(_field, _metadataLoadContext);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotSupportedException();
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override string ToString() => _field.ToString();
    }
}
