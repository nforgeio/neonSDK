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
[Cmdlet("Get", "VMRemoteFx3dVideoAdapter", DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMRemoteFx3DVideoAdapter) })]
internal sealed class GetVMRemoteFx3dVideoAdapter : VirtualizationCmdlet<VMRemoteFx3DVideoAdapter>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, IVMSnapshotCmdlet
{
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

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMSnapshot")]
    [Alias(new string[] { "VMCheckpoint" })]
    public VMSnapshot VMSnapshot { get; set; }

    internal override IList<VMRemoteFx3DVideoAdapter> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        WriteWarning(string.Format(CultureInfo.CurrentCulture, CmdletResources.RemoteFX_CmdletWarning, "warning"));
        IEnumerable<VirtualMachineBase> enumerable = null;
        enumerable = ((!CurrentParameterSetIs("VMSnapshot")) ? ((IEnumerable<VirtualMachineBase>)ParameterResolvers.ResolveVirtualMachines(this, operationWatcher)) : ((IEnumerable<VirtualMachineBase>)new VirtualMachineBase[1] { VMSnapshot }));
        return (from vm in enumerable
            select vm.RemoteFxAdapter into adapter
            where adapter != null
            select adapter).ToList();
    }

    internal override void ProcessOneOperand(VMRemoteFx3DVideoAdapter operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}
