// -----------------------------------------------------------------------------
// FILE:	    EChartsOptionSerializer.cs
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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Neon.ECharts.Options
{
    internal class EChartsOptionSerializer
    {
        /// <summary>
        /// Default instance of the EChartsOptionSerializer.
        /// </summary>
        public static readonly EChartsOptionSerializer Default = new();

        private readonly JsonSerializerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="EChartsOptionSerializer"/> class.
        /// </summary>
        private EChartsOptionSerializer()
        {
            _options = new()
            {
                WriteIndented          = true,
                PropertyNamingPolicy   = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters =
                    {
                        new SeriesConverter(),
                        new FormatterConverter(),
                        new JFuncConverter(),
                        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                        new Array2DConverter(),
                        new ColorConverter()
                    },
                IncludeFields = true,
            };
        }

        /// <summary>
        /// Serializes the specified option to a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of the option.</typeparam>
        /// <param name="option">The option to serialize.</param>
        /// <returns>The JSON string representation of the option.</returns>
        public string Serialize<T>(T option)
        {
            return JsonSerializer.Serialize(option, _options);
        }

        /// <summary>
        /// Deserializes the specified JSON string to an option object.
        /// </summary>
        /// <typeparam name="T">The type of the option.</typeparam>
        /// <param name="value">The JSON string to deserialize.</param>
        /// <returns>The deserialized option object.</returns>
        public T Deserialize<T>(string value)
        {
            return JsonSerializer.Deserialize<T>(value, _options);
        }
    }
}