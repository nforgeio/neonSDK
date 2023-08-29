//-----------------------------------------------------------------------------
// FILE:        RoslynExtensions.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis;

namespace Neon.Roslyn
{
    public static class RoslynExtensions
    {
        public static IMethodSymbol GetMethodSymbol(this MethodInfo methodInfo) => (methodInfo as RoslynMethodInfo)?.MethodSymbol;

        public static IPropertySymbol GetPropertySymbol(this PropertyInfo property) => (property as RoslynPropertyInfo)?.PropertySymbol;
        public static IFieldSymbol GetFieldSymbol(this FieldInfo field) => (field as RoslynFieldInfo)?.FieldSymbol;

        public static IParameterSymbol GetParameterSymbol(this ParameterInfo parameterInfo) => (parameterInfo as RoslynParameterInfo)?.ParameterSymbol;

        public static ITypeSymbol GetTypeSymbol(this Type type) => (type as RoslynType)?.TypeSymbol;

        public static T GetCustomAttribute<T>(this PropertyInfo prop)
        {
            var attributeData = prop.CustomAttributes.Where(ca => ca.AttributeType.FullName == typeof(T).FullName).FirstOrDefault();

            return attributeData.GetCustomAttribute<T>();
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this PropertyInfo prop, bool inherited = false)
        {
            return prop.CustomAttributes
                .Where(ca => ca.AttributeType.FullName == typeof(T).FullName)
                .Select(attr => attr.GetCustomAttribute<T>());
        }

        public static T GetCustomAttribute<T>(this Type type)
        {
            var attributeData = type.CustomAttributes.Where(ca => ca.AttributeType.FullName == typeof(T).FullName).FirstOrDefault();

            return attributeData.GetCustomAttribute<T>();
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this Type type, bool inherited = false)
        {
            return type.CustomAttributes
                .Where(ca => ca.AttributeType.FullName == typeof(T).FullName)
                .Select(attr => attr.GetCustomAttribute<T>());
        }

        public static T GetCustomAttribute<T>(this CustomAttributeData attributeData)
        {
            if (attributeData == null)
            {
                return default(T);
            }

            T attribute;

            if (attributeData.ConstructorArguments.Count > 0 && attributeData.Constructor != null)
            {
                var actualArgs = attributeData.GetActualConstuctorParams().ToArray();
                attribute = (T)Activator.CreateInstance(typeof(T), actualArgs);
            }
            else
            {
                attribute = (T)Activator.CreateInstance(typeof(T));
            }
            foreach (var p in attributeData.NamedArguments)
            {
                var propertyInfo = typeof(T).GetProperty(p.MemberInfo.Name);
                if (propertyInfo != null)
                {
                    propertyInfo.SetValue(attribute, p.TypedValue.Value);
                    continue;
                }

                var fieldInfo = typeof(T).GetField(p.MemberInfo.Name);
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(attribute, p.TypedValue.Value);
                    continue;
                }

                throw new Exception($"No field or property {p}");
            }
            return attribute;
        }
    }

    internal static class RoslynInternalExtensions
    {
        public static Assembly AsAssembly(this IAssemblySymbol assemblySymbol, MetadataLoadContext metadataLoadContext) => metadataLoadContext.GetOrCreate<Assembly>(assemblySymbol);

        public static Type AsType(this ITypeSymbol typeSymbol, MetadataLoadContext metadataLoadContext) => metadataLoadContext.GetOrCreate<Type>(typeSymbol);

        public static ParameterInfo AsParameterInfo(this IParameterSymbol parameterSymbol, MetadataLoadContext metadataLoadContext) => metadataLoadContext.GetOrCreate<ParameterInfo>(parameterSymbol);

        public static ConstructorInfo AsConstructorInfo(this IMethodSymbol methodSymbol, MetadataLoadContext metadataLoadContext) => metadataLoadContext.GetOrCreate<ConstructorInfo>(methodSymbol);

        public static MethodInfo AsMethodInfo(this IMethodSymbol methodSymbol, MetadataLoadContext metadataLoadContext) => metadataLoadContext.GetOrCreate<MethodInfo>(methodSymbol);

        public static PropertyInfo AsPropertyInfo(this IPropertySymbol propertySymbol, MetadataLoadContext metadataLoadContext) => metadataLoadContext.GetOrCreate<PropertyInfo>(propertySymbol);

        public static FieldInfo AsFieldInfo(this IFieldSymbol fieldSymbol, MetadataLoadContext metadataLoadContext) => metadataLoadContext.GetOrCreate<FieldInfo>(fieldSymbol);

        public static IEnumerable<ITypeSymbol> BaseTypes(this ITypeSymbol typeSymbol)
        {
            var t = typeSymbol;
            while (t != null)
            {
                yield return t;
                t = t.BaseType;
            }
        }

        private const string SourceItemGroupMetadata = "build_metadata.AdditionalFiles.SourceItemGroup";

        public static string GetMSBuildProperty(
            this GeneratorExecutionContext context,
            string name,
            string defaultValue = "")
        {
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue($"build_property.{name}", out var value);
            return value ?? defaultValue;
        }

        public static string[] GetMSBuildItems(this GeneratorExecutionContext context, string name)
            => context
                .AdditionalFiles
                .Where(f => context.AnalyzerConfigOptions
                    .GetOptions(f)
                    .TryGetValue(SourceItemGroupMetadata, out var sourceItemGroup)
                    && sourceItemGroup == name)
                .Select(f => f.Path)
                .ToArray();
    }
}
