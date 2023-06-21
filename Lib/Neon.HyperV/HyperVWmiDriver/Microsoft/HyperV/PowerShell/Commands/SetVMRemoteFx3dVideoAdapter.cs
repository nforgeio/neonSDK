using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "d", Justification = "This is from spec.")]
[Cmdlet("Set", "VMRemoteFx3dVideoAdapter", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMRemoteFx3DVideoAdapter) })]
internal sealed class SetVMRemoteFx3dVideoAdapter : VirtualizationCmdlet<VMRemoteFx3DVideoAdapter>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, ISupportsPassthrough
{
    private static class ParameterSetNames
    {
        public const string AdapterObject = "Object";
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public string[] VMName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VirtualMachine[] VM { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "d", Justification = "This is from spec.")]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Object")]
    public VMRemoteFx3DVideoAdapter[] VMRemoteFx3dVideoAdapter { get; set; }

    [ValidateNotNull]
    [Parameter(Position = 1)]
    public byte? MonitorCount { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Position = 2)]
    public string MaximumResolution { get; set; }

    [ValidateNotNull]
    [Parameter(Position = 3)]
    public ulong? VRAMSizeBytes { get; set; }

    [Parameter]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    public SwitchParameter Passthru { get; set; }

    internal override IList<VMRemoteFx3DVideoAdapter> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        WriteWarning(string.Format(CultureInfo.CurrentCulture, CmdletResources.RemoteFX_CmdletWarning, "warning"));
        if (CurrentParameterSetIs("Object"))
        {
            return VMRemoteFx3dVideoAdapter;
        }
        return (from vm in ParameterResolvers.ResolveVirtualMachines(this, operationWatcher)
            select vm.RemoteFxAdapter into adapter
            where adapter != null
            select adapter).ToList();
    }

    internal override void ProcessOneOperand(VMRemoteFx3DVideoAdapter operand, IOperationWatcher operationWatcher)
    {
        if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMRemoteFx3dVideoAdapter, operand.Name)))
        {
            return;
        }
        if (MonitorCount.HasValue)
        {
            operand.MaximumMonitors = MonitorCount.Value;
            if (!operand.IsMonitorCountInRange())
            {
                string[] array = operand.RetrieveMonitorCounts();
                throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMSynth3dVideoAdapter_InvalidMonitorCount, operand.MaximumMonitors, array[0], array[1]));
            }
        }
        if (!string.IsNullOrEmpty(MaximumResolution))
        {
            if (!operand.IsResolutionInRange(MaximumResolution, mappingBased: false))
            {
                throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMSynth3dVideoAdapter_InvalidResolution, MaximumResolution, operand.RetrieveResolutions()));
            }
            operand.MaximumScreenResolution = MaximumResolution;
        }
        if (VRAMSizeBytes.HasValue)
        {
            if (!operand.IsPermissibleToSetVram())
            {
                throw ExceptionHelper.CreateInvalidOperationException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VramSynth3dVideoAdapter_InvalidOperationOnVramSize, operand.GetVMConfigurationVersion()), null, operand);
            }
            operand.VRAMSizeBytes = VRAMSizeBytes.Value;
            if (!operand.IsVramSizeInRange())
            {
                throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMSynth3dVideoAdapter_InvalidVramSize, VRAMSizeBytes.Value, operand.RetrieveVramSizes(mappingBased: false)));
            }
        }
        if (!operand.IsResolutionInRange(operand.MaximumScreenResolution))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMSynth3dVideoAdapter_InvalidResolutionBasedOnMaping, operand.MaximumScreenResolution, operand.MaximumMonitors, operand.RetrieveResolutions()));
        }
        if (operand.IsPermissibleToSetVram() && !operand.IsVramSizeEnough())
        {
            if (VRAMSizeBytes.HasValue)
            {
                throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMSynth3dVideoAdapter_NotEnoughVramSize, operand.VRAMSizeBytes, operand.MaximumMonitors, operand.MaximumScreenResolution, operand.RetrieveVramSizes()));
            }
            if (!operand.AdjustVramSize())
            {
                throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMSynth3dVideoAdapter_NotEnoughVramSize, operand.VRAMSizeBytes, operand.MaximumMonitors, operand.MaximumScreenResolution, operand.RetrieveVramSizes()));
            }
        }
        ((IUpdatable)operand).Put(operationWatcher);
        if (Passthru.IsPresent)
        {
            operationWatcher.WriteObject(operand);
        }
    }
}
