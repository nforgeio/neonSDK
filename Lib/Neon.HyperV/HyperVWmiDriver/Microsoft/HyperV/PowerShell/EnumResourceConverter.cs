using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using Microsoft.HyperV.PowerShell.Common;

namespace Microsoft.HyperV.PowerShell;

internal class EnumResourceConverter<T> : TypeConverter where T : struct
{
    private readonly string m_TypeName = typeof(T).Name;

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

    protected virtual string GetResourcePath(T value)
    {
        string arg = value.ToString();
        return string.Format(CultureInfo.InvariantCulture, "{0}_{1}", m_TypeName, arg);
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
                foreach (T value2 in Enum.GetValues(typeof(T)))
                {
                    if (text == m_ResourceManager.GetString(GetResourcePath(value2), culture))
                    {
                        return value2;
                    }
                }
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, EnumValues.Error_CannotConvertValue, text));
            }
            return Enum.Parse(typeof(T), text);
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
        if (destinationType != null && destinationType == typeof(string) && value is T)
        {
            if (culture == null)
            {
                culture = CultureInfo.CurrentUICulture;
            }
            if (culture != CultureInfo.InvariantCulture)
            {
                string resourcePath = GetResourcePath((T)value);
                return m_ResourceManager.GetString(resourcePath, culture);
            }
            return Enum.GetName(typeof(T), value);
        }
        return base.ConvertTo(context, culture, value, destinationType);
    }

    public new string ConvertToString(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        return (string)ConvertTo(context, culture, value, typeof(string));
    }
}
