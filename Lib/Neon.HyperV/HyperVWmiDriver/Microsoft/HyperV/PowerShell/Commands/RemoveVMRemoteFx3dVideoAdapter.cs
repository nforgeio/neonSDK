using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "d", Justification = "This is from spec.")]
[Cmdlet("Remove", "VMRemoteFx3dVideoAdapter", SupportsShouldProcess = true, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMRemoteFx3DVideoAdapter) })]
internal sealed class RemoveVMRemoteFx3dVideoAdapter : VirtualizationCmdlet<VMRemoteFx3DVideoAdapter>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, ISupportsPassthrough
{
    private static class AdditionalParameterSetNames
    {
        public const string RemoteFXAdapter = "RemoteFXAdapter";
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VirtualMachine[] VM { get; set; }

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

    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "d", Justification = "This is from spec.")]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and arrays are easier to use.")]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "RemoteFXAdapter")]
    [ValidateNotNullOrEmpty]
    public VMRemoteFx3DVideoAdapter[] VMRemoteFx3dVideoAdapter { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    internal override IList<VMRemoteFx3DVideoAdapter> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        WriteWarning(string.Format(CultureInfo.CurrentCulture, CmdletResources.RemoteFX_CmdletWarning, "warning"));
        if (CurrentParameterSetIs("RemoteFXAdapter"))
        {
            return VMRemoteFx3dVideoAdapter;
        }
        return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectWithLogging(GetAdapterIfFound, operationWatcher).ToList();
    }

    internal override void ProcessOneOperand(VMRemoteFx3DVideoAdapter operand, IOperationWatcher operationWatcher)
    {
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RemoveVMRemoteFx3dVideoAdapter, operand.Name, operand.GetParentAs<VirtualMachineBase>().Name)))
        {
            VMRemoteFx3DVideoAdapter.RemoveSynthetic3DDisplayController(operand, operationWatcher);
            if ((bool)Passthru)
            {
                operationWatcher.WriteObject(operand);
            }
        }
    }

    private static VMRemoteFx3DVideoAdapter GetAdapterIfFound(VirtualMachine vm)
    {
        return vm.GetSynthetic3DDisplayController() ?? throw ExceptionHelper.CreateOperationFailedException(CmdletErrorMessages.NoDeviceFound);
    }
}
