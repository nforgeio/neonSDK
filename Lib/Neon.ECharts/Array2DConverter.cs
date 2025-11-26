// -----------------------------------------------------------------------------
// FILE:	    Array2DConverter.cs
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
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Neon.ECharts
{
    /// <summary>
    /// Converts a two-dimensional array to and from JSON using the specified value converter.
    /// </summary>
    public class Array2DConverter : JsonConverterFactory
    {
        /// <summary>
        /// Determines whether the specified type can be converted to a two-dimensional array.
        /// </summary>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <returns><c>true</c> if the specified type can be converted to a two-dimensional array; otherwise, <c>false</c>.</returns>
        public override bool CanConvert(Type typeToConvert) => typeToConvert.IsArray && typeToConvert.GetArrayRank() == 2;

        /// <summary>
        /// Creates a converter for the specified type.
        /// </summary>
        /// <param name="type">The type to convert.</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>A converter for the specified type.</returns>
        public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options) =>
            (JsonConverter)Activator.CreateInstance(
                typeof(Array2DConverterInner<>).MakeGenericType(new[] { type.GetElementType() }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { options },
                culture: null);

        class Array2DConverterInner<T> : JsonConverter<T[,]>
        {
            readonly JsonConverter<T> _valueConverter;

            public Array2DConverterInner(JsonSerializerOptions options) =>
                this._valueConverter = (typeof(T) == typeof(object) ? null : (JsonConverter<T>)options.GetConverter(typeof(T)));

            /// <summary>
            /// Writes the JSON representation of the two-dimensional array.
            /// </summary>
            /// <param name="writer">The writer to write to.</param>
            /// <param name="array">The two-dimensional array to write.</param>
            /// <param name="options">The serializer options.</param>
            public override void Write(Utf8JsonWriter writer, T[,] array, JsonSerializerOptions options)
            {
                var rowsFirstIndex = array.GetLowerBound(0);
                var rowsLastIndex = array.GetUpperBound(0);
                var columnsFirstIndex = array.GetLowerBound(1);
                var columnsLastIndex = array.GetUpperBound(1);

                writer.WriteStartArray();
                for (var i = rowsFirstIndex; i <= rowsLastIndex; i++)
                {
                    writer.WriteStartArray();
                    for (var j = columnsFirstIndex; j <= columnsLastIndex; j++)
                        _valueConverter.WriteOrSerialize(writer, array[i, j], options);
                    writer.WriteEndArray();
                }
                writer.WriteEndArray();
            }

            /// <summary>
            /// Reads the JSON representation of the two-dimensional array.
            /// </summary>
            /// <param name="reader">The reader to read from.</param>
            /// <param name="typeToConvert">The type to convert.</param>
            /// <param name="options">The serializer options.</param>
            /// <returns>The deserialized two-dimensional array.</returns>
            public override T[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                JsonSerializer.Deserialize<List<List<T>>>(ref reader, options)?.To2D();
        }
    }
}

