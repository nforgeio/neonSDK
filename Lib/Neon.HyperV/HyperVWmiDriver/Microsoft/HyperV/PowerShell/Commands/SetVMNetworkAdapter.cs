using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Set", "VMNetworkAdapter", SupportsShouldProcess = true, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMNetworkAdapterBase) })]
internal sealed class SetVMNetworkAdapter : VirtualizationCmdlet<VMNetworkAdapterBase>, IVmBySingularVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmBySingularObjectCmdlet, IVMInternalNetworkAdapterCmdlet, IVMNetworkAdapterBaseCmdlet, ISupportsPassthrough
{
	[Parameter(ParameterSetName = "ManagementOS", Mandatory = true)]
	public SwitchParameter ManagementOS { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public string VMName { get; set; }

	[Parameter(ParameterSetName = "ResourceObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VMNetworkAdapterBase VMNetworkAdapter { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VirtualMachine VM { get; set; }

	[Parameter(ParameterSetName = "VMObject")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
	[Alias(new string[] { "VMNetworkAdapterName" })]
	public string Name { get; set; }

	[Parameter(ParameterSetName = "VMObject")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ResourceObject")]
	public SwitchParameter DynamicMacAddress { get; set; }

	[ValidateNotNull]
	[Parameter]
	public uint MediaType { get; }

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "The AllowPacketDirect parameter is temporarily disabled for the RS1 release.")]
	private SwitchParameter AllowPacketDirect { get; set; }

	[ValidateNotNull]
	[Parameter]
	public bool? NumaAwarePlacement { get; set; }

	[ValidateNotNull]
	[Parameter]
	public bool? InterruptModeration { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMObject")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ResourceObject")]
	public string StaticMacAddress { get; set; }

	[ValidateNotNullOrEmpty]
	[ValidateNotNull]
	[Parameter]
	public OnOffState? MacAddressSpoofing { get; set; }

	[ValidateNotNull]
	[Parameter]
	public OnOffState? DhcpGuard { get; set; }

	[ValidateNotNull]
	[Parameter]
	public OnOffState? RouterGuard { get; set; }

	[ValidateNotNull]
	[Parameter]
	public VMNetworkAdapterPortMirroringMode? PortMirroring { get; set; }

	[ValidateNotNull]
	[Parameter]
	public OnOffState? IeeePriorityTag { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Vmq", Justification = "This is per spec.")]
	[ValidateNotNull]
	[Parameter]
	public uint? VmqWeight { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Iov", Justification = "This is per spec.")]
	[ValidateNotNull]
	[Parameter]
	public uint? IovQueuePairsRequested { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Iov", Justification = "This is per spec.")]
	[ValidateNotNull]
	[Parameter]
	public IovInterruptModerationValue? IovInterruptModeration { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Iov", Justification = "This is per spec.")]
	[ValidateNotNull]
	[Parameter]
	public uint? IovWeight { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Psec", Justification = "IPsec is a standard term.")]
	[ValidateNotNull]
	[Parameter]
	public uint? IPsecOffloadMaximumSecurityAssociation { get; set; }

	[ValidateNotNull]
	[Parameter]
	public long? MaximumBandwidth { get; set; }

	[ValidateNotNull]
	[Parameter]
	public long? MinimumBandwidthAbsolute { get; set; }

	[ValidateNotNull]
	[Parameter]
	public uint? MinimumBandwidthWeight { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[Parameter]
	public string[] MandatoryFeatureId { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter]
	public string ResourcePoolName { get; set; }

	[Parameter]
	public string TestReplicaPoolName { get; set; }

	[Parameter]
	public string TestReplicaSwitchName { get; set; }

	[ValidateNotNull]
	[Parameter]
	public uint? VirtualSubnetId { get; set; }

	[ValidateNotNullOrEmpty]
	[ValidateNotNull]
	[Parameter]
	public OnOffState? AllowTeaming { get; set; }

	[ValidateNotNull]
	[Parameter]
	public bool? NotMonitoredInCluster { get; set; }

	[ValidateNotNull]
	[Parameter]
	public uint? StormLimit { get; set; }

	[ValidateNotNull]
	[Parameter]
	public uint? DynamicIPAddressLimit { get; set; }

	[ValidateNotNull]
	[Parameter]
	public OnOffState? DeviceNaming { get; set; }

	[ValidateNotNull]
	[Parameter]
	public OnOffState? FixSpeed10G { get; set; }

	[ValidateNotNull]
	[Parameter]
	public uint? PacketDirectNumProcs { get; set; }

	[ValidateNotNull]
	[Parameter]
	public uint? PacketDirectModerationCount { get; set; }

	[ValidateNotNull]
	[Parameter]
	public uint? PacketDirectModerationInterval { get; set; }

	[ValidateNotNull]
	[Parameter]
	public bool? VrssEnabled { get; set; }

	[ValidateNotNull]
	[Parameter]
	public bool? VmmqEnabled { get; set; }

	[ValidateNotNull]
	[Parameter]
	[Alias(new string[] { "VmmqQueuePairs" })]
	public uint? VrssMaxQueuePairs { get; set; }

	[ValidateNotNull]
	[Parameter]
	public uint? VrssMinQueuePairs { get; set; }

	[ValidateNotNull]
	[Parameter]
	public VrssQueueSchedulingModeType? VrssQueueSchedulingMode { get; set; }

	[ValidateNotNull]
	[Parameter]
	public bool? VrssExcludePrimaryProcessor { get; set; }

	[ValidateNotNull]
	[Parameter]
	public bool? VrssIndependentHostSpreading { get; set; }

	[ValidateNotNull]
	[Parameter]
	public VrssVmbusChannelAffinityPolicyType? VrssVmbusChannelAffinityPolicy { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	protected override void NormalizeParameters()
	{
		base.NormalizeParameters();
		if (!string.IsNullOrEmpty(StaticMacAddress))
		{
			StaticMacAddress = ParameterResolvers.ValidateAndNormalizeMacAddress(StaticMacAddress, "StaticMacAddress");
		}
	}

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		if (StaticMacAddress != null && DynamicMacAddress.IsPresent)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_StaticAndDynamicMacAddressBothPresent);
		}
		if (ManagementOS.IsPresent && MandatoryFeatureId != null)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_MandatoryFeatureIdAndManagementOS);
		}
		if (IovWeight.HasValue && (IovWeight.Value < 0 || IovWeight.Value > 100))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_InvalidIovWeightRange);
		}
		if (MinimumBandwidthWeight.HasValue && (MinimumBandwidthWeight.Value < 0 || MinimumBandwidthWeight.Value > 100))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_InvalidBandwidthReservationRange);
		}
		if (IovQueuePairsRequested.HasValue && IovQueuePairsRequested.Value < 1)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_IovQueuePairCountValue);
		}
		if (VrssMaxQueuePairs.HasValue && VrssMaxQueuePairs.Value < 1)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_VrssMaxQueuePairsCountValue);
		}
		if (VrssMinQueuePairs.HasValue && (VrssMinQueuePairs.Value < 1 || (VrssMaxQueuePairs.HasValue && VrssMinQueuePairs.Value > VrssMaxQueuePairs.Value)))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_VrssMinQueuePairsCountValue);
		}
		if (VrssQueueSchedulingMode.HasValue && VrssQueueSchedulingMode.Value != 0 && VrssQueueSchedulingMode.Value != VrssQueueSchedulingModeType.StaticVrss && VrssQueueSchedulingMode.Value != VrssQueueSchedulingModeType.StaticVmq)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_VrssQueueSchedulingModeValue);
		}
		if (VrssVmbusChannelAffinityPolicy.HasValue && VrssVmbusChannelAffinityPolicy.Value != VrssVmbusChannelAffinityPolicyType.Weak && VrssVmbusChannelAffinityPolicy.Value != VrssVmbusChannelAffinityPolicyType.Strong && VrssVmbusChannelAffinityPolicy.Value != VrssVmbusChannelAffinityPolicyType.Strict)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_VrssVmbusChannelAffinityPolicyValue);
		}
	}

	internal override IList<VMNetworkAdapterBase> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (CurrentParameterSetIs("ResourceObject"))
		{
			return new VMNetworkAdapterBase[1] { VMNetworkAdapter };
		}
		return ParameterResolvers.ResolveNetworkAdapters(this, Name, operationWatcher);
	}

	internal override void ProcessOneOperand(VMNetworkAdapterBase operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMNetworkAdapter, operand.Name)))
		{
			operand.PrepareForModify(operationWatcher);
			ConfigureConnectionSettings(operand, operationWatcher);
			ConfigureFeatureSettings(operand, operationWatcher);
			if (Passthru.IsPresent)
			{
				operationWatcher.WriteObject(operand);
			}
		}
	}

	private void ConfigureConnectionSettings(VMNetworkAdapterBase adapter, IOperationWatcher operationWatcher)
	{
		if (!(adapter is VMNetworkAdapter vMNetworkAdapter))
		{
			if (NotMonitoredInCluster.HasValue || IsParameterSpecified("DynamicMacAddress") || IsParameterSpecified("AllowPacketDirect") || IsParameterSpecified("NumaAwarePlacement") || IsParameterSpecified("StaticMacAddress") || IsParameterSpecified("MandatoryFeatureId") || IsParameterSpecified("ResourcePoolName") || IsParameterSpecified("TestReplicaPoolName") || IsParameterSpecified("TestReplicaSwitchName"))
			{
				throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.SetVMNetworkAdapter_ParameterInvalidForManagementOS);
			}
			return;
		}
		if (NotMonitoredInCluster.HasValue)
		{
			vMNetworkAdapter.ClusterMonitored = !NotMonitoredInCluster.Value;
		}
		if (IsParameterSpecified("DynamicMacAddress"))
		{
			vMNetworkAdapter.DynamicMacAddressEnabled = DynamicMacAddress.IsPresent;
		}
		if (IsParameterSpecified("AllowPacketDirect"))
		{
			vMNetworkAdapter.AllowPacketDirect = AllowPacketDirect;
		}
		if (NumaAwarePlacement.HasValue)
		{
			vMNetworkAdapter.NumaAwarePlacement = NumaAwarePlacement.Value;
		}
		if (InterruptModeration.HasValue)
		{
			vMNetworkAdapter.InterruptModeration = InterruptModeration.Value;
		}
		if (IsParameterSpecified("StaticMacAddress"))
		{
			vMNetworkAdapter.MacAddress = StaticMacAddress;
			vMNetworkAdapter.DynamicMacAddressEnabled = false;
		}
		if (IsParameterSpecified("MandatoryFeatureId"))
		{
			vMNetworkAdapter.MandatoryFeatureId = MandatoryFeatureId;
		}
		if (IsParameterSpecified("ResourcePoolName"))
		{
			vMNetworkAdapter.PoolName = ResourcePoolName;
		}
		if (IsParameterSpecified("TestReplicaPoolName"))
		{
			vMNetworkAdapter.TestReplicaPoolName = TestReplicaPoolName;
		}
		if (IsParameterSpecified("TestReplicaSwitchName"))
		{
			vMNetworkAdapter.TestReplicaSwitchName = TestReplicaSwitchName;
		}
		if (DeviceNaming.HasValue)
		{
			vMNetworkAdapter.DeviceNaming = DeviceNaming.Value;
		}
		((IUpdatable)adapter).Put(operationWatcher);
	}

	private void ConfigureFeatureSettings(VMNetworkAdapterBase adapter, IOperationWatcher operationWatcher)
	{
		ConfigureSecuritySettings(adapter, operationWatcher);
		ConfigureOffloadSettings(adapter, operationWatcher);
		ConfigureBandwidthSettings(adapter, operationWatcher);
	}

	private void ConfigureSecuritySettings(VMNetworkAdapterBase adapter, IOperationWatcher operationWatcher)
	{
		VMNetworkAdapterSecuritySetting vMNetworkAdapterSecuritySetting = adapter.SecuritySetting;
		if (vMNetworkAdapterSecuritySetting == null)
		{
			vMNetworkAdapterSecuritySetting = VMNetworkAdapterSecuritySetting.CreateTemplateSecuritySetting(adapter);
		}
		bool flag = false;
		if (MacAddressSpoofing.HasValue)
		{
			vMNetworkAdapterSecuritySetting.MacAddressSpoofing = MacAddressSpoofing.Value;
			flag = true;
		}
		if (DhcpGuard.HasValue)
		{
			vMNetworkAdapterSecuritySetting.DhcpGuard = DhcpGuard.Value;
			flag = true;
		}
		if (RouterGuard.HasValue)
		{
			vMNetworkAdapterSecuritySetting.RouterGuard = RouterGuard.Value;
			flag = true;
		}
		if (PortMirroring.HasValue)
		{
			vMNetworkAdapterSecuritySetting.PortMirroringMode = PortMirroring.Value;
			flag = true;
		}
		if (IeeePriorityTag.HasValue)
		{
			vMNetworkAdapterSecuritySetting.IeeePriorityTag = IeeePriorityTag.Value;
			flag = true;
		}
		if (VirtualSubnetId.HasValue)
		{
			vMNetworkAdapterSecuritySetting.VirtualSubnetId = VirtualSubnetId.Value;
			flag = true;
		}
		if (AllowTeaming.HasValue)
		{
			vMNetworkAdapterSecuritySetting.AllowTeaming = AllowTeaming.Value;
			flag = true;
		}
		if (StormLimit.HasValue)
		{
			vMNetworkAdapterSecuritySetting.StormLimit = StormLimit.Value;
			flag = true;
		}
		if (DynamicIPAddressLimit.HasValue)
		{
			vMNetworkAdapterSecuritySetting.DynamicIPAddressLimit = DynamicIPAddressLimit.Value;
			flag = true;
		}
		if (FixSpeed10G.HasValue)
		{
			vMNetworkAdapterSecuritySetting.FixSpeed10G = FixSpeed10G.Value;
			flag = true;
		}
		if (flag)
		{
			adapter.AddOrModifyOneFeatureSetting(vMNetworkAdapterSecuritySetting, operationWatcher);
		}
	}

	private void ConfigureOffloadSettings(VMNetworkAdapterBase adapter, IOperationWatcher operationWatcher)
	{
		VMNetworkAdapterOffloadSetting vMNetworkAdapterOffloadSetting = adapter.OffloadSetting;
		if (vMNetworkAdapterOffloadSetting == null)
		{
			vMNetworkAdapterOffloadSetting = VMNetworkAdapterOffloadSetting.CreateTemplateOffloadSetting(adapter);
		}
		bool flag = false;
		if (VmqWeight.HasValue)
		{
			vMNetworkAdapterOffloadSetting.VMQWeight = VmqWeight.Value;
			flag = true;
		}
		if (IPsecOffloadMaximumSecurityAssociation.HasValue)
		{
			vMNetworkAdapterOffloadSetting.IPSecOffloadMaxSA = IPsecOffloadMaximumSecurityAssociation.Value;
			flag = true;
		}
		if (IovWeight.HasValue)
		{
			vMNetworkAdapterOffloadSetting.IovWeight = IovWeight.Value;
			flag = true;
		}
		if (IovQueuePairsRequested.HasValue)
		{
			vMNetworkAdapterOffloadSetting.IOVQueuePairsRequested = IovQueuePairsRequested.Value;
			flag = true;
		}
		if (IovInterruptModeration.HasValue)
		{
			vMNetworkAdapterOffloadSetting.IovInterruptModeration = IovInterruptModeration.Value;
			flag = true;
		}
		if (PacketDirectNumProcs.HasValue)
		{
			vMNetworkAdapterOffloadSetting.PacketDirectNumProcs = PacketDirectNumProcs.Value;
			flag = true;
		}
		if (PacketDirectModerationCount.HasValue)
		{
			vMNetworkAdapterOffloadSetting.PacketDirectModerationCount = PacketDirectModerationCount.Value;
			flag = true;
		}
		if (PacketDirectModerationInterval.HasValue)
		{
			vMNetworkAdapterOffloadSetting.PacketDirectModerationInterval = PacketDirectModerationInterval.Value;
			flag = true;
		}
		if (VrssEnabled.HasValue)
		{
			vMNetworkAdapterOffloadSetting.VrssEnabled = VrssEnabled.Value;
			flag = true;
		}
		if (VmmqEnabled.HasValue)
		{
			vMNetworkAdapterOffloadSetting.VmmqEnabled = VmmqEnabled.Value;
			flag = true;
		}
		if (VrssMaxQueuePairs.HasValue)
		{
			vMNetworkAdapterOffloadSetting.VrssMaxQueuePairs = VrssMaxQueuePairs.Value;
			flag = true;
		}
		if (VrssMinQueuePairs.HasValue)
		{
			vMNetworkAdapterOffloadSetting.VrssMinQueuePairs = VrssMinQueuePairs.Value;
			flag = true;
		}
		if (VrssQueueSchedulingMode.HasValue)
		{
			vMNetworkAdapterOffloadSetting.VrssQueueSchedulingMode = VrssQueueSchedulingMode.Value;
			flag = true;
		}
		if (VrssExcludePrimaryProcessor.HasValue)
		{
			vMNetworkAdapterOffloadSetting.VrssExcludePrimaryProcessor = VrssExcludePrimaryProcessor.Value;
			flag = true;
		}
		if (VrssIndependentHostSpreading.HasValue)
		{
			vMNetworkAdapterOffloadSetting.VrssIndependentHostSpreading = VrssIndependentHostSpreading.Value;
			flag = true;
		}
		if (VrssVmbusChannelAffinityPolicy.HasValue)
		{
			vMNetworkAdapterOffloadSetting.VrssVmbusChannelAffinityPolicy = VrssVmbusChannelAffinityPolicy.Value;
			flag = true;
		}
		if (flag)
		{
			adapter.AddOrModifyOneFeatureSetting(vMNetworkAdapterOffloadSetting, operationWatcher);
		}
	}

	private void ConfigureBandwidthSettings(VMNetworkAdapterBase adapter, IOperationWatcher operationWatcher)
	{
		if (MinimumBandwidthAbsolute.HasValue || MinimumBandwidthWeight.HasValue)
		{
			VMSwitch connectedSwitch = adapter.GetConnectedSwitch();
			if (connectedSwitch != null && connectedSwitch.BandwidthReservationMode == VMSwitchBandwidthMode.None)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_InvalidBandwidthReservationMode);
			}
		}
		VMNetworkAdapterBandwidthSetting vMNetworkAdapterBandwidthSetting = adapter.BandwidthSetting;
		if (vMNetworkAdapterBandwidthSetting == null)
		{
			vMNetworkAdapterBandwidthSetting = VMNetworkAdapterBandwidthSetting.CreateTemplateBandwidthSetting(adapter);
		}
		bool flag = false;
		if (MaximumBandwidth.HasValue)
		{
			vMNetworkAdapterBandwidthSetting.MaximumBandwidth = MaximumBandwidth.Value;
			flag = true;
		}
		if (MinimumBandwidthAbsolute.HasValue)
		{
			vMNetworkAdapterBandwidthSetting.MinimumBandwidthAbsolute = MinimumBandwidthAbsolute.Value;
			flag = true;
		}
		if (MinimumBandwidthWeight.HasValue)
		{
			vMNetworkAdapterBandwidthSetting.MinimumBandwidthWeight = MinimumBandwidthWeight.Value;
			flag = true;
		}
		if (flag)
		{
			adapter.AddOrModifyOneFeatureSetting(vMNetworkAdapterBandwidthSetting, operationWatcher);
		}
	}
}
