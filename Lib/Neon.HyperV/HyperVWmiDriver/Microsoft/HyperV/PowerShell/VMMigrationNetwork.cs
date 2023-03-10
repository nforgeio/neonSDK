using System.Globalization;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMMigrationNetwork : VirtualizationObject, IUpdatable
{
	private readonly IDataUpdater<IVMMigrationNetworkSetting> m_MigrationNetworkSetting;

	[VirtualizationObjectIdentifier(IdentifierFlags.UniqueIdentifier | IdentifierFlags.FriendlyName)]
	public string Subnet => string.Format(CultureInfo.InvariantCulture, "{0}/{1}", m_MigrationNetworkSetting.GetData(UpdatePolicy.EnsureUpdated).SubnetNumber, m_MigrationNetworkSetting.GetData(UpdatePolicy.EnsureUpdated).PrefixLength);

	public int? Priority => NumberConverter.UInt32ToInt32(m_MigrationNetworkSetting.GetData(UpdatePolicy.EnsureUpdated).Metric);

	internal WmiObjectPath WmiPath => m_MigrationNetworkSetting.GetData(UpdatePolicy.None).ManagementPath;

	internal VMMigrationNetwork(IVMMigrationNetworkSetting migrationNetworkSetting)
		: base(migrationNetworkSetting)
	{
		m_MigrationNetworkSetting = InitializePrimaryDataUpdater(migrationNetworkSetting);
	}

	internal void SetPriority(uint priority)
	{
		m_MigrationNetworkSetting.GetData(UpdatePolicy.None).Metric = priority;
	}

	void IUpdatable.Put(IOperationWatcher operationWatcher)
	{
		operationWatcher.PerformPut(m_MigrationNetworkSetting.GetData(UpdatePolicy.None), TaskDescriptions.SetVMMigrationNetwork, this);
	}
}
