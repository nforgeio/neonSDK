using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMSnapshot", DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMSnapshot) })]
internal sealed class GetVMSnapshot : VirtualizationCmdlet<VMSnapshot>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user-friendly.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMName")]
    public string[] VMName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user-friendly.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMObject")]
    public VirtualMachine[] VM { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "Id")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "Id")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "Id")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [ValidateNotNull]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "Id")]
    public Guid? Id { get; set; }

    [Parameter(Position = 0, ParameterSetName = "Child")]
    [Parameter(Position = 0, ParameterSetName = "Parent")]
    [Parameter(Position = 1, ParameterSetName = "VMName")]
    [Parameter(Position = 1, ParameterSetName = "VMObject")]
    [ValidateNotNullOrEmpty]
    public string Name { get; set; }

    [ValidateNotNull]
    [Parameter(Mandatory = true, ParameterSetName = "Child")]
    public VMSnapshot ChildOf { get; set; }

    [ValidateNotNull]
    [Parameter(Mandatory = true, ParameterSetName = "Parent")]
    public VirtualMachineBase ParentOf { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "Parent")]
    [Parameter(ParameterSetName = "Child")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "VMObject")]
    [Alias(new string[] { "VMRecoveryCheckpoint" })]
    public SnapshotType SnapshotType { get; set; }

    internal override IList<VMSnapshot> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        IEnumerable<VMSnapshot> source = (CurrentParameterSetIs("Id") ? ParameterResolvers.GetServers(this, operationWatcher).SelectWithLogging((Server server) => VirtualizationObjectLocator.GetVMSnapshotById(server, Id.Value), operationWatcher) : (CurrentParameterSetIs("Parent") ? new VMSnapshot[1] { ParentOf.GetParentSnapshot() } : ((!CurrentParameterSetIs("Child")) ? ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectMany((VirtualMachine vm) => vm.GetVMSnapshots()) : ChildOf.GetChildSnapshots())));
        if (IsParameterSpecified("SnapshotType"))
        {
            source = source.Where((VMSnapshot snapshot) => snapshot.SnapshotType == SnapshotType);
        }
        if (!string.IsNullOrEmpty(Name))
        {
            WildcardPattern pattern = new WildcardPattern(Name, WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
            source = source.Where((VMSnapshot snapshot) => pattern.IsMatch(snapshot.Name));
        }
        return source.OrderBy((VMSnapshot snapshot) => snapshot.CreationTime).ToList();
    }

    internal override void ValidateOperandList(IList<VMSnapshot> operands, IOperationWatcher operationWatcher)
    {
        base.ValidateOperandList(operands, operationWatcher);
        if (!operands.Any() && (VirtualMachineParameterType == VirtualMachineParameterType.None || IsParameterSpecified("SnapshotType") || !string.IsNullOrEmpty(Name)))
        {
            throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.SnapshotNotFound, null);
        }
    }

    internal override void ProcessOneOperand(VMSnapshot operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}
