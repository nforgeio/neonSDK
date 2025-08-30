// -----------------------------------------------------------------------------
// FILE:	    ByteArrayStringYamlConverter.cs
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
using System.Text;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Neon.K8s.YamlConverters
{
    internal class ByteArrayStringYamlConverter : IYamlTypeConverter
    {
        /// <inheritdoc/>
        public bool Accepts(Type type)
        {
            return type == typeof(byte[]);
        }

        /// <inheritdoc/>
        public object ReadYaml(IParser parser, Type type, ObjectDeserializer deserializer)
        {
            if (parser?.Current is Scalar scalar)
            {
                try
                {
                    if (string.IsNullOrEmpty(scalar.Value))
                    {
                        return null;
                    }

                    return Encoding.UTF8.GetBytes(scalar.Value);
                }
                finally
                {
                    parser.MoveNext();
                }
            }

            throw new InvalidOperationException(parser.Current?.ToString());
        }

        /// <inheritdoc/>
        public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
        {
            var obj = (byte[])value;

            emitter?.Emit(new Scalar(Encoding.UTF8.GetString(obj)));
        }
    }
}
