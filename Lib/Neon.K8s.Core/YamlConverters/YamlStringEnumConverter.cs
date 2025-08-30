// -----------------------------------------------------------------------------
// FILE:	    EnumMemberConverter.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2025 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
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
using System.Runtime.Serialization;
using System.Text;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Neon.Kubernetes.Core.YamlConverters
{
    /// <summary>
    /// Converts enum values to and from strings using the <see cref="EnumMemberAttribute"/>.
    /// </summary>
    public class YamlStringEnumConverter : IYamlTypeConverter
    {
        /// <inheritdoc/>
        public bool Accepts(Type type) => type.IsEnum;

        /// <inheritdoc/>
        public object ReadYaml(IParser parser, Type type, ObjectDeserializer deserializer)
        {
            var parsedEnum         = parser.Consume<Scalar>();
            var serializableValues = type.GetMembers()
                .Select(m => new KeyValuePair<string, MemberInfo>(m.GetCustomAttributes<EnumMemberAttribute>(true).Select(ema => ema.Value).FirstOrDefault(), m))
                .Where(pa => !string.IsNullOrEmpty(pa.Key)).ToDictionary(pa => pa.Key, pa => pa.Value);

            if (!serializableValues.ContainsKey(parsedEnum.Value))
            {
                throw new YamlException(parsedEnum.Start, parsedEnum.End, $"Value '{parsedEnum.Value}' not found in enum '{type.Name}'");
            }

            return Enum.Parse(type, serializableValues[parsedEnum.Value].Name);
        }

        /// <inheritdoc/>
        public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
        {
            var enumMember = type.GetMember(value.ToString()).FirstOrDefault();
            var yamlValue  = enumMember?.GetCustomAttributes<EnumMemberAttribute>(true).Select(ema => ema.Value).FirstOrDefault() ?? value.ToString();

            emitter.Emit(new Scalar(yamlValue));
        }
    }
}
