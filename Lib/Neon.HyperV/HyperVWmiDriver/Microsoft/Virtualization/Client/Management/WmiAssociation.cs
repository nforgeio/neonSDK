#define TRACE
using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class WmiAssociation : Association
{
	private readonly string m_AssociationName;

	private readonly string m_RelatedClassName;

	private readonly string m_RelatedRoleName;

	private readonly int m_Hashcode;

	public WmiAssociation(string name)
		: this(name, null, null)
	{
	}

	public WmiAssociation(string name, string relatedClass)
		: this(name, relatedClass, null)
	{
	}

	public WmiAssociation(string name, string relatedClass, string relatedRole)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentException("Name can not be empty or null.");
		}
		m_AssociationName = name;
		m_RelatedClassName = relatedClass;
		m_RelatedRoleName = relatedRole;
		string text = m_AssociationName.ToUpperInvariant();
		if (!string.IsNullOrEmpty(m_RelatedClassName))
		{
			text += m_RelatedClassName.ToUpperInvariant();
		}
		if (!string.IsNullOrEmpty(m_RelatedRoleName))
		{
			text += m_RelatedRoleName.ToUpperInvariant();
		}
		m_Hashcode = text.GetHashCode();
	}

	protected override IEnumerable<ICimInstance> GetRelatedObjectsSelf(Server server, WmiObjectPath wmiObjectPath, WmiOperationOptions options)
	{
		VMTrace.TraceWmiAssociation("Start WMI associator query...", wmiObjectPath.ToString(), m_AssociationName, m_RelatedClassName);
		return server.EnumerateAssociatedInstances(wmiObjectPath, m_AssociationName, m_RelatedClassName, null, m_RelatedRoleName, options?.CimOperationOptions);
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (obj is WmiAssociation wmiAssociation)
		{
			if (string.Equals(m_AssociationName, wmiAssociation.m_AssociationName, StringComparison.OrdinalIgnoreCase) && string.Equals(m_RelatedClassName, wmiAssociation.m_RelatedClassName, StringComparison.OrdinalIgnoreCase))
			{
				return string.Equals(m_RelatedRoleName, wmiAssociation.m_RelatedRoleName, StringComparison.OrdinalIgnoreCase);
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
