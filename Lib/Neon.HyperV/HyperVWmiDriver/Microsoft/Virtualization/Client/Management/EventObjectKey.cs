using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Microsoft.Virtualization.Client.Management;

internal class EventObjectKey : ObjectKey
{
	public const string gm_BaseQueryFormat = "SELECT * FROM __{0} WITHIN {1} WHERE TargetInstance ISA '{2}'";

	public const string gm_StringKeyConditionClause = " AND TargetInstance.{0}=\"{1}\"";

	public const string gm_NonStringKeyConditionClause = " AND TargetInstance.{0}={1}";

	public const string gm_AdditionalConditionClause = " AND TargetInstance.";

	private readonly InstanceEventType m_InstanceEventType;

	private readonly string[] m_AdditionalConditions;

	private readonly int m_HashCode;

	private readonly string m_IndexString;

	public InstanceEventType InstanceEventType => m_InstanceEventType;

	public string[] AdditionalConditions => m_AdditionalConditions;

	public EventObjectKey(Server server, WmiObjectPath path, InstanceEventType instanceEventType, string[] additionalConditions)
		: this(new ObjectKey(server, path), instanceEventType, additionalConditions)
	{
	}

	public EventObjectKey(ObjectKey objectKey, InstanceEventType instanceEventType, string[] additionalConditions)
		: base(objectKey)
	{
		m_InstanceEventType = instanceEventType;
		m_AdditionalConditions = additionalConditions;
		m_IndexString = GetIndexString();
		m_HashCode = m_IndexString.ToUpperInvariant().GetHashCode();
	}

	public override int GetHashCode()
	{
		return m_HashCode;
	}

	public override string ToString()
	{
		return m_IndexString;
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (obj is EventObjectKey eventObjectKey)
		{
			return string.Equals(m_IndexString, eventObjectKey.m_IndexString, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public string GetInstanceEventQuery(TimeSpan interval, bool classQuery)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "SELECT * FROM __{0} WITHIN {1} WHERE TargetInstance ISA '{2}'", InstanceEventType.ToString(), interval.Seconds, base.ClassName);
		if (!classQuery)
		{
			foreach (KeyValuePair<string, object> keyValue in base.ManagementPath.KeyValues)
			{
				if (keyValue.Value is string)
				{
					stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " AND TargetInstance.{0}=\"{1}\"", keyValue.Key, keyValue.Value);
				}
				else
				{
					stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " AND TargetInstance.{0}={1}", keyValue.Key, keyValue.Value);
				}
			}
		}
		if (AdditionalConditions != null)
		{
			string[] additionalConditions = AdditionalConditions;
			foreach (string text in additionalConditions)
			{
				stringBuilder.Append(" AND TargetInstance.");
				stringBuilder.Append(text.Trim());
			}
		}
		return stringBuilder.ToString();
	}

	private string GetIndexString()
	{
		StringBuilder stringBuilder = new StringBuilder(base.Server.Name);
		stringBuilder.Append(":");
		stringBuilder.Append(base.NamespaceName);
		stringBuilder.Append(":");
		stringBuilder.Append(base.ClassName);
		stringBuilder.Append(":");
		InstanceEventType instanceEventType = m_InstanceEventType;
		stringBuilder.Append(instanceEventType.ToString());
		if (m_AdditionalConditions != null)
		{
			string[] additionalConditions = m_AdditionalConditions;
			foreach (string value in additionalConditions)
			{
				stringBuilder.Append(":");
				stringBuilder.Append(value);
			}
		}
		return stringBuilder.ToString();
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
	private void ObjectInvariant()
	{
	}
}
