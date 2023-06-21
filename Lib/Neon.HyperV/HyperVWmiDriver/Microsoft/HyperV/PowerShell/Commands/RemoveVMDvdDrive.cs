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

[Cmdlet("Remove", "VMDvdDrive", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(DvdDrive) })]
internal sealed class RemoveVMDvdDrive : VirtualizationCmdlet<DvdDrive>, ISupportsPassthrough, IVmBySingularVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet
{
    internal static class ParameterSetNames
    {
        public const string VMName = "VMName";

        public const string VMDvdDrive = "VMDvdDrive";
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

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "VMName")]
    public string VMName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and arrays are easier to use.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "VMDvdDrive", ValueFromPipeline = true)]
    public DvdDrive[] VMDvdDrive { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "VMName")]
    public int? ControllerNumber { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 2, ParameterSetName = "VMName")]
    public int? ControllerLocation { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    internal override IList<DvdDrive> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        if (VMDvdDrive != null)
        {
            return VMDvdDrive;
        }
        return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectManyWithLogging((VirtualMachine vm) => vm.FindDrives((vm.Generation == 2) ? ControllerType.SCSI : ControllerType.IDE, ControllerNumber, ControllerLocation), operationWatcher).OfType<DvdDrive>()
            .ToList();
    }

    internal override void ProcessOneOperand(DvdDrive operand, IOperationWatcher operationWatcher)
    {
        VirtualMachineBase parentAs = operand.GetParentAs<VirtualMachineBase>();
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RemoveVMDvdDrive, operand.Name, parentAs.Name)))
        {
            ((IRemovable)operand).Remove(operationWatcher);
            if (parentAs.IsClustered)
            {
                ClusterUtilities.UpdateClusterVMConfiguration(parentAs, base.InvokeCommand, operationWatcher);
            }
            if (Passthru.IsPresent)
            {
                operationWatcher.WriteObject(operand);
            }
        }
    }
}
