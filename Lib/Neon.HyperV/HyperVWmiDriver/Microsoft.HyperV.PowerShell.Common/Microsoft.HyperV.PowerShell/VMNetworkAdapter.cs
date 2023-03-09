#define TRACE
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMNetworkAdapter : VMNetworkAdapterBase, IBootableDevice
{
	private bool m_PortSettingAddRequired;

	private bool m_ConnectionSettingAddRequired;

	private readonly IEthernetSwitchFeatureService m_FeatureService;

	internal override string PutDescription => TaskDescriptions.SetVMNetworkAdapter;

	public bool ClusterMonitored
	{
		get
		{
			return m_PortSetting.GetData(UpdatePolicy.EnsureUpdated).ClusterMonitored;
		}
		internal set
		{
			m_PortSetting.GetData(UpdatePolicy.None).ClusterMonitored = value;
		}
	}

	public string MacAddress
	{
		get
		{
			return m_PortSetting.GetData(UpdatePolicy.EnsureUpdated).NetworkAddress;
		}
		internal set
		{
			m_PortSetting.GetData(UpdatePolicy.None).NetworkAddress = value;
		}
	}

	public uint MediaType => (m_PortSetting.GetData(UpdatePolicy.EnsureUpdated) as ISyntheticEthernetPortSetting).MediaType;

	public bool DynamicMacAddressEnabled
	{
		get
		{
			return !m_PortSetting.GetData(UpdatePolicy.EnsureUpdated).IsNetworkAddressStatic;
		}
		internal set
		{
			m_PortSetting.GetData(UpdatePolicy.None).IsNetworkAddressStatic = !value;
		}
	}

	public bool InterruptModeration
	{
		get
		{
			if (m_PortSetting.GetData(UpdatePolicy.EnsureUpdated) is ISyntheticEthernetPortSetting syntheticEthernetPortSetting)
			{
				return syntheticEthernetPortSetting.InterruptModeration;
			}
			return true;
		}
		internal set
		{
			if (m_PortSetting.GetData(UpdatePolicy.None) is ISyntheticEthernetPortSetting syntheticEthernetPortSetting)
			{
				syntheticEthernetPortSetting.InterruptModeration = value;
			}
		}
	}

	public bool AllowPacketDirect
	{
		get
		{
			if (m_PortSetting.GetData(UpdatePolicy.EnsureUpdated) is ISyntheticEthernetPortSetting syntheticEthernetPortSetting)
			{
				return syntheticEthernetPortSetting.AllowPacketDirect;
			}
			return false;
		}
		set
		{
			if (m_PortSetting.GetData(UpdatePolicy.None) is ISyntheticEthernetPortSetting syntheticEthernetPortSetting)
			{
				syntheticEthernetPortSetting.AllowPacketDirect = value;
			}
			else if (value)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.PacketDirect_DoesNotApply));
			}
		}
	}

	public bool NumaAwarePlacement
	{
		get
		{
			if (m_PortSetting.GetData(UpdatePolicy.EnsureUpdated) is ISyntheticEthernetPortSetting syntheticEthernetPortSetting)
			{
				return syntheticEthernetPortSetting.NumaAwarePlacement;
			}
			return false;
		}
		set
		{
			if (m_PortSetting.GetData(UpdatePolicy.None) is ISyntheticEthernetPortSetting syntheticEthernetPortSetting)
			{
				syntheticEthernetPortSetting.NumaAwarePlacement = value;
			}
			else if (value)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.NumaAwarePlacement_DoesNotApply));
			}
		}
	}

	public bool IsLegacy => m_PortSetting.GetData(UpdatePolicy.EnsureUpdated).VMDeviceSettingType == VMDeviceSettingType.EthernetPortEmulated;

	public bool IsSynthetic => m_PortSetting.GetData(UpdatePolicy.EnsureUpdated).VMDeviceSettingType == VMDeviceSettingType.EthernetPortSynthetic;

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	public string[] IPAddresses => m_PortSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated).GetGuestNetworkAdapterConfiguration().IPAddresses;

	public OnOffState DeviceNaming
	{
		get
		{
			if (m_PortSetting.GetData(UpdatePolicy.EnsureUpdated) is ISyntheticEthernetPortSetting syntheticEthernetPortSetting)
			{
				return syntheticEthernetPortSetting.DeviceNamingEnabled.ToOnOffState();
			}
			return OnOffState.Off;
		}
		internal set
		{
			if (m_PortSetting.GetData(UpdatePolicy.None) is ISyntheticEthernetPortSetting syntheticEthernetPortSetting)
			{
				syntheticEthernetPortSetting.DeviceNamingEnabled = value.ToBool();
			}
			else if (value.ToBool())
			{
				throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.DeviceNaming_NotApply));
			}
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Iov", Justification = "This is by spec.")]
	public uint IovWeight => base.OffloadSetting?.IovWeight ?? 0;

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Iov", Justification = "This is by spec.")]
	public uint IovQueuePairsRequested => base.OffloadSetting?.IOVQueuePairsRequested ?? 1;

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Iov", Justification = "This is by spec.")]
	public IovInterruptModerationValue IovInterruptModeration => base.OffloadSetting?.IovInterruptModeration ?? IovInterruptModerationValue.Default;

	public uint PacketDirectNumProcs => base.OffloadSetting?.PacketDirectNumProcs ?? 0;

	public uint PacketDirectModerationCount => base.OffloadSetting?.PacketDirectModerationCount ?? 0;

	public uint PacketDirectModerationInterval
	{
		get
		{
			_ = base.OffloadSetting;
			if (base.OffloadSetting == null)
			{
				return 0u;
			}
			return base.OffloadSetting.PacketDirectModerationInterval;
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Iov", Justification = "This is by spec.")]
	public uint IovQueuePairsAssigned => GetSwitchPortOffloadStatus()?.IovQueuePairUsage ?? 0;

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Iov", Justification = "This is by spec.")]
	public int IovUsage => GetSwitchPortOffloadStatus()?.IovOffloadUsage ?? 0;

	public CimInstance VirtualFunction
	{
		get
		{
			IEthernetSwitchPortOffloadStatus switchPortOffloadStatus = GetSwitchPortOffloadStatus();
			if (switchPortOffloadStatus != null && switchPortOffloadStatus.IovOffloadUsage > 0)
			{
				VMSwitch connectedSwitch = GetConnectedSwitch();
				if (connectedSwitch != null)
				{
					string text = connectedSwitch.NetAdapterInterfaceDescriptions?.FirstOrDefault();
					if (!string.IsNullOrEmpty(text))
					{
						ICimInstance virtualFunction = NetworkingUtilities.GetVirtualFunction(base.Server, text, switchPortOffloadStatus.IovVirtualFunctionId);
						if (virtualFunction != null)
						{
							return virtualFunction.Instance;
						}
					}
				}
			}
			return null;
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	public string[] MandatoryFeatureId
	{
		get
		{
			if (m_ConnectionSetting.GetData(UpdatePolicy.EnsureUpdated) is IEthernetConnectionAllocationRequest ethernetConnectionAllocationRequest)
			{
				return ethernetConnectionAllocationRequest.RequiredFeatureIds.ToArray();
			}
			return new string[0];
		}
		internal set
		{
			(m_ConnectionSetting.GetData(UpdatePolicy.None) as IEthernetConnectionAllocationRequest).RequiredFeatureIds = value;
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	public string[] MandatoryFeatureName
	{
		get
		{
			if (m_ConnectionSetting.GetData(UpdatePolicy.EnsureUpdated) is IEthernetConnectionAllocationRequest ethernetConnectionAllocationRequest)
			{
				return ethernetConnectionAllocationRequest.RequiredFeatureNames.ToArray();
			}
			return new string[0];
		}
	}

	public string PoolName
	{
		get
		{
			string text = null;
			IEthernetPortAllocationSettingData data = m_ConnectionSetting.GetData(UpdatePolicy.EnsureUpdated);
			if (data != null)
			{
				text = data.PoolId;
				if (string.IsNullOrEmpty(text) && data.HostResource == null)
				{
					text = "Primordial";
				}
			}
			return text;
		}
		internal set
		{
			string text2 = ((m_ConnectionSetting.GetData(UpdatePolicy.None) as IEthernetConnectionAllocationRequest).PoolId = (VMResourcePool.IsPrimordialPoolName(value) ? string.Empty : value));
		}
	}

	public bool Connected
	{
		get
		{
			bool result = false;
			if (m_ConnectionSetting.GetData(UpdatePolicy.EnsureUpdated) is IEthernetConnectionAllocationRequest ethernetConnectionAllocationRequest && ethernetConnectionAllocationRequest.IsEnabled)
			{
				result = true;
			}
			return result;
		}
	}

	public override string SwitchName
	{
		get
		{
			string result = null;
			VMSwitch connectedSwitch = GetConnectedSwitch();
			if (connectedSwitch != null)
			{
				result = connectedSwitch.Name;
			}
			else if (m_ConnectionSetting.GetData(UpdatePolicy.EnsureUpdated) is IEthernetConnectionAllocationRequest ethernetConnectionAllocationRequest && ethernetConnectionAllocationRequest.IsEnabled && !string.IsNullOrEmpty(ethernetConnectionAllocationRequest.PoolId))
			{
				result = ObjectDescriptions.VMNetworkAdapter_SwitchName_SoftAffinity;
			}
			return result;
		}
	}

	public override string AdapterId => m_PortSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated).EthernetDevice?.DeviceId.Replace("Microsoft:", string.Empty);

	public string TestReplicaPoolName
	{
		get
		{
			string result = null;
			IEthernetPortAllocationSettingData data = m_ConnectionSetting.GetData(UpdatePolicy.EnsureUpdated);
			if (data != null)
			{
				result = data.TestReplicaPoolId;
			}
			return result;
		}
		internal set
		{
			(m_ConnectionSetting.GetData(UpdatePolicy.None) as IEthernetConnectionAllocationRequest).TestReplicaPoolId = value;
		}
	}

	public string TestReplicaSwitchName
	{
		get
		{
			string result = null;
			IEthernetPortAllocationSettingData data = m_ConnectionSetting.GetData(UpdatePolicy.EnsureUpdated);
			if (data != null)
			{
				result = data.TestReplicaSwitchName;
			}
			return result;
		}
		internal set
		{
			(m_ConnectionSetting.GetData(UpdatePolicy.None) as IEthernetConnectionAllocationRequest).TestReplicaSwitchName = value;
		}
	}

	VMBootSource IBootableDevice.BootSource => new VMBootSource(m_PortSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated).BootEntry, GetParentAs<VirtualMachineBase>());

	internal bool IsEnabled
	{
		get
		{
			if (m_ConnectionSetting.GetData(UpdatePolicy.EnsureUpdated) is IEthernetConnectionAllocationRequest ethernetConnectionAllocationRequest)
			{
				return ethernetConnectionAllocationRequest.IsEnabled;
			}
			return false;
		}
		set
		{
			(m_ConnectionSetting.GetData(UpdatePolicy.None) as IEthernetConnectionAllocationRequest).IsEnabled = value;
		}
	}

	internal override IEthernetSwitchFeatureService FeatureService => m_FeatureService;

	internal VMNetworkAdapterFailoverSetting FailoverSetting
	{
		get
		{
			VMReplicationMode replicationMode = GetParentAs<VirtualMachine>().ReplicationMode;
			if (replicationMode == VMReplicationMode.Primary || replicationMode == VMReplicationMode.Replica || replicationMode == VMReplicationMode.ExtendedReplica)
			{
				IFailoverNetworkAdapterSetting failoverNetworkAdapterSetting = (m_PortSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated) as ISyntheticEthernetPortSetting).FailoverNetworkAdapterSetting;
				if (failoverNetworkAdapterSetting != null)
				{
					return new VMNetworkAdapterFailoverSetting(failoverNetworkAdapterSetting);
				}
			}
			return null;
		}
	}

	internal VMNetworkAdapter(IEthernetPortSetting setting, IEthernetConnectionAllocationRequest connection, ComputeResource parentComputeResource)
		: this(setting, connection, parentComputeResource, isTemplate: false)
	{
		m_FeatureService = ObjectLocator.GetVirtualizationService(base.Server);
	}

	private VMNetworkAdapter(IEthernetPortSetting setting, IEthernetConnectionAllocationRequest connection, ComputeResource parentComputeResource, bool isTemplate)
		: base(setting, connection, parentComputeResource, isTemplate)
	{
		base.IsTemplate = isTemplate;
		m_FeatureService = ObjectLocator.GetVirtualizationService(base.Server);
	}

	internal override IDataUpdater<IVMDeviceSetting> GetDeviceDataUpdater()
	{
		return m_PortSetting;
	}

	internal override VMSwitch GetConnectedSwitch()
	{
		VMSwitch result = null;
		if (m_ConnectionSetting.GetData(UpdatePolicy.EnsureUpdated) is IEthernetConnectionAllocationRequest ethernetConnectionAllocationRequest && ethernetConnectionAllocationRequest.IsEnabled)
		{
			WmiObjectPath hostResource = ethernetConnectionAllocationRequest.HostResource;
			if (hostResource != null)
			{
				IVirtualEthernetSwitch virtualEthernetSwitch = (IVirtualEthernetSwitch)ObjectLocator.GetVirtualizationManagementObject(base.Server, hostResource);
				if (virtualEthernetSwitch != null)
				{
					virtualEthernetSwitch.UpdateAssociationCache();
					if (virtualEthernetSwitch.Setting != null)
					{
						result = new VMSwitch(virtualEthernetSwitch.Setting);
					}
				}
			}
		}
		return result;
	}

	internal override IVirtualEthernetSwitchPort GetSwitchPort()
	{
		IVirtualEthernetSwitchPort result = null;
		IEthernetPort ethernetDevice = m_PortSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated).EthernetDevice;
		if (ethernetDevice != null)
		{
			result = (IVirtualEthernetSwitchPort)ethernetDevice.GetConnectedEthernetPort(Constants.UpdateThreshold);
		}
		return result;
	}

	internal void SetConnectedSwitchName(string switchName)
	{
		VMSwitch vMSwitch = VMSwitch.GetSwitchesByNamesAndServers(new Server[1] { base.Server }, new string[1] { switchName }, allowWildcards: true, ErrorDisplayMode.None, null).SingleOrDefault();
		if (vMSwitch != null)
		{
			SetConnectedSwitch(vMSwitch);
			return;
		}
		throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.VMSwitch_NotFoundByName, switchName));
	}

	internal void SetConnectedSwitch(VMSwitch virtualSwitch)
	{
		IEthernetConnectionAllocationRequest ethernetConnectionAllocationRequest = m_ConnectionSetting.GetData(UpdatePolicy.None) as IEthernetConnectionAllocationRequest;
		if (virtualSwitch != null)
		{
			bool flag = true;
			if (!string.IsNullOrEmpty(ethernetConnectionAllocationRequest.PoolId))
			{
				IResourcePool resourcePool = ethernetConnectionAllocationRequest.ResourcePool;
				if (resourcePool != null && !new VMEthernetResourcePool(resourcePool).VMSwitches.Contains(virtualSwitch))
				{
					flag = false;
				}
			}
			ethernetConnectionAllocationRequest.HostResource = virtualSwitch.VirtualizationManagementSwitch.ManagementPath;
			if (!flag)
			{
				ethernetConnectionAllocationRequest.PoolId = null;
			}
		}
		else
		{
			ethernetConnectionAllocationRequest.HostResource = null;
		}
		ethernetConnectionAllocationRequest.IsEnabled = true;
	}

	internal VMNetworkAdapterConnectionTestResult TestNetworkConnectivity(bool isSender, string senderIPAddress, string receiverIPAddress, string receiverMacAddress, int isolationID, int sequenceNumber, int payloadSize)
	{
		return new VMNetworkAdapterConnectionTestResult(((m_ConnectionSetting.GetData(UpdatePolicy.None) as IEthernetConnectionAllocationRequest) ?? throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.VMNetworkAdapter_ConnectionNotFound, null)).TestNetworkConnectivity(isSender, senderIPAddress, receiverIPAddress, receiverMacAddress, isolationID, sequenceNumber, payloadSize));
	}

	internal override ILanEndpoint GetLanEndpoint()
	{
		ILanEndpoint result = null;
		IEthernetPort ethernetDevice = m_PortSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated).EthernetDevice;
		if (ethernetDevice != null)
		{
			ethernetDevice.UpdateAssociationCache();
			result = ethernetDevice.LanEndpoint;
		}
		return result;
	}

	internal static VMNetworkAdapter ApplyAdd(VMNetworkAdapter templateAdapter, IOperationWatcher operationWatcher)
	{
		IEthernetPortSetting data = templateAdapter.m_PortSetting.GetData(UpdatePolicy.None);
		IEthernetPortSetting ethernetPortSetting = templateAdapter.AddSettingInternal(data, operationWatcher);
		IEthernetConnectionAllocationRequest ethernetConnectionAllocationRequest = (IEthernetConnectionAllocationRequest)templateAdapter.m_ConnectionSetting.GetData(UpdatePolicy.None);
		ethernetConnectionAllocationRequest.Parent = ethernetPortSetting;
		IEthernetConnectionAllocationRequest connection;
		try
		{
			connection = templateAdapter.AddSettingInternal(ethernetConnectionAllocationRequest, operationWatcher);
		}
		catch (Exception)
		{
			try
			{
				operationWatcher.PerformDelete(ethernetPortSetting, TaskDescriptions.AddVMNetworkAdapter_RollbackPartialAdd, null);
			}
			catch (Exception ex)
			{
				VMTrace.TraceError("Failed to clean up a partially added network adapter after a failed add!", ex);
			}
			throw;
		}
		ComputeResource parentAs = templateAdapter.GetParentAs<ComputeResource>();
		parentAs.InvalidateDeviceSettingsList();
		return new VMNetworkAdapter(ethernetPortSetting, connection, parentAs);
	}

	internal override void PrepareForModify(IOperationWatcher operationWatcher)
	{
		if (!m_ConnectionSettingAddRequired && !(m_ConnectionSetting.GetData(UpdatePolicy.EnsureUpdated) is IEthernetConnectionAllocationRequest))
		{
			m_ConnectionSettingAddRequired = true;
		}
		if (m_ConnectionSettingAddRequired)
		{
			IEthernetPortSetting portSetting = m_PortSetting.GetData(UpdatePolicy.None);
			IEthernetConnectionAllocationRequest ethernetConnectionAllocationRequest = VMDevice.CreateTemplateDeviceSetting<IEthernetConnectionAllocationRequest>(base.Server, VMDeviceSettingType.EthernetConnection);
			ethernetConnectionAllocationRequest.IsEnabled = false;
			ethernetConnectionAllocationRequest.Parent = portSetting;
			IEthernetConnectionAllocationRequest dataItem = AddSettingInternal(ethernetConnectionAllocationRequest, operationWatcher);
			m_ConnectionSetting = new DependentObjectDataUpdater<IEthernetPortAllocationSettingData>(dataItem, (TimeSpan span) => portSetting.GetConnectionConfiguration());
			m_ConnectionSettingAddRequired = false;
		}
	}

	internal override void PutSelf(IOperationWatcher operationWatcher)
	{
		IEthernetPortSetting data = m_PortSetting.GetData(UpdatePolicy.None);
		PutOneDeviceSetting(data, operationWatcher);
		IEthernetPortAllocationSettingData data2 = m_ConnectionSetting.GetData(UpdatePolicy.None);
		PutOneDeviceSetting(data2, operationWatcher);
	}

	internal override void RemoveSelf(IOperationWatcher operationWatcher)
	{
		IEthernetPortSetting data = m_PortSetting.GetData(UpdatePolicy.None);
		RemoveInternal(data, TaskDescriptions.RemoveVMNetworkAdapter_FromVM, operationWatcher);
	}

	private TDeviceSetting AddSettingInternal<TDeviceSetting>(TDeviceSetting deviceSetting, IOperationWatcher operationWatcher) where TDeviceSetting : class, IVMDeviceSetting
	{
		return GetParentAs<ComputeResource>().AddDeviceSetting(deviceSetting, TaskDescriptions.AddVMNetworkAdapter_VirtualMachine, operationWatcher);
	}

	internal static VMNetworkAdapter CreateTemplateForAdd(bool isEmulated, ComputeResource parentComputeResource)
	{
		Server server = parentComputeResource.Server;
		VMDeviceSettingType deviceType = (isEmulated ? VMDeviceSettingType.EthernetPortEmulated : VMDeviceSettingType.EthernetPortSynthetic);
		IEthernetPortSetting ethernetPortSetting = VMDevice.CreateTemplateDeviceSetting<IEthernetPortSetting>(server, deviceType);
		if (!isEmulated)
		{
			ethernetPortSetting.VMBusChannelInstanceGuid = Guid.NewGuid();
		}
		ethernetPortSetting.FriendlyName = (isEmulated ? ObjectDescriptions.VMNetworkAdapter_DefaultName_Legacy : ObjectDescriptions.VMNetworkAdapter_DefaultName_Synthetic);
		IEthernetConnectionAllocationRequest ethernetConnectionAllocationRequest = VMDevice.CreateTemplateDeviceSetting<IEthernetConnectionAllocationRequest>(server, VMDeviceSettingType.EthernetConnection);
		ethernetConnectionAllocationRequest.IsEnabled = false;
		return new VMNetworkAdapter(ethernetPortSetting, ethernetConnectionAllocationRequest, parentComputeResource, isTemplate: true)
		{
			m_PortSettingAddRequired = true,
			m_ConnectionSettingAddRequired = true
		};
	}
}
