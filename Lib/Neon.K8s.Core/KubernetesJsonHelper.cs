// -----------------------------------------------------------------------------
// FILE:	    KubernetesJsonHelper.cs
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
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml;

using k8s;
using k8s.Models;

namespace Neon.K8s
{
    /// <summary>
    /// JSON/Kubernetes related utilities.
    /// </summary>
    public static class KubernetesJsonHelper
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions();

        private sealed class Iso8601TimeSpanConverter : JsonConverter<TimeSpan>
        {
            /// <inheritdoc/>
            public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return XmlConvert.ToTimeSpan(reader.GetString());
            }

            /// <inheritdoc/>
            public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
            {
                // XmlConvert for TimeSpan uses ISO8601, so delegate serialization to it

                writer.WriteStringValue(XmlConvert.ToString(value));
            }
        }

        private sealed class KubernetesDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
        {
            private const string RFC3339MicroFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.ffffffK";
            private const string RFC3339NanoFormat  = "yyyy-MM-dd'T'HH':'mm':'ss.fffffffK";
            private const string RFC3339Format      = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK";

            /// <inheritdoc/>
            public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var str = reader.GetString();

                if (DateTimeOffset.TryParseExact(str, new[] { RFC3339Format, RFC3339MicroFormat }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                {
                    return result;
                }

                // try RFC3339NanoLenient by trimming 1-9 digits to 7 digits

                var originalstr = str;

                str = Regex.Replace(str, @"\.\d+", m => (m.Value + "000000000").Substring(0, 7 + 1)); // 7 digits + 1 for the dot

                if (DateTimeOffset.TryParseExact(str, new[] { RFC3339NanoFormat }, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                {
                    return result;
                }

                throw new FormatException($"Unable to parse {originalstr} as RFC3339 RFC3339Micro or RFC3339Nano");
            }

            /// <inheritdoc/>
            public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString(RFC3339MicroFormat));
            }
        }

        private sealed class KubernetesDateTimeConverter : JsonConverter<DateTime>
        {
            private static readonly JsonConverter<DateTimeOffset> UtcConverter = new KubernetesDateTimeOffsetConverter();

            /// <inheritdoc/>
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return UtcConverter.Read(ref reader, typeToConvert, options).UtcDateTime;
            }

            /// <inheritdoc/>
            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                UtcConverter.Write(writer, value, options);
            }
        }

        /// <summary>
        /// Static onstructor.
        /// </summary>
        static KubernetesJsonHelper()
        {
            JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            JsonSerializerOptions.PropertyNamingPolicy   = JsonNamingPolicy.CamelCase;
            JsonSerializerOptions.DictionaryKeyPolicy    = JsonNamingPolicy.CamelCase;

            JsonSerializerOptions.Converters.Add(new Iso8601TimeSpanConverter());
            JsonSerializerOptions.Converters.Add(new KubernetesDateTimeConverter());
            JsonSerializerOptions.Converters.Add(new KubernetesDateTimeOffsetConverter());
            JsonSerializerOptions.Converters.Add(new V1StatusConverter());
            JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        /// <summary>
        /// Configures <see cref="JsonSerializerOptions"/> for the <see cref="JsonSerializer"/>.
        /// To override existing converters, add them to the top of the <see cref="JsonSerializerOptions.Converters"/> list
        /// e.g. as follows: <code>options.Converters.Insert(index: 0, new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));</code>
        /// </summary>
        /// <param name="configure">An <see cref="Action"/> to configure the <see cref="JsonSerializerOptions"/>.</param>
        public static void AddJsonOptions(Action<JsonSerializerOptions> configure)
        {
            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            configure(JsonSerializerOptions);
        }

        /// <summary>
        /// Deserializes a JSON string to a value of type <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to deserialize.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="jsonSerializerOptions">The optional <see cref="JsonSerializerOptions"/> to use for deserialization.</param>
        /// <returns>The deserialized value of type <typeparamref name="TValue"/>.</returns>
        public static TValue JsonDeserialize<TValue>(string json, JsonSerializerOptions jsonSerializerOptions = null)
        {
            return JsonSerializer.Deserialize<TValue>(json, jsonSerializerOptions ?? JsonSerializerOptions);
        }

        /// <summary>
        /// Deserializes a JSON stream to a value of type <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to deserialize.</typeparam>
        /// <param name="json">The JSON stream to deserialize.</param>
        /// <param name="jsonSerializerOptions">The optional <see cref="JsonSerializerOptions"/> to use for deserialization.</param>
        /// <returns>The deserialized value of type <typeparamref name="TValue"/>.</returns>
        public static TValue JsonDeserialize<TValue>(Stream json, JsonSerializerOptions jsonSerializerOptions = null)
        {
            return JsonSerializer.Deserialize<TValue>(json, jsonSerializerOptions ?? JsonSerializerOptions);
        }

        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="jsonSerializerOptions">The optional <see cref="JsonSerializerOptions"/> to use for serialization.</param>
        /// <returns>The JSON string representation of the object.</returns>
        public static string JsonSerialize(object value, JsonSerializerOptions jsonSerializerOptions = null)
        {
            return JsonSerializer.Serialize(value, jsonSerializerOptions ?? JsonSerializerOptions);
        }

        /// <summary>
        /// Creates a deep copy of an object by serializing and deserializing it.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="value">The object to clone.</param>
        /// <param name="jsonSerializerOptions">The optional <see cref="JsonSerializerOptions"/> to use for serialization and deserialization.</param>
        /// <returns>A deep copy of the object.</returns>
        public static T JsonClone<T>(T value, JsonSerializerOptions jsonSerializerOptions = null)
        {
            return JsonDeserialize<T>(
                json: JsonSerializer.Serialize(value, jsonSerializerOptions ?? JsonSerializerOptions),
                jsonSerializerOptions: jsonSerializerOptions ?? JsonSerializerOptions);
        }

        /// <summary>
        /// Compares two objects by serializing them to JSON and checking for equality.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c>.</returns>
        public static bool JsonEquals<T>(this T x, T y)
        {
            return JsonSerialize(x) == JsonSerialize(y);
        }
    }
}
