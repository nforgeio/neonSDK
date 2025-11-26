// -----------------------------------------------------------------------------
// FILE:	    OneOfConverter.cs
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

using OneOf;

namespace Neon.Mapbox
{
    internal class OneOfConverter : JsonConverter<IOneOf>
    {
        public override IOneOf Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return true;
        }

        public override void Write(Utf8JsonWriter writer, IOneOf value, JsonSerializerOptions options)
        {
            if (value.Value == null)
            {
                writer.WriteNullValue();
            }
            else if (value.Value is bool)
            {
                writer.WriteBooleanValue((bool)value.Value);
            }
            else if (value.Value is int)
            {
                writer.WriteNumberValue((int)value.Value);
            }
            else if (value.Value is long)
            {
                writer.WriteNumberValue((long)value.Value);
            }
            else if (value.Value is double)
            {
                writer.WriteNumberValue((double)value.Value);
            }
            else if (value.Value is float)
            {
                writer.WriteNumberValue((float)value.Value);
            }
            else if (value.Value is decimal)
            {
                writer.WriteNumberValue((decimal)value.Value);
            }
            else if (value.Value is DateTime)
            {
                writer.WriteStringValue((DateTime)value.Value);
            }
            else if (value.Value is DateTimeOffset)
            {
                writer.WriteStringValue((DateTimeOffset)value.Value);
            }
            else
            {
                JsonSerializer.Serialize(writer, value.Value);
            }
        }
    }
}
