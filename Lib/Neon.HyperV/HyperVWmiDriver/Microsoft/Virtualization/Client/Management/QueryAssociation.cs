#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class QueryAssociation : Association
{
    private readonly string m_Query;

    private readonly string m_Namespace;

    private readonly int m_Hashcode;

    private QueryAssociation(string wmiNamespace, string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            throw new ArgumentException("className can not be null or empty.");
        }
        if (wmiNamespace == null)
        {
            throw new ArgumentNullException("wmiNamespace");
        }
        m_Query = query;
        m_Hashcode = m_Query.GetHashCode();
        m_Namespace = wmiNamespace;
    }

    public static QueryAssociation CreateFromClassName(string wmiNamespace, string className)
    {
        if (className == null)
        {
            throw new ArgumentNullException("className");
        }
        string query = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM {0}", className.ToUpperInvariant());
        return CreateFromQuery(wmiNamespace, query);
    }

    public static QueryAssociation CreateFromQuery(string wmiNamespace, string query)
    {
        return new QueryAssociation(wmiNamespace, query);
    }

    protected override IEnumerable<ICimInstance> GetRelatedObjectsSelf(Server server, WmiObjectPath wmiObjectPath, WmiOperationOptions options)
    {
        VMTrace.TraceWmiQueryAssociation("Start WMI query...", m_Query);
        return server.QueryInstances(m_Namespace, m_Query, options?.CimOperationOptions);
    }

    public override bool Equals(object obj)
    {
        if (this == obj)
        {
            return true;
        }
        if (obj is QueryAssociation queryAssociation)
        {
            if (string.Equals(m_Query, queryAssociation.m_Query, StringComparison.OrdinalIgnoreCase))
            {
                return string.Equals(m_Namespace, queryAssociation.m_Namespace, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return m_Hashcode;
    }

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
    private void ContractInvariant()
    {
    }
}
