// -----------------------------------------------------------------------------
// FILE:	    JFuncConverter.cs
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
using System.Text.Json;
using System.Text.Json.Serialization;

using Neon.ECharts.Options;

namespace Neon.ECharts
{
    public class JFuncConverter : JsonConverter<JFunc>
    {
        /// <summary>
        /// Determines whether the specified type can be converted to a JFunc.
        /// </summary>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <returns><c>true</c> if the specified type can be converted to a JFunc; otherwise, <c>false</c>.</returns>
        public override bool CanConvert(Type typeToConvert) =>
             typeof(JFunc).IsAssignableFrom(typeToConvert);

        /// <summary>
        /// Reads the JSON value and converts it to a JFunc object.
        /// </summary>
        /// <param name="reader">The reader to extract the JSON value from.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>The converted JFunc object.</returns>
        public override JFunc Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes the JFunc object as a raw JSON value.
        /// </summary>
        /// <param name="writer">The writer to write the JSON value to.</param>
        /// <param name="value">The JFunc object to write.</param>
        /// <param name="options">The serializer options.</param>
        public override void Write(Utf8JsonWriter writer, JFunc value, JsonSerializerOptions options)
        {
            writer.WriteRawValue(value.RAW, true);
        }
    }
}