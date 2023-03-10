using System;

namespace Microsoft.Virtualization.Client.Management;

internal class SummaryInformationBaseView : View, ISummaryInformationBase, ISummaryInformationPropertiesBase, IVirtualizationManagementObject
{
	public string Name => GetProperty<string>("Name");

	public string ElementName => GetPropertyOrDefault<string>("ElementName");

	public string HostComputerSystemName => GetPropertyOrDefault("HostComputerSystemName", base.Server.Name);

	public VMComputerSystemState State
	{
		get
		{
			VMComputerSystemState vMComputerSystemState = (VMComputerSystemState)GetPropertyOrDefault("EnabledState", (ushort)0);
			if (vMComputerSystemState == VMComputerSystemState.Other)
			{
				vMComputerSystemState = VMComputerSystemStateUtilities.ConvertVMComputerSystemOtherState(GetPropertyOrDefault<string>("OtherEnabledState"));
			}
			return vMComputerSystemState;
		}
	}

	public bool RdpEnhancedModeAvailable => GetPropertyOrDefault("EnhancedSessionModeState", (ushort)3) == 2;

	public VirtualSystemSubType VirtualSystemSubType => VMComputerSystemSettingView.WmiVirtualSystemSubTypeToEnumVirtualSystemSubType(GetPropertyOrDefault("VirtualSystemSubType", "Microsoft:Hyper-V:SubType:1"));

	public TimeSpan Uptime => TimeSpan.FromMilliseconds(GetPropertyOrDefault("UpTime", 0uL));

	public VMComputerSystemHealthState HealthState => (VMComputerSystemHealthState)GetPropertyOrDefault("HealthState", (ushort)0);

	public DateTime CreationTime => GetPropertyOrDefault<DateTime>("CreationTime");

	public string Notes => GetPropertyOrDefault("Notes", "");

	public string Version => GetProperty<string>("Version");

	public bool Shielded => GetPropertyOrDefault("Shielded", defaultValue: false);

	public void UpdatePropertyCache(SummaryInformationRequest requestedInformation)
	{
		UpdatePropertyCache(TimeSpan.Zero, requestedInformation);
	}

	public void UpdatePropertyCache(TimeSpan threshold, SummaryInformationRequest requestedInformation)
	{
		if (NeedsUpdate(threshold))
		{
			base.Proxy.UpdatePropertyCache(threshold, SummaryInformation.GetRequestedInformationOptions(requestedInformation));
		}
	}

	public VMComputerSystemOperationalStatus[] GetOperationalStatus()
	{
		ushort[] propertyOrDefault = GetPropertyOrDefault<ushort[]>("OperationalStatus");
		if (propertyOrDefault != null)
		{
			return VMComputerSystemStateUtilities.ConvertOperationalStatus(propertyOrDefault);
		}
		return new VMComputerSystemOperationalStatus[1];
	}

	public string[] GetStatusDescriptions()
	{
		return GetPropertyOrDefault<string[]>("StatusDescriptions");
	}
}
