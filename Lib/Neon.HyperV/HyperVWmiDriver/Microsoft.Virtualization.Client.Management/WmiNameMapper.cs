using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Virtualization.Client.Management;

internal static class WmiNameMapper
{
	private static readonly IReadOnlyDictionary<string, Type> gm_ClassNameToType;

	private static readonly IReadOnlyDictionary<Type, string> gm_TypeToClassName;

	static WmiNameMapper()
	{
		Dictionary<string, Type> dictionary = new Dictionary<string, Type>(50, StringComparer.OrdinalIgnoreCase);
		Dictionary<Type, string> dictionary2 = new Dictionary<Type, string>(50);
		Type[] types = typeof(IVirtualizationManagementObject).GetTypeInfo().Assembly.GetTypes();
		foreach (Type type in types)
		{
			if (!type.GetTypeInfo().IsInterface || !typeof(IVirtualizationManagementObject).IsAssignableFrom(type))
			{
				continue;
			}
			WmiNameAttribute customAttribute = type.GetTypeInfo().GetCustomAttribute<WmiNameAttribute>();
			if (customAttribute == null)
			{
				continue;
			}
			string name = customAttribute.Name;
			bool primaryMapping = customAttribute.PrimaryMapping;
			try
			{
				if (primaryMapping)
				{
					dictionary.Add(name, type);
				}
				dictionary2.Add(type, name);
			}
			catch (ArgumentException)
			{
				throw;
			}
		}
		gm_ClassNameToType = dictionary;
		gm_TypeToClassName = dictionary2;
	}

	internal static Type MapWmiClassNameToType(string wmiName, ICimClass cimClass)
	{
		if (wmiName == null)
		{
			throw new ArgumentNullException("wmiName");
		}
		if (TryMapWmiClassNameToType(wmiName, out var type) || TryResolveCimClassToExtensibleType(cimClass, out type))
		{
			return type;
		}
		throw new NoWmiMappingException("No mapped type found for WMI class: " + wmiName);
	}

	internal static bool TryMapWmiClassNameToType(string wmiName, out Type type)
	{
		return gm_ClassNameToType.TryGetValue(wmiName, out type);
	}

	internal static bool TryResolveCimClassToExtensibleType(ICimClass cimClass, out Type type)
	{
		type = null;
		ICimClass cimClass2 = cimClass;
		while (cimClass2 != null && type == null)
		{
			string cimSuperClassName = cimClass2.CimSuperClassName;
			if (string.IsNullOrEmpty(cimSuperClassName) || TryMapWmiClassNameToType(cimSuperClassName, out type))
			{
				break;
			}
			cimClass2 = cimClass2.CimSuperClass;
		}
		return type != null;
	}

	internal static string MapTypeToWmiClassName(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (gm_TypeToClassName.TryGetValue(type, out var value))
		{
			return value;
		}
		throw new NoWmiMappingException("No WMI class found for type: " + type.Name);
	}
}
