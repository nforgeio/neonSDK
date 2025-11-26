// -----------------------------------------------------------------------------
// FILE:	    FormatterConverter.cs
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

namespace Neon.ECharts
{
    /// <summary>
    /// Converts the <see cref="Formatter"/> class to and from JSON.
    /// </summary>
    public class FormatterConverter : JsonConverter<Formatter>
    {
        /// <summary>
        /// Determines whether the specified type can be converted to a <see cref="Formatter"/>.
        /// </summary>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <returns><c>true</c> if the specified type can be converted to a <see cref="Formatter"/>; otherwise, <c>false</c>.</returns>
        public override bool CanConvert(Type typeToConvert) =>
             typeof(Formatter).IsAssignableFrom(typeToConvert);

        /// <summary>
        /// Reads and converts the JSON to a <see cref="Formatter"/> object.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>The converted <see cref="Formatter"/> object.</returns>
        public override Formatter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes the <see cref="Formatter"/> object to JSON.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        /// <param name="value">The <see cref="Formatter"/> object to write.</param>
        /// <param name="options">The serializer options.</param>
        public override void Write(Utf8JsonWriter writer, Formatter value, JsonSerializerOptions options)
        {
            if (value.JFunc != null)
            {
                //JsonSerializer.Serialize(writer, value.JFunc, options);
                writer.WriteRawValue(value.JFunc.RAW, true);
            }
            else
            {
                writer.WriteStringValue(value.StringFormat);
            }
        }
    }
}