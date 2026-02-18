//-----------------------------------------------------------------------------
// FILE:        JsonStringEnumMemberConverter.cs
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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.Serialization;

namespace System.Text.Json.Serialization
{
    /// <summary>
    /// Converts enum values to and from strings using the <see cref="EnumMemberAttribute"/>.
    /// </summary>
    public class JsonStringEnumMemberConverter : JsonConverterFactory
	{
		private readonly HashSet<Type>                        enumTypes;
		private readonly JsonStringEnumMemberConverterOptions options;

        /// <summary>
        /// Constructs a default instance of the <see cref="JsonStringEnumMemberConverter"/> class.
        /// </summary>
        public JsonStringEnumMemberConverter()
		{
		}

        /// <summary>
        /// Initializes a new instance of the JsonStringEnumMemberConverter class with the specified naming policy and a
        /// value indicating whether integer values are allowed during serialization and deserialization.
        /// </summary>
        /// <param name="namingPolicy">The policy used to convert enum member names to strings. If null, the default naming policy is used.</param>
        /// <param name="allowIntegerValues">true to allow integer values when reading and writing JSON; otherwise, false. If false, integer values will
        /// cause a JsonException during serialization or deserialization.</param>
		public JsonStringEnumMemberConverter(
            JsonNamingPolicy namingPolicy = null,
            bool allowIntegerValues = true)
			: this(new JsonStringEnumMemberConverterOptions { NamingPolicy = namingPolicy, AllowIntegerValues = allowIntegerValues })
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonStringEnumMemberConverter"/> class with the specified options and target enum types.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="targetEnumTypes"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public JsonStringEnumMemberConverter(JsonStringEnumMemberConverterOptions options, params Type[] targetEnumTypes)
		{
            Covenant.Requires<ArgumentNullException>(options != null, nameof(options));

            this.options = options;

			if (targetEnumTypes != null && targetEnumTypes.Length > 0)
			{
#if NETSTANDARD2_0
				enumTypes = new HashSet<Type>();
#else
				_EnumTypes = new HashSet<Type>(targetEnumTypes.Length);
#endif
				foreach (Type enumType in targetEnumTypes)
				{
					if (enumType.IsEnum)
					{
						enumTypes.Add(enumType);
						continue;
					}

					if (enumType.IsGenericType)
					{
                        Type UnderlyingType = Nullable.GetUnderlyingType(enumType);
                        var  isNullableEnum = UnderlyingType?.IsEnum ?? false;

						if (isNullableEnum)
						{
							enumTypes.Add(UnderlyingType!);
							continue;
						}
					}

					throw new NotSupportedException($"Type {enumType} is not supported by JsonStringEnumMemberConverter.");
				}
			}
		}

		/// <inheritdoc/>
		public override bool CanConvert(Type typeToConvert)
		{
            Covenant.Requires<ArgumentNullException>(typeToConvert != null, nameof(typeToConvert));

            return enumTypes != null
				? enumTypes.Contains(typeToConvert)
				: typeToConvert.IsEnum;
		}

		/// <inheritdoc/>
		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
            var underlyingType = Nullable.GetUnderlyingType(typeToConvert);
            var  isNullableEnum = underlyingType?.IsEnum ?? false;

			try
			{
				return (JsonConverter)Activator.CreateInstance(
					typeof(JsonStringEnumMemberConverter<>).MakeGenericType(isNullableEnum ? underlyingType! : typeToConvert),
                    BindingFlags.Instance | BindingFlags.Public,
					binder: null,
					args: [ this.options ],
					culture: null)!;
			}
			catch (TargetInvocationException targetInvocationEx)
			{
                if (targetInvocationEx.InnerException != null)
                {
                    throw targetInvocationEx.InnerException;
                }
				throw;
			}
		}

		internal static ulong GetEnumValue(TypeCode enumTypeCode, object value)
		{
			return enumTypeCode switch
			{
				TypeCode.Int32 => (ulong)(int)value,
				TypeCode.Int64 => (ulong)(long)value,
				TypeCode.Int16 => (ulong)(short)value,
				TypeCode.Byte => (byte)value,
				TypeCode.UInt32 => (uint)value,
				TypeCode.UInt64 => (ulong)value,
				TypeCode.UInt16 => (ushort)value,
				TypeCode.SByte => (ulong)(sbyte)value,
				_ => throw new NotSupportedException($"Enum '{value}' of {enumTypeCode} type is not supported."),
			};
		}
	}
}
