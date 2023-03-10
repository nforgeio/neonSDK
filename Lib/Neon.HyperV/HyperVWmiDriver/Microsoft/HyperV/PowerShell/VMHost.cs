using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Net.Sockets;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMHost : VirtualizationObject, IUpdatable
{
	private readonly DataUpdater<IHostComputerSystem> m_HostComputerSystem;

	private readonly DataUpdater<IMetricServiceSetting> m_MetricServiceSettingData;

	private readonly DataUpdater<IVMService> m_Service;

	private readonly DataUpdater<IVMServiceSetting> m_ServiceSettings;

	private readonly DataUpdater<IVirtualSwitchManagementService> m_VirtualSwitchService;

	private readonly DataUpdater<IWin32ComputerSystem> m_Win32ComputerSystem;

	private const string AnySubnetIPv4 = "0.0.0.0/0";

	private const string AnySubnetIPv6 = "::0/0";

	private const string AnySubnetIPv6Alt = "::/0";

	private readonly DataUpdater<IVMMigrationService> m_MigrationService;

	private readonly DataUpdater<IVMMigrationServiceSetting> m_MigrationServiceSetting;

	public int LogicalProcessorCount => m_HostComputerSystem.GetData(UpdatePolicy.None).GetPhysicalProcessors().Sum((IPhysicalProcessor processor) => processor.NumberOfThreadsOfExecution);

	public TimeSpan ResourceMeteringSaveInterval
	{
		get
		{
			return m_MetricServiceSettingData.GetData(UpdatePolicy.EnsureUpdated).FlushInterval;
		}
		internal set
		{
			m_MetricServiceSettingData.GetData(UpdatePolicy.None).FlushInterval = value;
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	public VMHostNumaNode[] HostNumaStatus => VMHostNumaNode.GetHostNumaNodes(base.Server);

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	public VMNumaNodeStatus[] NumaStatus => VMNumaNodeStatus.GetVMNumaNodeStatus(base.Server);

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Iov", Justification = "This is by spec.")]
	public bool IovSupport => m_VirtualSwitchService.GetData(UpdatePolicy.EnsureAssociatorsUpdated).Capabilities.IOVSupport;

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Iov", Justification = "This is by spec.")]
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	public string[] IovSupportReasons => m_VirtualSwitchService.GetData(UpdatePolicy.EnsureAssociatorsUpdated).Capabilities.IOVSupportReasons;

	public List<VMInternalNetworkAdapter> InternalNetworkAdapters
	{
		get
		{
			IVirtualSwitchManagementService data = m_VirtualSwitchService.GetData(UpdatePolicy.None);
			data.UpdateInternalEthernetPorts(Constants.UpdateThreshold);
			return data.InternalEthernetPorts.Select(delegate(IInternalEthernetPort internalPort)
			{
				IVirtualEthernetSwitchPort virtualEthernetSwitchPort = (IVirtualEthernetSwitchPort)internalPort.GetConnectedEthernetPort(Constants.UpdateThreshold);
				VMSwitch virtualSwitch = new VMSwitch(virtualEthernetSwitchPort.VirtualEthernetSwitch.Setting);
				return new VMInternalNetworkAdapter(virtualEthernetSwitchPort.Setting, virtualEthernetSwitchPort, internalPort, virtualSwitch);
			}).ToList();
		}
	}

	public List<VMExternalNetworkAdapter> ExternalNetworkAdapters
	{
		get
		{
			IVirtualSwitchManagementService data = m_VirtualSwitchService.GetData(UpdatePolicy.None);
			data.UpdateExternalNetworkPorts(Constants.UpdateThreshold);
			List<VMExternalNetworkAdapter> list = new List<VMExternalNetworkAdapter>();
			foreach (IExternalNetworkPort item in data.ExternalNetworkPorts.Where((IExternalNetworkPort port) => port.IsBound))
			{
				IVirtualEthernetSwitchPort virtualEthernetSwitchPort = (IVirtualEthernetSwitchPort)item.GetConnectedEthernetPort(Constants.UpdateThreshold);
				if (virtualEthernetSwitchPort != null)
				{
					VMSwitch virtualSwitch = new VMSwitch(virtualEthernetSwitchPort.VirtualEthernetSwitch.Setting);
					list.Add(new VMExternalNetworkAdapter(virtualEthernetSwitchPort.Setting, virtualEthernetSwitchPort, new IExternalNetworkPort[1] { item }, virtualSwitch));
				}
			}
			return list;
		}
	}

	public IReadOnlyList<string> SupportedVmVersions => m_Service.GetData(UpdatePolicy.EnsureUpdated).GetSupportedVmVersions().ToList()
		.AsReadOnly();

	public IReadOnlyList<SecureBootTemplate> SecureBootTemplates => m_Service.GetData(UpdatePolicy.EnsureUpdated).GetSecureBootTemplates().ToList()
		.AsReadOnly();

	public bool EnableEnhancedSessionMode
	{
		get
		{
			return m_ServiceSettings.GetData(UpdatePolicy.EnsureUpdated).EnhancedSessionModeEnabled;
		}
		internal set
		{
			m_ServiceSettings.GetData(UpdatePolicy.None).EnhancedSessionModeEnabled = value;
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Wwnn", Justification = "This is per spec.")]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fibre", Justification = "This is per spec.")]
	public string FibreChannelWwnn
	{
		get
		{
			return m_ServiceSettings.GetData(UpdatePolicy.EnsureUpdated).AssignedWorldWideNodeName;
		}
		internal set
		{
			m_ServiceSettings.GetData(UpdatePolicy.None).AssignedWorldWideNodeName = value;
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Wwpn", Justification = "This is per spec.")]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fibre", Justification = "This is per spec.")]
	public string FibreChannelWwpnMaximum
	{
		get
		{
			return m_ServiceSettings.GetData(UpdatePolicy.EnsureUpdated).MaximumWorldWidePortName;
		}
		internal set
		{
			m_ServiceSettings.GetData(UpdatePolicy.None).MaximumWorldWidePortName = value;
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Wwpn", Justification = "This is per spec.")]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fibre", Justification = "This is per spec.")]
	public string FibreChannelWwpnMinimum
	{
		get
		{
			return m_ServiceSettings.GetData(UpdatePolicy.EnsureUpdated).MinimumWorldWidePortName;
		}
		internal set
		{
			m_ServiceSettings.GetData(UpdatePolicy.None).MinimumWorldWidePortName = value;
		}
	}

	public string MacAddressMaximum
	{
		get
		{
			return m_ServiceSettings.GetData(UpdatePolicy.EnsureUpdated).MaximumMacAddress;
		}
		internal set
		{
			m_ServiceSettings.GetData(UpdatePolicy.None).MaximumMacAddress = value;
		}
	}

	public string MacAddressMinimum
	{
		get
		{
			return m_ServiceSettings.GetData(UpdatePolicy.EnsureUpdated).MinimumMacAddress;
		}
		internal set
		{
			m_ServiceSettings.GetData(UpdatePolicy.None).MinimumMacAddress = value;
		}
	}

	public bool NumaSpanningEnabled
	{
		get
		{
			return m_ServiceSettings.GetData(UpdatePolicy.EnsureUpdated).NumaSpanningEnabled;
		}
		internal set
		{
			m_ServiceSettings.GetData(UpdatePolicy.None).NumaSpanningEnabled = value;
		}
	}

	public string VirtualHardDiskPath
	{
		get
		{
			return m_ServiceSettings.GetData(UpdatePolicy.EnsureUpdated).DefaultVirtualHardDiskPath;
		}
		internal set
		{
			m_ServiceSettings.GetData(UpdatePolicy.None).DefaultVirtualHardDiskPath = value;
		}
	}

	public string VirtualMachinePath
	{
		get
		{
			return m_ServiceSettings.GetData(UpdatePolicy.EnsureUpdated).DefaultExternalDataRoot;
		}
		internal set
		{
			m_ServiceSettings.GetData(UpdatePolicy.None).DefaultExternalDataRoot = value;
		}
	}

	public string FullyQualifiedDomainName => m_Win32ComputerSystem.GetData(UpdatePolicy.EnsureUpdated).Domain;

	public long MemoryCapacity => m_Win32ComputerSystem.GetData(UpdatePolicy.EnsureUpdated).TotalPhysicalMemory;

	[VirtualizationObjectIdentifier(IdentifierFlags.UniqueIdentifier | IdentifierFlags.FriendlyName)]
	public string Name => base.Server.Name;

	public uint MaximumStorageMigrations
	{
		get
		{
			return NumberConverter.Int64ToUInt32(m_MigrationServiceSetting.GetData(UpdatePolicy.EnsureUpdated).MaximumActiveStorageMigration);
		}
		internal set
		{
			m_MigrationServiceSetting.GetData(UpdatePolicy.None).MaximumActiveStorageMigration = value;
		}
	}

	public uint MaximumVirtualMachineMigrations
	{
		get
		{
			return NumberConverter.Int64ToUInt32(m_MigrationServiceSetting.GetData(UpdatePolicy.EnsureUpdated).MaximumActiveVirtualSystemMigration);
		}
		internal set
		{
			m_MigrationServiceSetting.GetData(UpdatePolicy.None).MaximumActiveVirtualSystemMigration = value;
		}
	}

	public bool UseAnyNetworkForMigration => GetAnyNetworks().Count == 2;

	public MigrationAuthenticationType VirtualMachineMigrationAuthenticationType
	{
		get
		{
			return (MigrationAuthenticationType)m_MigrationServiceSetting.GetData(UpdatePolicy.EnsureUpdated).AuthenticationType;
		}
		internal set
		{
			m_MigrationServiceSetting.GetData(UpdatePolicy.None).AuthenticationType = (int)value;
		}
	}

	public bool VirtualMachineMigrationEnabled
	{
		get
		{
			return m_MigrationServiceSetting.GetData(UpdatePolicy.EnsureUpdated).EnableVirtualSystemMigration;
		}
		internal set
		{
			m_MigrationServiceSetting.GetData(UpdatePolicy.None).EnableVirtualSystemMigration = value;
		}
	}

	public VMMigrationPerformance VirtualMachineMigrationPerformanceOption
	{
		get
		{
			IVMMigrationServiceSetting data = m_MigrationServiceSetting.GetData(UpdatePolicy.EnsureUpdated);
			if (data.EnableCompression)
			{
				return VMMigrationPerformance.Compression;
			}
			if (data.EnableSmbTransport)
			{
				return VMMigrationPerformance.SMB;
			}
			return VMMigrationPerformance.TCPIP;
		}
	}

	internal VMHost(IHostComputerSystem host)
		: base(host)
	{
		IVMService virtualizationService = ObjectLocator.GetVirtualizationService(base.Server);
		m_HostComputerSystem = InitializePrimaryDataUpdater(host);
		m_Service = new DataUpdater<IVMService>(virtualizationService);
		m_ServiceSettings = new DataUpdater<IVMServiceSetting>(virtualizationService.Setting);
		m_VirtualSwitchService = new DataUpdater<IVirtualSwitchManagementService>(ObjectLocator.GetVirtualSwitchManagementService(base.Server));
		m_Win32ComputerSystem = new DataUpdater<IWin32ComputerSystem>(ObjectLocator.GetWin32ComputerSystem(base.Server));
		m_MigrationService = new DataUpdater<IVMMigrationService>(ObjectLocator.GetVMMigrationService(base.Server));
		m_MigrationServiceSetting = new DataUpdater<IVMMigrationServiceSetting>(m_MigrationService.GetData(UpdatePolicy.None).Setting);
		m_MetricServiceSettingData = new DataUpdater<IMetricServiceSetting>(ObjectLocator.GetMetricService(base.Server).Setting);
	}

	void IUpdatable.Put(IOperationWatcher operationWatcher)
	{
		operationWatcher.PerformPut(m_ServiceSettings.GetData(UpdatePolicy.None), TaskDescriptions.SetVMHost, this);
		operationWatcher.PerformPut(m_MigrationServiceSetting.GetData(UpdatePolicy.None), TaskDescriptions.SetVMMigrationService, this);
		operationWatcher.PerformPut(m_MetricServiceSettingData.GetData(UpdatePolicy.None), TaskDescriptions.SetVMMetrics, this);
	}

	internal void AddAnyNetwork()
	{
		try
		{
			AddNetwork("0.0.0.0/0", null, isUserManaged: false);
		}
		catch (VirtualizationManagementException)
		{
		}
		try
		{
			AddNetwork("::0/0", null, isUserManaged: false);
		}
		catch (VirtualizationManagementException)
		{
		}
	}

	internal VMMigrationNetwork AddUserManagedMigrationNetwork(string subnet, uint? priority)
	{
		AddNetwork(subnet, priority, isUserManaged: true);
		List<VMMigrationNetwork> list = GetUserManagedVMMigrationNetworks(new string[1] { subnet }, (!priority.HasValue) ? null : new uint[1] { priority.Value }).ToList();
		if (list.Count == 1)
		{
			return list[0];
		}
		return list.First((VMMigrationNetwork network) => string.Equals(network.Subnet, subnet, StringComparison.OrdinalIgnoreCase));
	}

	internal IList<VMMigrationNetwork> GetAnyNetworks()
	{
		return (from adapter in m_MigrationServiceSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated).NetworkSettings.Where(delegate(IVMMigrationNetworkSetting network)
			{
				string a = network.SubnetNumber + "/" + network.PrefixLength;
				return (string.Equals(a, "0.0.0.0/0", StringComparison.OrdinalIgnoreCase) || string.Equals(a, "::0/0", StringComparison.OrdinalIgnoreCase) || string.Equals(a, "::/0", StringComparison.OrdinalIgnoreCase)) && network.Tags.Contains("Microsoft:UserManagedAllNetworks");
			})
			select new VMMigrationNetwork(adapter)).ToList();
	}

	internal bool GetIsLiveMigrationSupported()
	{
		return m_MigrationService.GetData(UpdatePolicy.EnsureUpdated).Capabilities.MigrationSettings.Any((IVMMigrationSetting setting) => setting.MigrationType == VMMigrationType.VirtualSystem);
	}

	[SuppressMessage("Microsoft.Usage", "#pw26501", Scope = "member", Target = "subnets")]
	internal IList<VMMigrationNetwork> GetUserManagedVMMigrationNetworks(string[] subnets, uint[] priorities)
	{
		List<IVMMigrationNetworkSetting> list = m_MigrationServiceSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated).GetUserManagedNetworkSettings().ToList();
		if (list.Count == 0)
		{
			return new List<VMMigrationNetwork>();
		}
		bool flag = subnets != null;
		bool flag2 = priorities != null;
		if (!flag && !flag2)
		{
			return list.Select((IVMMigrationNetworkSetting setting) => new VMMigrationNetwork(setting)).ToList();
		}
		if (flag && flag2)
		{
			return (from setting in list
				where MatchesAnySubnets(setting, subnets) && priorities.Any((uint priority) => setting.Metric == priority)
				select setting into matched
				select new VMMigrationNetwork(matched)).ToList();
		}
		if (flag)
		{
			return (from setting in list
				where MatchesAnySubnets(setting, subnets)
				select setting into matched
				select new VMMigrationNetwork(matched)).ToList();
		}
		return (from setting in list
			where priorities.Any((uint priority) => setting.Metric == priority)
			select setting into matched
			select new VMMigrationNetwork(matched)).ToList();
	}

	internal void RemoveAnyNetwork()
	{
		IList<VMMigrationNetwork> anyNetworks = GetAnyNetworks();
		if (anyNetworks.Count > 0)
		{
			m_MigrationService.GetData(UpdatePolicy.None).RemoveNetworkSettings(anyNetworks.Select((VMMigrationNetwork network) => network.WmiPath).ToArray());
		}
		m_MigrationServiceSetting.GetData(UpdatePolicy.None).InvalidateAssociationCache();
	}

	internal void RemoveMigrationNetwork(VMMigrationNetwork networkToRemove)
	{
		m_MigrationService.GetData(UpdatePolicy.None).RemoveNetworkSettings(new WmiObjectPath[1] { networkToRemove.WmiPath });
		m_MigrationServiceSetting.GetData(UpdatePolicy.None).InvalidateAssociationCache();
	}

	internal void SetVirtualMigrationPerformanceOption(VMMigrationPerformance option)
	{
		IVMMigrationServiceSetting data = m_MigrationServiceSetting.GetData(UpdatePolicy.None);
		switch (option)
		{
		case VMMigrationPerformance.Compression:
			data.EnableCompression = true;
			data.EnableSmbTransport = false;
			break;
		case VMMigrationPerformance.SMB:
			data.EnableCompression = false;
			data.EnableSmbTransport = true;
			break;
		case VMMigrationPerformance.TCPIP:
			data.EnableCompression = false;
			data.EnableSmbTransport = false;
			break;
		}
	}

	internal static bool IsApipaAddress(IPAddress ipAddress)
	{
		if (ipAddress.AddressFamily != AddressFamily.InterNetwork)
		{
			return false;
		}
		byte[] addressBytes = ipAddress.GetAddressBytes();
		if (addressBytes[0] == 169)
		{
			return addressBytes[1] == 254;
		}
		return false;
	}

	private static bool MatchesAnySubnets(IVMMigrationNetworkSetting setting, IEnumerable<string> subnets)
	{
		return subnets.Any((string subnet) => MatchesSubnet(setting, subnet));
	}

	private static bool MatchesSubnet(IVMMigrationNetworkSetting setting, string subnet)
	{
		if (WildcardPattern.ContainsWildcardCharacters(subnet))
		{
			return new WildcardPattern(subnet, WildcardOptions.Compiled | WildcardOptions.IgnoreCase).IsMatch(string.Format(CultureInfo.InvariantCulture, "{0}/{1}", setting.SubnetNumber, setting.PrefixLength));
		}
		NetworkUtilities.ParseSubnetString(subnet, out var address, out var prefixLength);
		IPAddress iPAddress = IPAddress.Parse(address);
		if (prefixLength == setting.PrefixLength)
		{
			return iPAddress.Equals(IPAddress.Parse(setting.SubnetNumber));
		}
		return false;
	}

	private void AddNetwork(string subnet, uint? priority, bool isUserManaged)
	{
		NetworkUtilities.ParseSubnetString(subnet, out var address, out var prefixLength);
		Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		dictionary.Add("SubnetNumber", address);
		dictionary.Add("PrefixLength", prefixLength);
		dictionary.Add("Tags", new string[1] { isUserManaged ? "Microsoft:UserManaged" : "Microsoft:UserManagedAllNetworks" });
		Dictionary<string, object> dictionary2 = dictionary;
		if (priority.HasValue)
		{
			dictionary2.Add("Metric", priority.Value);
		}
		string newEmbeddedInstance = base.Server.GetNewEmbeddedInstance("Msvm_VirtualSystemMigrationNetworkSettingData", dictionary2);
		m_MigrationService.GetData(UpdatePolicy.None).AddNetworkSettings(new string[1] { newEmbeddedInstance });
		m_MigrationServiceSetting.GetData(UpdatePolicy.None).InvalidateAssociationCache();
	}
}
