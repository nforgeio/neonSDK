using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal class ReplicationHealthInformation
{
	private readonly List<ReplicationStatistics> m_ReplicationStatistics = new List<ReplicationStatistics>();

	private readonly List<MsvmError> m_HealthMessages = new List<MsvmError>();

	public List<ReplicationStatistics> ReplicationStatistics => m_ReplicationStatistics;

	public List<MsvmError> HealthMessages => m_HealthMessages;
}
