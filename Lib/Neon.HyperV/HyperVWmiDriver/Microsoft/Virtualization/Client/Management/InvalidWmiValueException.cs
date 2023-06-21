using System;

namespace Microsoft.Virtualization.Client.Management;

internal class InvalidWmiValueException : VirtualizationManagementException
{
    private string m_ValueName = string.Empty;

    private Type m_ValueType;

    private object m_Value;

    public string ValueName
    {
        get
        {
            return m_ValueName;
        }
        set
        {
            m_ValueName = value;
        }
    }

    public Type ValueType
    {
        get
        {
            return m_ValueType;
        }
        set
        {
            m_ValueType = value;
        }
    }

    public object Value
    {
        get
        {
            return m_Value;
        }
        set
        {
            m_Value = value;
        }
    }

    public InvalidWmiValueException()
    {
    }

    public InvalidWmiValueException(string message)
        : base(message)
    {
    }

    public InvalidWmiValueException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public InvalidWmiValueException(string valueName, Type valueType, object value, string message)
        : this(valueName, valueType, value, message, null)
    {
    }

    public InvalidWmiValueException(string valueName, Type valueType, object value, string message, Exception innerException)
        : base(message, innerException)
    {
        m_ValueName = valueName;
        m_ValueType = valueType;
        m_Value = value;
    }
}
