#define TRACE
using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class WmiRelationship : Association
{
    private readonly string m_AssociationName;

    private readonly string m_SourceRoleName;

    private readonly int m_Hashcode;

    public WmiRelationship(string name)
        : this(name, null)
    {
    }

    public WmiRelationship(string name, string sourceRole)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException("name");
        }
        m_AssociationName = name;
        m_SourceRoleName = sourceRole;
        string text = m_AssociationName.ToUpperInvariant();
        if (!string.IsNullOrEmpty(m_SourceRoleName))
        {
            text += m_SourceRoleName.ToUpperInvariant();
        }
        m_Hashcode = text.GetHashCode();
    }

    protected override IEnumerable<ICimInstance> GetRelatedObjectsSelf(Server server, WmiObjectPath wmiObjectPath, WmiOperationOptions options)
    {
        VMTrace.TraceWmiRelationship("Start WMI associator query...", wmiObjectPath.ToString(), m_AssociationName, m_SourceRoleName);
        return server.EnumerateReferencingInstances(wmiObjectPath, m_AssociationName, m_SourceRoleName, options?.CimOperationOptions);
    }

    public override bool Equals(object obj)
    {
        if (this == obj)
        {
            return true;
        }
        if (obj is WmiRelationship wmiRelationship)
        {
            if (string.Equals(m_AssociationName, wmiRelationship.m_AssociationName, StringComparison.OrdinalIgnoreCase))
            {
                return string.Equals(m_SourceRoleName, wmiRelationship.m_SourceRoleName, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return m_Hashcode;
    }
}
