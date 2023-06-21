using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class EmbeddedInstance
{
    private Server m_Server;

    private string m_WmiNamespace;

    private string m_WmiClassName;

    private readonly KeyValueDictionary m_Properties = new KeyValueDictionary();

    public Server Server => m_Server;

    public bool IsInitialized { get; private set; }

    protected EmbeddedInstance()
    {
    }

    protected EmbeddedInstance(Server server, string wmiNamespace, string wmiClassName)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        if (wmiNamespace == null)
        {
            throw new ArgumentNullException("wmiNamespace");
        }
        if (wmiClassName == null)
        {
            throw new ArgumentNullException("wmiClassName");
        }
        m_Server = server;
        m_WmiNamespace = wmiNamespace;
        m_WmiClassName = wmiClassName;
        IsInitialized = true;
    }

    public override string ToString()
    {
        return m_Server.GetNewEmbeddedInstance(m_WmiNamespace, m_WmiClassName, m_Properties);
    }

    public static TType ConvertTo<TType>(Server server, string embeddedInstance) where TType : EmbeddedInstance, new()
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        return ConvertTo<TType>(server, server.VirtualizationNamespace, embeddedInstance);
    }

    public static TType ConvertTo<TType>(Server server, string wmiNamespace, string embeddedInstance) where TType : EmbeddedInstance, new()
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        if (string.IsNullOrEmpty(wmiNamespace))
        {
            throw new ArgumentNullException("wmiNamespace");
        }
        WmiNameAttribute wmiNameAttribute = typeof(TType).GetTypeInfo().GetCustomAttributes<WmiNameAttribute>().First();
        return ConvertTo<TType>(server, wmiNamespace, wmiNameAttribute.Name, embeddedInstance);
    }

    private static TType ConvertTo<TType>(Server server, string wmiNamespace, string wmiClassName, string embeddedInstance) where TType : EmbeddedInstance, new()
    {
        TType val = null;
        using ICimInstance cimInstance = server.GetInstanceFromEmbeddedInstance(wmiNamespace, wmiClassName, embeddedInstance);
        val = new TType();
        val.Initialize(server, wmiNamespace, wmiClassName, cimInstance.CimInstanceProperties);
        return val;
    }

    public void Initialize(Server server, string wmiNamespace, string wmiClassName, CimKeyedCollection<CimProperty> cimProperties)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        if (string.IsNullOrEmpty(wmiNamespace))
        {
            throw new ArgumentNullException("wmiNamespace");
        }
        if (string.IsNullOrEmpty(wmiClassName))
        {
            throw new ArgumentNullException("wmiClassName");
        }
        if (cimProperties == null)
        {
            throw new ArgumentNullException("cimProperties");
        }
        m_Server = server;
        m_WmiNamespace = wmiNamespace;
        m_WmiClassName = wmiClassName;
        foreach (CimProperty cimProperty in cimProperties)
        {
            m_Properties.Add(cimProperty.Name, cimProperty.Value);
        }
        IsInitialized = true;
    }

    protected void AddProperty(string name, object value)
    {
        m_Properties.Add(name, value);
    }

    [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "This is a language feature. We should use it.")]
    protected TType GetProperty<TType>(string name, TType defaultValue = default(TType))
    {
        if (!TryGetProperty(name, out var value, defaultValue))
        {
            throw ThrowHelper.CreateClassDefinitionMismatchException(ClassDefinitionMismatchReason.Property, m_WmiClassName, name, null);
        }
        return value;
    }

    [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "This is a language feature. We should use it.")]
    protected bool TryGetProperty<TType>(string name, out TType value, TType defaultValue = default(TType))
    {
        object value2;
        bool num = m_Properties.TryGetValue(name, out value2);
        if (num)
        {
            value = (TType)value2;
            return num;
        }
        value = defaultValue;
        return num;
    }

    protected void SetProperty(string name, object value)
    {
        m_Properties[name] = value;
    }

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
    private void ObjectInvariant()
    {
    }
}
