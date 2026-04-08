// -----------------------------------------------------------------------------
// FILE:	    KubernetesYamlHelper.cs
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using k8s;
using k8s.Models;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Neon.K8s
{
    /// <summary>
    /// YAML/Kubernetes related utilities.
    /// </summary>
    internal static class KubernetesYaml
    {
        private static DeserializerBuilder CommonDeserializerBuilder =>
            new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new IntOrStringYamlConverter())
                .WithTypeConverter(new ByteArrayStringYamlConverter())
                .WithTypeConverter(new ResourceQuantityYamlConverter())
                .WithOverridesFromJsonPropertyAttributes();

        private static IDeserializer GetDeserializer(bool strict, bool stringTypeDeserialization = true)
        {
            var builder = CommonDeserializerBuilder;

            if (strict)
            {
                builder = builder.WithDuplicateKeyChecking();
            }
            else
            {
                builder = builder.IgnoreUnmatchedProperties();
            }

            if (stringTypeDeserialization)
            {
                builder = builder.WithAttemptingUnquotedStringTypeDeserialization();
            }

            return builder.Build();
        }

        private static readonly IValueSerializer Serializer;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static KubernetesYaml()
        {
            // $note(jefflill):
            //
            // We're going to initialize the field initialization here so it'll
            // be easier to debug exceptions.

            Serializer = new SerializerBuilder()
                .DisableAliases()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new IntOrStringYamlConverter())
                .WithTypeConverter(new ByteArrayStringYamlConverter())
                .WithTypeConverter(new YamlStringEnumConverter())
                .WithTypeConverter(new ResourceQuantityYamlConverter())
                .WithEventEmitter(e => new StringQuotingEmitter(e))
                .WithEventEmitter(e => new FloatEmitter(e))
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .WithOverridesFromJsonPropertyAttributes()
                .BuildValueSerializer();
        }

        /// <summary>
        /// Deserialize a YAML string into a Kubernetes object.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="yaml"></param>
        /// <param name="strict"></param>
        /// <param name="stringTypeDeserialization"></param>
        /// <returns></returns>
        public static TValue Deserialize<TValue>(string yaml, bool strict = false, bool stringTypeDeserialization = true)
        {
            using var reader = new StringReader(yaml);

            return GetDeserializer(strict, stringTypeDeserialization).Deserialize<TValue>(new MergingParser(new Parser(reader)));
        }

        /// <summary>
        /// Deserialize a YAML stream into a Kubernetes object.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="yaml"></param>
        /// <param name="strict"></param>
        /// <param name="stringTypeDeserialization"></param>
        /// <returns></returns>
        public static TValue Deserialize<TValue>(Stream yaml, bool strict = false, bool stringTypeDeserialization = true)
        {
            using var reader = new StreamReader(yaml);

            return GetDeserializer(strict, stringTypeDeserialization).Deserialize<TValue>(new MergingParser(new Parser(reader)));
        }

        /// <summary>
        /// Serialize a Kubernetes object into a YAML string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Serialize(object value)
        {
            if (value == null)
            {
                return "";
            }

            var stringBuilder = new StringBuilder();
            var writer        = new StringWriter(stringBuilder);
            var emitter       = new Emitter(writer);

            emitter.Emit(new StreamStart());
            emitter.Emit(new DocumentStart());
            Serializer.SerializeValue(emitter, value, value.GetType());

            return stringBuilder.ToString();
        }

        private static TBuilder WithOverridesFromJsonPropertyAttributes<TBuilder>(this TBuilder builder)
            where TBuilder : BuilderSkeleton<TBuilder>
        {
            // Use VersionInfo from the model namespace as that should be stable.
            // If this is not generated in the future we will get an obvious compiler error.

            var targetNamespace = typeof(VersionInfo).Namespace;

            // Get all the concrete model types from the code generated namespace.

            var types = typeof(KubernetesEntityAttribute).Assembly
                .ExportedTypes
                .Where(type => type.Namespace == targetNamespace && !type.IsInterface && !type.IsAbstract);

            // Map any JsonPropertyAttribute instances to YamlMemberAttribute instances.

            foreach (var type in types)
            {
                foreach (var property in type.GetProperties())
                {
                    var jsonAttribute = property.GetCustomAttribute<JsonPropertyNameAttribute>();

                    if (jsonAttribute == null)
                    {
                        continue;
                    }

                    var yamlAttribute = new YamlMemberAttribute { Alias = jsonAttribute.Name, ApplyNamingConventions = false };

                    builder.WithAttributeOverride(type, property.Name, yamlAttribute);
                }
            }

            return builder;
        }
    }
}
