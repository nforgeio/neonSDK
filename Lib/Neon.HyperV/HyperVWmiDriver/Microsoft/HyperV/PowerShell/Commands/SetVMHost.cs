#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Set", "VMHost", SupportsShouldProcess = true, DefaultParameterSetName = "ComputerName")]
[OutputType(new Type[] { typeof(VMHost) })]
internal sealed class SetVMHost : VirtualizationCmdlet<VMHost>, ISupportsPassthrough
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = "CimSession")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(Position = 0, ParameterSetName = "ComputerName")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(Position = 1, ParameterSetName = "ComputerName")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [Parameter]
    public uint MaximumStorageMigrations { get; set; }

    [Parameter]
    public uint MaximumVirtualMachineMigrations { get; set; }

    [Parameter]
    public MigrationAuthenticationType VirtualMachineMigrationAuthenticationType { get; set; }

    [Parameter]
    public bool UseAnyNetworkForMigration { get; set; }

    [Parameter]
    public VMMigrationPerformance VirtualMachineMigrationPerformanceOption { get; set; }

    [ValidateNotNull]
    [Parameter]
    public TimeSpan? ResourceMeteringSaveInterval { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter]
    public string VirtualHardDiskPath { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter]
    public string VirtualMachinePath { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter]
    public string MacAddressMaximum { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter]
    public string MacAddressMinimum { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Wwnn", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter]
    public string FibreChannelWwnn { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Wwpn", Justification = "This is per spec.")]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fibre", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter]
    public string FibreChannelWwpnMaximum { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Wwpn", Justification = "This is per spec.")]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fibre", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter]
    public string FibreChannelWwpnMinimum { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Numa", Justification = "This is by spec.")]
    [Parameter]
    public bool? NumaSpanningEnabled { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "EnhancedMode", Justification = "This is by spec.")]
    [Parameter]
    public bool? EnableEnhancedSessionMode { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();
        if (IsParameterSpecified("MacAddressMaximum"))
        {
            MacAddressMaximum = ParameterResolvers.ValidateAndNormalizeMacAddress(MacAddressMaximum, "MacAddressMaximum");
        }
        if (IsParameterSpecified("MacAddressMinimum"))
        {
            MacAddressMinimum = ParameterResolvers.ValidateAndNormalizeMacAddress(MacAddressMinimum, "MacAddressMinimum");
        }
    }

    internal override IList<VMHost> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return VirtualizationObjectLocator.GetVMHosts(ParameterResolvers.GetServers(this, operationWatcher), operationWatcher);
    }

    internal override void ProcessOneOperand(VMHost host, IOperationWatcher operationWatcher)
    {
        if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMHost, host.Server)))
        {
            return;
        }
        ValidateUserHasSpecifiedArgumentsOrThrow(host);
        if (!string.IsNullOrEmpty(VirtualHardDiskPath))
        {
            host.VirtualHardDiskPath = VirtualHardDiskPath;
        }
        if (!string.IsNullOrEmpty(VirtualMachinePath))
        {
            host.VirtualMachinePath = VirtualMachinePath;
        }
        if (!string.IsNullOrEmpty(MacAddressMaximum))
        {
            host.MacAddressMaximum = MacAddressMaximum;
        }
        if (!string.IsNullOrEmpty(MacAddressMinimum))
        {
            host.MacAddressMinimum = MacAddressMinimum;
        }
        if (!string.IsNullOrEmpty(FibreChannelWwnn))
        {
            host.FibreChannelWwnn = FibreChannelWwnn;
        }
        if (!string.IsNullOrEmpty(FibreChannelWwpnMaximum))
        {
            host.FibreChannelWwpnMaximum = FibreChannelWwpnMaximum;
        }
        if (!string.IsNullOrEmpty(FibreChannelWwpnMinimum))
        {
            host.FibreChannelWwpnMinimum = FibreChannelWwpnMinimum;
        }
        if (ResourceMeteringSaveInterval.HasValue)
        {
            host.ResourceMeteringSaveInterval = ResourceMeteringSaveInterval.Value;
        }
        if (UserHasSpecifiedLiveMigrationArguments())
        {
            if (!host.GetIsLiveMigrationSupported())
            {
                throw ExceptionHelper.CreateOperationFailedException(CmdletErrorMessages.OperationFailed_HostDoesNotSupportLM);
            }
            if (IsParameterSpecified("UseAnyNetworkForMigration"))
            {
                if (UseAnyNetworkForMigration)
                {
                    host.AddAnyNetwork();
                }
                else
                {
                    host.RemoveAnyNetwork();
                }
            }
            if (IsParameterSpecified("MaximumVirtualMachineMigrations"))
            {
                host.MaximumVirtualMachineMigrations = MaximumVirtualMachineMigrations;
            }
            if (IsParameterSpecified("VirtualMachineMigrationAuthenticationType") && host.VirtualMachineMigrationAuthenticationType != VirtualMachineMigrationAuthenticationType)
            {
                host.VirtualMachineMigrationAuthenticationType = VirtualMachineMigrationAuthenticationType;
                if (VirtualMachineMigrationAuthenticationType == MigrationAuthenticationType.CredSSP)
                {
                    VMTrace.TraceWarning(CmdletErrorMessages.Warning_CredSSP);
                }
            }
            if (IsParameterSpecified("VirtualMachineMigrationPerformanceOption"))
            {
                host.SetVirtualMigrationPerformanceOption(VirtualMachineMigrationPerformanceOption);
            }
        }
        if (IsParameterSpecified("MaximumStorageMigrations"))
        {
            host.MaximumStorageMigrations = MaximumStorageMigrations;
        }
        if (NumaSpanningEnabled.HasValue && host.NumaSpanningEnabled != NumaSpanningEnabled.Value)
        {
            host.NumaSpanningEnabled = NumaSpanningEnabled.Value;
            VMTrace.TraceWarning(CmdletErrorMessages.NumaSpanningChangeNeedsVmmsReboot);
        }
        if (EnableEnhancedSessionMode.HasValue)
        {
            host.EnableEnhancedSessionMode = EnableEnhancedSessionMode.Value;
        }
        ((IUpdatable)host).Put(operationWatcher);
        if (Passthru.IsPresent)
        {
            operationWatcher.WriteObject(host);
        }
    }

    private bool UserHasSpecifiedLiveMigrationArguments()
    {
        if (!IsParameterSpecified("MaximumVirtualMachineMigrations") && !IsParameterSpecified("UseAnyNetworkForMigration") && !IsParameterSpecified("VirtualMachineMigrationAuthenticationType"))
        {
            return IsParameterSpecified("VirtualMachineMigrationPerformanceOption");
        }
        return true;
    }

    private void ValidateUserHasSpecifiedArgumentsOrThrow(VirtualizationObject target)
    {
        if (IsParameterSpecified("EnableEnhancedSessionMode") || IsParameterSpecified("FibreChannelWwnn") || IsParameterSpecified("FibreChannelWwpnMaximum") || IsParameterSpecified("FibreChannelWwpnMinimum") || IsParameterSpecified("MacAddressMaximum") || IsParameterSpecified("MacAddressMinimum") || IsParameterSpecified("MaximumStorageMigrations") || IsParameterSpecified("NumaSpanningEnabled") || IsParameterSpecified("ResourceMeteringSaveInterval") || IsParameterSpecified("VirtualHardDiskPath") || IsParameterSpecified("VirtualMachinePath") || UserHasSpecifiedLiveMigrationArguments())
        {
            return;
        }
        throw new VirtualizationException(ErrorMessages.InvalidOperation_SetVMHostNoParameterProvided, null, "InvalidOperation", ErrorCategory.InvalidOperation, target);
    }
}
