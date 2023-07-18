//-----------------------------------------------------------------------------
// FILE:        RoslynParameterInfo.cs
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
using System.Reflection;

using Microsoft.CodeAnalysis;

namespace Neon.Roslyn
{
    public class RoslynParameterInfo : ParameterInfo
    {
        private readonly IParameterSymbol    _parameter;
        private readonly MetadataLoadContext _metadataLoadContext;

        public RoslynParameterInfo(IParameterSymbol parameter, MetadataLoadContext metadataLoadContext)
        {
            _parameter           = parameter;
            _metadataLoadContext = metadataLoadContext;
        }

        public IParameterSymbol ParameterSymbol => _parameter;

        public override Type ParameterType => _parameter.Type.AsType(_metadataLoadContext);
        public override string Name => _parameter.Name;
        public override bool HasDefaultValue => _parameter.HasExplicitDefaultValue;

        public override object DefaultValue => HasDefaultValue ? _parameter.ExplicitDefaultValue : null;

        public override int Position => _parameter.Ordinal;

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return SharedUtilities.GetCustomAttributesData(_parameter, _metadataLoadContext);
        }

        public override string ToString() => _parameter.ToString();
    }
}
