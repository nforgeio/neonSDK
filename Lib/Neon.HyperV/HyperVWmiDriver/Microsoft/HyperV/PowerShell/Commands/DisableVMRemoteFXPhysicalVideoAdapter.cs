using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Disable", "VMRemoteFXPhysicalVideoAdapter", SupportsShouldProcess = true, DefaultParameterSetName = "GPUByName")]
[OutputType(new Type[] { typeof(VMRemoteFXPhysicalVideoAdapter) })]
internal sealed class DisableVMRemoteFXPhysicalVideoAdapter : VirtualizationCmdlet<VMRemoteFXPhysicalVideoAdapter>, ISupportsPassthrough
{
    internal static class ParameterSetNames
    {
        public const string GPUByName = "GPUByName";

        public const string GPUByObject = "GPUByObject";
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "GPUByName")]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "GPUByName")]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "GPUByName")]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "GPUByObject")]
    public VMRemoteFXPhysicalVideoAdapter[] GPU { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly, and this is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "GPUByName")]
    public string[] Name { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    internal override IList<VMRemoteFXPhysicalVideoAdapter> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        WriteWarning(string.Format(CultureInfo.CurrentCulture, CmdletResources.RemoteFX_CmdletWarning, "warning"));
        IList<VMRemoteFXPhysicalVideoAdapter> list = ((!CurrentParameterSetIs("GPUByName")) ? ((IList<VMRemoteFXPhysicalVideoAdapter>)GPU) : ((IList<VMRemoteFXPhysicalVideoAdapter>)VMRemoteFXPhysicalVideoAdapter.GetVmRemoteFxPhysicalVideoAdapters(ParameterResolvers.GetServers(this, operationWatcher), Name).ToList()));
        if (!list.Any())
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.NoDeviceFound);
        }
        return list;
    }

    internal override void ProcessOneOperand(VMRemoteFXPhysicalVideoAdapter operand, IOperationWatcher operationWatcher)
    {
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_DisableVMRemoteFXPhysicalVideoAdapter, operand.Name, operand.Server)))
        {
            operand.SetGPUEnabledForVirtualizationState(enabled: false, operationWatcher);
            if (Passthru.IsPresent)
            {
                operationWatcher.WriteObject(operand);
            }
        }
    }
}
