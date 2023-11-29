using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Set", "VMSwitch", SupportsShouldProcess = true, DefaultParameterSetName = "SwitchName_SwitchType")]
[OutputType(new Type[] { typeof(VMSwitch) })]
internal sealed class SetVMSwitch : VirtualizationCmdlet<VMSwitch>, ISupportsPassthrough
{
    internal static class ParameterSetNames
    {
        public const string SwitchNameSwitchType = "SwitchName_SwitchType";

        public const string SwitchNameNetAdapterInterfaceDescription = "SwitchName_NetAdapterInterfaceDescription";

        public const string SwitchNameNetAdapterName = "SwitchName_NetAdapterName";

        public const string SwitchObjectSwitchType = "SwitchObject_SwitchType";

        public const string SwitchObjectNetAdapterName = "SwitchObject_NetAdapterName";

        public const string SwitchObjectNetAdapterInterfaceDescription = "SwitchObject_NetAdapterInterfaceDescription";
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "SwitchName_SwitchType", ValueFromPipelineByPropertyName = true)]
    [Parameter(ParameterSetName = "SwitchName_NetAdapterInterfaceDescription", ValueFromPipelineByPropertyName = true)]
    [Parameter(ParameterSetName = "SwitchName_NetAdapterName", ValueFromPipelineByPropertyName = true)]
    [Parameter(ParameterSetName = "SwitchObject_NetAdapterInterfaceDescription")]
    [Parameter(ParameterSetName = "SwitchObject_NetAdapterName")]
    [Parameter(ParameterSetName = "SwitchObject_SwitchType")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "SwitchName_SwitchType")]
    [Parameter(ParameterSetName = "SwitchName_NetAdapterInterfaceDescription")]
    [Parameter(ParameterSetName = "SwitchName_NetAdapterName")]
    [Parameter(ParameterSetName = "SwitchObject_NetAdapterInterfaceDescription")]
    [Parameter(ParameterSetName = "SwitchObject_NetAdapterName")]
    [Parameter(ParameterSetName = "SwitchObject_SwitchType")]
    [Alias(new string[] { "PSComputerName" })]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "SwitchName_SwitchType")]
    [Parameter(ParameterSetName = "SwitchName_NetAdapterInterfaceDescription")]
    [Parameter(ParameterSetName = "SwitchName_NetAdapterName")]
    [Parameter(ParameterSetName = "SwitchObject_NetAdapterInterfaceDescription")]
    [Parameter(ParameterSetName = "SwitchObject_NetAdapterName")]
    [Parameter(ParameterSetName = "SwitchObject_SwitchType")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec, and array is more user friendly as this is an user input parameter.")]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "SwitchObject_SwitchType")]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "SwitchObject_NetAdapterName")]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "SwitchObject_NetAdapterInterfaceDescription")]
    public VMSwitch[] VMSwitch { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Array is more user friendly, and this is a user input parameter.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "SwitchName_SwitchType")]
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "SwitchName_NetAdapterInterfaceDescription")]
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "SwitchName_NetAdapterName")]
    [Alias(new string[] { "SwitchName" })]
    public string[] Name { get; set; }

    [ValidateNotNullOrEmpty]
    [Alias(new string[] { "InterfaceAlias" })]
    [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true, ParameterSetName = "SwitchName_NetAdapterName")]
    [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true, ParameterSetName = "SwitchObject_NetAdapterName")]
    public string NetAdapterName { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "SwitchName_NetAdapterInterfaceDescription")]
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "SwitchObject_NetAdapterInterfaceDescription")]
    public string NetAdapterInterfaceDescription { get; set; }

    [Parameter(ParameterSetName = "SwitchName_SwitchType")]
    [Parameter(ParameterSetName = "SwitchObject_SwitchType")]
    public VMSwitchType? SwitchType { get; set; }

    [Parameter]
    public bool? AllowManagementOS { get; set; }

    [ValidateNotNull]
    [Parameter]
    public long? DefaultFlowMinimumBandwidthAbsolute { get; set; }

    [ValidateNotNull]
    [Parameter]
    public long? DefaultFlowMinimumBandwidthWeight { get; set; }

    [Parameter]
    public bool? DefaultQueueVrssEnabled { get; set; }

    [Parameter]
    public bool? DefaultQueueVmmqEnabled { get; set; }

    [ValidateNotNull]
    [Parameter]
    [Alias(new string[] { "DefaultQueueVmmqQueuePairs" })]
    public uint? DefaultQueueVrssMaxQueuePairs { get; set; }

    [ValidateNotNull]
    [Parameter]
    public uint? DefaultQueueVrssMinQueuePairs { get; set; }

    [ValidateNotNull]
    [Parameter]
    public VrssQueueSchedulingModeType? DefaultQueueVrssQueueSchedulingMode { get; set; }

    [Parameter]
    public bool? DefaultQueueVrssExcludePrimaryProcessor { get; set; }

    [Parameter]
    public bool? EnableSoftwareRsc { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Array is more user friendly, and this is a user input parameter.")]
    [ValidateNotNullOrEmpty]
    [Parameter]
    public VMSwitchExtension[] Extensions { get; set; }

    [ValidateNotNull]
    [Parameter]
    public string Notes { get; set; }

    [Parameter]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is per spec. Also Passthru is a standard PowerShell cmdlet parameter name.")]
    public SwitchParameter Passthru { get; set; }

    protected override void NormalizeParameters()
    {
        base.NormalizeParameters();
        if (!IsParameterSpecified("SwitchType") && (IsParameterSpecified("NetAdapterName") || IsParameterSpecified("NetAdapterInterfaceDescription")))
        {
            SwitchType = VMSwitchType.External;
        }
        if (!IsParameterSpecified("AllowManagementOS") && SwitchType == VMSwitchType.External)
        {
            AllowManagementOS = true;
        }
    }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();
        if (SwitchType == VMSwitchType.External && !IsParameterSpecified("NetAdapterInterfaceDescription") && !IsParameterSpecified("NetAdapterName"))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.SetVMSwitch_NoNetworkAdapterSpecified);
        }
        if (AllowManagementOS.GetValueOrDefault() && SwitchType.HasValue && SwitchType != VMSwitchType.External)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.SetVMSwitch_AllowManagementOSForExternalSwitchOnly);
        }
        ValidateMutuallyExclusiveParameters("DefaultFlowMinimumBandwidthAbsolute", "DefaultFlowMinimumBandwidthWeight");
        if (DefaultFlowMinimumBandwidthWeight.HasValue && (DefaultFlowMinimumBandwidthWeight.Value < 0 || DefaultFlowMinimumBandwidthWeight.Value > 100))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_InvalidBandwidthReservationRange);
        }
        if (DefaultQueueVrssMaxQueuePairs.HasValue && DefaultQueueVrssMaxQueuePairs.Value < 1)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_VrssMaxQueuePairsCountValue);
        }
        if (DefaultQueueVrssMinQueuePairs.HasValue && (DefaultQueueVrssMinQueuePairs.Value < 1 || (DefaultQueueVrssMaxQueuePairs.HasValue && DefaultQueueVrssMinQueuePairs.Value > DefaultQueueVrssMaxQueuePairs.Value)))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_VrssMinQueuePairsCountValue);
        }
        if (DefaultQueueVrssQueueSchedulingMode.HasValue && DefaultQueueVrssQueueSchedulingMode.Value != 0 && DefaultQueueVrssQueueSchedulingMode.Value != VrssQueueSchedulingModeType.StaticVrss && DefaultQueueVrssQueueSchedulingMode.Value != VrssQueueSchedulingModeType.StaticVmq)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_VrssQueueSchedulingModeValue);
        }
        if (EnableSoftwareRsc.GetValueOrDefault() && SwitchType.HasValue && SwitchType != VMSwitchType.External)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_EnableSoftwareRsc);
        }
    }

    internal override IList<VMSwitch> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        if (IsParameterSpecified("VMSwitch"))
        {
            return VMSwitch;
        }
        return global::Microsoft.HyperV.PowerShell.VMSwitch.GetSwitchesByNamesAndServers(ParameterResolvers.GetServers(this, operationWatcher), Name, allowWildcards: true, ErrorDisplayMode.WriteError, operationWatcher);
    }

    internal override void ProcessOneOperand(VMSwitch operand, IOperationWatcher operationWatcher)
    {
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMSwitch, operand.Name)))
        {
            ModifySwitchSettings(operand, operationWatcher);
            ModifySwitchConnections(operand, operationWatcher);
            ModifySwitchFeatures(operand, operationWatcher);
            if (Passthru.IsPresent)
            {
                operationWatcher.WriteObject(operand);
            }
        }
    }

    private void ModifySwitchSettings(VMSwitch virtualSwitch, IOperationWatcher operationWatcher)
    {
        if (Notes != null)
        {
            virtualSwitch.Notes = Notes;
        }
        if (Extensions != null)
        {
            virtualSwitch.SetExtensionOrder(Extensions);
        }
        ((IUpdatable)virtualSwitch).Put(operationWatcher);
    }

    private void ModifySwitchFeatures(VMSwitch virtualSwitch, IOperationWatcher operationWatcher)
    {
        VMSwitchBandwidthSetting vMSwitchBandwidthSetting = virtualSwitch.BandwidthSetting;
        if (vMSwitchBandwidthSetting == null)
        {
            vMSwitchBandwidthSetting = VMSwitchBandwidthSetting.CreateTemplateSwitchBandwidthSetting(virtualSwitch);
        }
        bool flag = false;
        if (DefaultFlowMinimumBandwidthAbsolute.HasValue)
        {
            vMSwitchBandwidthSetting.DefaultFlowReservation = DefaultFlowMinimumBandwidthAbsolute.Value;
            flag = true;
        }
        if (DefaultFlowMinimumBandwidthWeight.HasValue)
        {
            vMSwitchBandwidthSetting.DefaultFlowWeight = DefaultFlowMinimumBandwidthWeight.Value;
            flag = true;
        }
        if (flag)
        {
            virtualSwitch.AddOrModifyOneFeatureSetting(vMSwitchBandwidthSetting, operationWatcher);
        }
        VMSwitchOffloadSetting vMSwitchOffloadSetting = virtualSwitch.OffloadSetting;
        flag = false;
        if (vMSwitchOffloadSetting == null)
        {
            vMSwitchOffloadSetting = VMSwitchOffloadSetting.CreateTemplateSwitchOffloadSetting(virtualSwitch);
        }
        if (DefaultQueueVrssEnabled.HasValue)
        {
            vMSwitchOffloadSetting.DefaultQueueVrssEnabled = DefaultQueueVrssEnabled.Value;
            flag = true;
        }
        if (DefaultQueueVmmqEnabled.HasValue)
        {
            vMSwitchOffloadSetting.DefaultQueueVmmqEnabled = DefaultQueueVmmqEnabled.Value;
            flag = true;
        }
        if (DefaultQueueVrssMaxQueuePairs.HasValue)
        {
            vMSwitchOffloadSetting.DefaultQueueVrssMaxQueuePairs = DefaultQueueVrssMaxQueuePairs.Value;
            flag = true;
        }
        if (DefaultQueueVrssMinQueuePairs.HasValue)
        {
            vMSwitchOffloadSetting.DefaultQueueVrssMinQueuePairs = DefaultQueueVrssMinQueuePairs.Value;
            flag = true;
        }
        if (DefaultQueueVrssQueueSchedulingMode.HasValue)
        {
            vMSwitchOffloadSetting.DefaultQueueVrssQueueSchedulingMode = DefaultQueueVrssQueueSchedulingMode.Value;
            flag = true;
        }
        if (DefaultQueueVrssExcludePrimaryProcessor.HasValue)
        {
            vMSwitchOffloadSetting.DefaultQueueVrssExcludePrimaryProcessor = DefaultQueueVrssExcludePrimaryProcessor.Value;
            flag = true;
        }
        if (EnableSoftwareRsc.HasValue)
        {
            vMSwitchOffloadSetting.SoftwareRscEnabled = EnableSoftwareRsc.Value;
            flag = true;
        }
        if (flag)
        {
            virtualSwitch.AddOrModifyOneFeatureSetting(vMSwitchOffloadSetting, operationWatcher);
        }
    }

    private void ModifySwitchConnections(VMSwitch virtualSwitch, IOperationWatcher operationWatcher)
    {
        VMSwitchType vMSwitchType = SwitchType ?? virtualSwitch.SwitchType;
        bool allowManagementOS = AllowManagementOS ?? virtualSwitch.AllowManagementOS;
        if (virtualSwitch.EmbeddedTeamingEnabled && (!string.IsNullOrEmpty(NetAdapterName) || !string.IsNullOrEmpty(NetAdapterInterfaceDescription) || vMSwitchType != virtualSwitch.SwitchType))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.SetVMSwitch_TeamingSwitchCannotChangeTypeAndExtNics);
        }
        virtualSwitch.ConfigureConnections(vMSwitchType, allowManagementOS, string.IsNullOrEmpty(NetAdapterName) ? null : new string[1] { NetAdapterName }, string.IsNullOrEmpty(NetAdapterInterfaceDescription) ? null : new string[1] { NetAdapterInterfaceDescription }, operationWatcher);
    }
}
