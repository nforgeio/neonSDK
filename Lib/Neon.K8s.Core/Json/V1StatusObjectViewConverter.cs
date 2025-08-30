// -----------------------------------------------------------------------------
// FILE:	    V1StatusObjectViewConverter.cs
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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace k8s.Models
{
    internal sealed class V1StatusConverter : JsonConverter<V1Status>
    {
        public override V1Status Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var obj = JsonElement.ParseValue(ref reader);

            try
            {
                return obj.Deserialize<V1Status>();
            }
            catch (JsonException)
            {
                // should be an object
            }

            var status       = new V1Status();
            var originalProp = typeof(V1Status).GetProperty("_original", System.Reflection.BindingFlags.NonPublic);

            originalProp.SetValue(status, obj);

            var hasObjectProp = typeof(V1Status).GetProperty("HasObject", System.Reflection.BindingFlags.Public);

            hasObjectProp.SetValue(status, true);

            return status;
        }

        public override void Write(Utf8JsonWriter writer, V1Status value, JsonSerializerOptions options)
        {
            if (value.HasObject)
            {
                var originalProp = typeof(V1Status).GetProperty("_original", System.Reflection.BindingFlags.NonPublic);
                var obj          = (JsonElement)originalProp.GetValue(value);

                writer.WriteRawValue(JsonSerializer.Serialize(obj, options: options));
            }
            else
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }
}
