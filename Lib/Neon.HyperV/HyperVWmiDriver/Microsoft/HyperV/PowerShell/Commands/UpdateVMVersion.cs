using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Text;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Update", "VMVersion", DefaultParameterSetName = "Name", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
[OutputType(new Type[] { typeof(VirtualMachine) })]
internal sealed class UpdateVMVersion : VirtualizationCmdlet<VirtualMachine>, IVMObjectOrNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByNameCmdlet, ISupportsAsJob, ISupportsForce, ISupportsPassthrough
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "Name")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "Name")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "Name")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Alias(new string[] { "VMName" })]
    [Parameter(ParameterSetName = "Name", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public string[] Name { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VirtualMachine[] VM { get; set; }

    [Parameter]
    public SwitchParameter Force { get; set; }

    [Parameter]
    public SwitchParameter AsJob { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    private List<VMSnapshot> GetSnapshotsWithSavedState(VirtualMachine virtualMachine)
    {
        return (from snapshot in virtualMachine.GetVMSnapshots()
            where snapshot.State == VMState.Saved
            select snapshot).ToList();
    }

    private void PerformUpgrade(VirtualMachine virtualMachine, IEnumerable<VMSnapshot> savedSnapshotList, IOperationWatcher operationWatcher)
    {
        VMState currentState = virtualMachine.GetCurrentState();
        if (currentState != VMState.Off && currentState != VMState.Saved && currentState != VMState.FastSaved)
        {
            throw ExceptionHelper.CreateInvalidStateException(ErrorMessages.OperationFailed_InvalidState, null, virtualMachine);
        }
        if (savedSnapshotList == null)
        {
            savedSnapshotList = GetSnapshotsWithSavedState(virtualMachine);
        }
        foreach (VMSnapshot savedSnapshot in savedSnapshotList)
        {
            savedSnapshot.DeleteSavedState(operationWatcher);
        }
        if (currentState == VMState.Saved || currentState == VMState.FastSaved)
        {
            virtualMachine.ChangeState(VirtualMachineAction.Stop, operationWatcher);
        }
        if (!virtualMachine.IsUpgradable())
        {
            operationWatcher.WriteWarning(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.UpdateVMConfigurationVersion_VMVersionMaximum_Warning, virtualMachine.Name));
        }
        else
        {
            virtualMachine.Upgrade(operationWatcher);
        }
        if (Passthru.IsPresent)
        {
            operationWatcher.WriteObject(virtualMachine);
        }
    }

    internal override IList<VirtualMachine> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher);
    }

    internal override void ProcessOneOperand(VirtualMachine operand, IOperationWatcher operationWatcher)
    {
        string arg = string.Empty;
        List<VMSnapshot> list = null;
        if (!Force)
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool flag = operand.State == VMState.Saved || operand.State == VMState.FastSaved;
            list = GetSnapshotsWithSavedState(operand);
            foreach (VMSnapshot item in list)
            {
                stringBuilder.AppendFormat("{0}    -  {1}", Environment.NewLine, item.Name);
            }
            bool flag2 = list.Count > 0;
            if (flag || flag2)
            {
                string arg2 = (flag ? CmdletResources.ShouldProcess_UpdateVMSavedVM : string.Empty);
                string arg3 = (flag2 ? string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_UpdateVMOnlineCheckpoints, stringBuilder.ToString()) : string.Empty);
                arg = string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_UpdateVMWithSavedState, arg2, arg3);
            }
        }
        if ((bool)Force || operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_UpdateVMConfigurationVersion, operand.Name, arg)))
        {
            PerformUpgrade(operand, list, operationWatcher);
        }
    }
}
