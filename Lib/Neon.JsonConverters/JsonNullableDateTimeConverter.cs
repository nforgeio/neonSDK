//-----------------------------------------------------------------------------
// FILE:        JsonNullableDateTimeConverter.cs
// CONTRIBUTOR: Marcus Bowyer
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
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Neon.Common;

namespace Neon.JsonConverters
{
    /// <summary>
    /// Converts <see cref="Nullable{DateTime}"/> values for <see cref="System.Text.Json"/> based serialization.
    /// </summary>
    public class JsonNullableDateTimeConverter : JsonConverter<DateTime?>
    {
        private const string dateFormat = "yyyy-MM-ddTHH:mm:ssZ";

        /// <inheritdoc/>
        public override DateTime? Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
        {
            Covenant.Requires<ArgumentException>(type == typeof(DateTime?), nameof(type));

            reader.GetString();

            var input = reader.GetString();

            if (input == "null")
            {
                return null;
            }
            else
            {
                return DateTime.ParseExact(input, dateFormat, CultureInfo.InvariantCulture).ToUniversalTime();
            }
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToString(dateFormat));
            }
            else
            {
                writer.WriteRawValue("null");
            }
        }
    }
}
