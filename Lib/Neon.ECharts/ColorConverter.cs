// -----------------------------------------------------------------------------
// FILE:	    ColorConverter.cs
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
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Neon.ECharts
{
    public class ColorConverter : JsonConverter<Color>
    {
        /// <summary>
        /// Determines whether the specified type can be converted to a Color.
        /// </summary>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <returns><c>true</c> if the specified type can be converted to a Color; otherwise, <c>false</c>.</returns>
        public override bool CanConvert(Type typeToConvert) =>
             typeof(Color).IsAssignableFrom(typeToConvert);

        /// <summary>
        /// Reads the JSON value and converts it to a Color object.
        /// </summary>
        /// <param name="reader">The reader to extract the JSON value from.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>The converted Color object.</returns>
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }

            var stringValue = reader.GetString();

            if (stringValue.StartsWith("#"))
            {
                return ColorTranslator.FromHtml(reader.GetString());
            }

            if (stringValue.StartsWith("rgba"))
            {
                var numbers = Regex.Matches(stringValue, @"[\d\.]+");
                var a = double.Parse(numbers[3].Value);
                var r = int.Parse(numbers[0].Value);
                var g = int.Parse(numbers[1].Value);
                var b = int.Parse(numbers[2].Value);

                return Color.FromArgb((int)(a * 255.0), r, g, b);
            }

            throw new JsonException();
        }

        /// <summary>
        /// Writes the Color object as a raw JSON value.
        /// </summary>
        /// <param name="writer">The writer to write the JSON value to.</param>
        /// <param name="value">The Color object to write.</param>
        /// <param name="options">The serializer options.</param>
        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteStringValue($"rgba({(int)value.R}, {(int)value.G}, {(int)value.B}, {((int)value.A) / 255.0})");
        }
    }
}