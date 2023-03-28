#define TRACE
using System.Globalization;
using System.Linq;
using Microsoft.Management.Infrastructure;

namespace Microsoft.Virtualization.Client.Management;

internal class SummaryInformationView : SummaryInformationBaseView, ISummaryInformation, ISummaryInformationProperties, ISummaryInformationPropertiesBase, ISummaryInformationBase, IVirtualizationManagementObject
{
	public int ProcessorLoad => GetPropertyOrDefault("ProcessorLoad", (ushort)0);

	public long AssignedMemory => (long)GetPropertyOrDefault("MemoryUsage", 0uL);

	public int MemoryAvailable => GetPropertyOrDefault("MemoryAvailable", SummaryInformation.InvalidMemoryAvailable);

	public long MemoryDemand
	{
		get
		{
			int memoryAvailable = MemoryAvailable;
			if (memoryAvailable != SummaryInformation.InvalidMemoryAvailable)
			{
				return AssignedMemory * (100 - memoryAvailable) / 100;
			}
			return SummaryInformation.InvalidMemory;
		}
	}

	public int AvailableMemoryBuffer => GetPropertyOrDefault("AvailableMemoryBuffer", SummaryInformation.InvalidAvailableMemoryBuffer);

	public bool SwapFilesInUse => GetPropertyOrDefault("SwapFilesInUse", defaultValue: false);

	public bool MemorySpansPhysicalNumaNodes => GetPropertyOrDefault("MemorySpansPhysicalNumaNodes", defaultValue: false);

	public VMHeartbeatStatus Heartbeat
	{
		get
		{
			ushort propertyOrDefault = GetPropertyOrDefault("Heartbeat", (ushort)0);
			VMIntegrationComponentOperationalStatus propertyOrDefault2 = (VMIntegrationComponentOperationalStatus)GetPropertyOrDefault("ApplicationHealth", (ushort)0);
			return GetHeartbeatStatus((VMIntegrationComponentOperationalStatus)propertyOrDefault, propertyOrDefault2);
		}
	}

	public FailoverReplicationMode ReplicationMode => (FailoverReplicationMode)GetPropertyOrDefault("ReplicationMode", (ushort)0);

	public WmiObjectPath TestReplicaSystemPath
	{
		get
		{
			WmiObjectPath result = null;
			CimInstance propertyOrDefault = GetPropertyOrDefault<CimInstance>("TestReplicaSystem");
			if (propertyOrDefault != null)
			{
				result = new WmiObjectPath(base.Server, base.Server.VirtualizationNamespace, propertyOrDefault.ToICimInstance());
			}
			return result;
		}
	}

	public int ThumbnailImageWidth => GetPropertyOrDefault("ThumbnailImageWidth", (ushort)SummaryInformation.DefaultThumbnailImageWidth);

	public int ThumbnailImageHeight => GetPropertyOrDefault("ThumbnailImageHeight", (ushort)SummaryInformation.DefaultThumbnailImageHeight);

	public ISummaryInformationSnapshot CreateSnapshot()
	{
		return new SummaryInformationSnapshot(this);
	}

	public byte[] GetThumbnailImage()
	{
		byte[] result = null;
		switch (base.State)
		{
		case VMComputerSystemState.Running:
		case VMComputerSystemState.Saved:
		case VMComputerSystemState.Paused:
		case VMComputerSystemState.Saving:
		case VMComputerSystemState.Pausing:
		case VMComputerSystemState.Resuming:
		case VMComputerSystemState.FastSaved:
		case VMComputerSystemState.FastSaving:
		case VMComputerSystemState.Hibernated:
			result = GetPropertyOrDefault<byte[]>("ThumbnailImage");
			break;
		}
		return result;
	}

	public FailoverReplicationState[] GetReplicationStateEx()
	{
		FailoverReplicationState[] array = null;
		ushort[] propertyOrDefault = GetPropertyOrDefault<ushort[]>("ReplicationStateEx");
		if (propertyOrDefault != null)
		{
			return VMComputerSystemStateUtilities.ConvertReplicationState(propertyOrDefault);
		}
		return new FailoverReplicationState[1];
	}

	public FailoverReplicationHealth[] GetReplicationHealthEx()
	{
		FailoverReplicationHealth[] array = null;
		ushort[] propertyOrDefault = GetPropertyOrDefault<ushort[]>("ReplicationHealthEx");
		if (propertyOrDefault != null)
		{
			return VMComputerSystemStateUtilities.ConvertReplicationHealth(propertyOrDefault);
		}
		return new FailoverReplicationHealth[1];
	}

	public bool[] GetReplicatingToDefaultProvider()
	{
		bool[] array = null;
		string[] propertyOrDefault = GetPropertyOrDefault<string[]>("ReplicationProviderId");
		if (propertyOrDefault != null)
		{
			return propertyOrDefault.Select(SummaryInformation.IsDefaultReplicationProviderId).ToArray();
		}
		return new bool[1] { true };
	}

	private static VMHeartbeatStatus GetHeartbeatStatus(VMIntegrationComponentOperationalStatus primaryStatus, VMIntegrationComponentOperationalStatus secondaryStatus)
	{
		VMHeartbeatStatus vMHeartbeatStatus = VMHeartbeatStatus.Unknown;
		switch (primaryStatus)
		{
		case VMIntegrationComponentOperationalStatus.Unknown:
			return VMHeartbeatStatus.Unknown;
		case VMIntegrationComponentOperationalStatus.Error:
			return VMHeartbeatStatus.Error;
		case VMIntegrationComponentOperationalStatus.Disabled:
			return VMHeartbeatStatus.Disabled;
		case VMIntegrationComponentOperationalStatus.LostCommunication:
			return VMHeartbeatStatus.LostCommunication;
		case VMIntegrationComponentOperationalStatus.NoContact:
			return VMHeartbeatStatus.NoContact;
		case VMIntegrationComponentOperationalStatus.Dormant:
			return VMHeartbeatStatus.Paused;
		case VMIntegrationComponentOperationalStatus.Ok:
			switch (secondaryStatus)
			{
			case VMIntegrationComponentOperationalStatus.Unknown:
				return VMHeartbeatStatus.OkApplicationsUnknown;
			case VMIntegrationComponentOperationalStatus.NoContact:
				return VMHeartbeatStatus.OkApplicationsUnknown;
			case VMIntegrationComponentOperationalStatus.Ok:
				return VMHeartbeatStatus.OkApplicationsHealthy;
			case VMIntegrationComponentOperationalStatus.ApplicationCritical:
				return VMHeartbeatStatus.OkApplicationsCritical;
			default:
				VMTrace.TraceWarning(string.Format(CultureInfo.InvariantCulture, "Got unknown secondary heartbeat status - {0}", (int)secondaryStatus));
				return VMHeartbeatStatus.OkApplicationsUnknown;
			}
		default:
			VMTrace.TraceWarning(string.Format(CultureInfo.InvariantCulture, "Got unknown primary heartbeat status - {0}", (int)primaryStatus));
			return VMHeartbeatStatus.Unknown;
		}
	}
}
