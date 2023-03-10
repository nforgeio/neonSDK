using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace Microsoft.Virtualization.Client.Management;

internal class EnumResourceConverter<EnumType> : TypeConverter where EnumType : struct
{
	private readonly string m_TypeName = typeof(EnumType).Name;

	private ResourceManager m_ResourceManager;

	public ResourceManager ResourceManager
	{
		get
		{
			return m_ResourceManager;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_ResourceManager = value;
		}
	}

	public EnumResourceConverter()
		: this(EnumValues.ResourceManager)
	{
	}

	public EnumResourceConverter(ResourceManager resourceManager)
	{
		if (resourceManager == null)
		{
			throw new ArgumentNullException("resourceManager");
		}
		ResourceManager = resourceManager;
	}

	protected virtual string GetResourcePath(EnumType value, ResourceContext resourceContext)
	{
		string arg = value.ToString();
		string text = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", m_TypeName, arg);
		if (resourceContext != 0)
		{
			text = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", text, resourceContext.ToString());
		}
		return text;
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType != null && sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string text)
		{
			if (culture != null && culture != CultureInfo.InvariantCulture)
			{
				foreach (EnumType value2 in Enum.GetValues(typeof(EnumType)))
				{
					if (text == m_ResourceManager.GetString(GetResourcePath(value2, ResourceContext.Default), culture))
					{
						return value2;
					}
				}
				throw new NotSupportedException("The value " + text + " could not be converted.");
			}
			return Enum.Parse(typeof(EnumType), text);
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType != null && destinationType == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		return ConvertTo(context, culture, value, ResourceContext.Default, destinationType);
	}

	public string ConvertToString(ITypeDescriptorContext context, CultureInfo culture, object value, ResourceContext resourceContext)
	{
		return (string)ConvertTo(context, culture, value, resourceContext, typeof(string));
	}

	private object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, ResourceContext resourceContext, Type destinationType)
	{
		if (destinationType != null && destinationType == typeof(string) && value is EnumType)
		{
			if (culture != null && culture != CultureInfo.InvariantCulture)
			{
				string resourcePath = GetResourcePath((EnumType)value, resourceContext);
				return m_ResourceManager.GetString(resourcePath, culture);
			}
			return Enum.GetName(typeof(EnumType), value);
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	private void ObjectInvariant()
	{
	}
}
