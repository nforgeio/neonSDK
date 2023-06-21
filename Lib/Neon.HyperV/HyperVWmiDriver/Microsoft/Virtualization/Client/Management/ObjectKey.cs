using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

internal class ObjectKey
{
    private readonly Server m_Server;

    private readonly WmiObjectPath m_Path;

    private readonly int m_Hashcode;

    public Server Server => m_Server;

    public WmiObjectPath ManagementPath => m_Path;

    public string ServerName => m_Path.ServerName;

    public string NamespaceName => m_Path.NamespaceName;

    public string ClassName => m_Path.ClassName;

    public ObjectKey(Server server, WmiObjectPath path)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        WmiNameMapper.MapWmiClassNameToType(path.ClassName, path.CimClass);
        m_Server = server;
        m_Path = path;
        m_Hashcode = m_Path.GetHashCode();
    }

    protected ObjectKey(ObjectKey key)
    {
        m_Server = key.Server;
        m_Path = key.m_Path;
        m_Hashcode = key.m_Hashcode;
    }

    public override int GetHashCode()
    {
        return m_Hashcode;
    }

    public override bool Equals(object obj)
    {
        if (this == obj)
        {
            return true;
        }
        if (obj is ObjectKey objectKey)
        {
            return m_Path.Equals(objectKey.m_Path);
        }
        return false;
    }

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
    private void ContractInvariant()
    {
    }
}
