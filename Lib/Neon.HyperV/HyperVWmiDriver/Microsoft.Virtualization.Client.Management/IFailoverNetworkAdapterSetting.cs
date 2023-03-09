using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IFailoverNetworkAdapterSettingContract : IFailoverNetworkAdapterSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
	public VMNetworkAdapterProtocolType ProtocolIFType
	{
		get
		{
			return VMNetworkAdapterProtocolType.Unknown;
		}
		set
		{
		}
	}

	public bool DhcpEnabled
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public string[] IPAddresses
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string[] Subnets
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string[] DefaultGateways
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string[] DnsServers
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public abstract IVMTask BeginPut();

	public abstract void EndPut(IVMTask putTask);

	public abstract void Put();

	public abstract void InvalidatePropertyCache();

	public abstract void UpdatePropertyCache();

	public abstract void UpdatePropertyCache(TimeSpan threshold);

	public abstract void RegisterForInstanceModificationEvents(InstanceModificationEventStrategy strategy);

	public abstract void UnregisterForInstanceModificationEvents();

	public abstract void InvalidateAssociationCache();

	public abstract void UpdateAssociationCache();

	public abstract void UpdateAssociationCache(TimeSpan threshold);

	public abstract string GetEmbeddedInstance();

	public abstract void DiscardPendingPropertyChanges();
}
[WmiName("Msvm_FailoverNetworkAdapterSettingData")]
internal interface IFailoverNetworkAdapterSetting : IVirtualizationManagementObject, IPutableAsync, IPutable
{
	VMNetworkAdapterProtocolType ProtocolIFType { get; set; }

	bool DhcpEnabled { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	string[] IPAddresses { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	string[] Subnets { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	string[] DefaultGateways { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	string[] DnsServers { get; set; }
}
