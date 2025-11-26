// -----------------------------------------------------------------------------
// FILE:	    JsonSerializerExtensions.cs
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

namespace Neon.ECharts
{
    /// <summary>
    /// Extension methods for JsonSerializer.
    /// </summary>
    public static class JsonSerializerExtensions
    {
        /// <summary>
        /// Writes or serializes the specified value using the provided converter, writer, and options.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="converter">The converter to use for writing the value.</param>
        /// <param name="writer">The writer to write the value to.</param>
        /// <param name="value">The value to write or serialize.</param>
        /// <param name="options">The options to use for serialization.</param>
        public static void WriteOrSerialize<T>(this JsonConverter<T> converter, Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (converter != null)
                converter.Write(writer, value, options);
            else
                JsonSerializer.Serialize(writer, value, typeof(T), options);
        }
    }
}
