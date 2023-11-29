using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Remove", "VMGpuPartitionAdapter", SupportsShouldProcess = true, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMGpuPartitionAdapter) })]
internal sealed class RemoveVMGpuPartitionAdapter : VirtualizationCmdlet<VMGpuPartitionAdapter>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, ISupportsPassthrough
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [Parameter(ValueFromPipeline = true, Position = 0, Mandatory = true, ParameterSetName = "VMObject")]
    [ValidateNotNullOrEmpty]
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
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "VMName")]
    public string[] VMName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Object")]
    [ValidateNotNullOrEmpty]
    public VMGpuPartitionAdapter[] VMGpuPartitionAdapter { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "VMObject")]
    public string AdapterId { get; set; }

    internal override IList<VMGpuPartitionAdapter> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        if (CurrentParameterSetIs("Object"))
        {
            return VMGpuPartitionAdapter;
        }
        IEnumerable<VMGpuPartitionAdapter> enumerable = ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectManyWithLogging((VirtualMachine vm) => vm.GetGpuPartitionAdapters(AdapterId), operationWatcher);
        if (enumerable == null || !enumerable.Any())
        {
            throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.VMGpuPartitionAdapter_NotFound, null);
        }
        return enumerable.ToList();
    }

    internal override void ProcessOneOperand(VMGpuPartitionAdapter operand, IOperationWatcher operationWatcher)
    {
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RemoveVMGpuPartitionAdapter, operand.Name, operand.VMName)))
        {
            ((IRemovable)operand).Remove(operationWatcher);
            if ((bool)Passthru)
            {
                operationWatcher.WriteObject(operand);
            }
        }
    }
}
