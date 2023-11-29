using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Remove", "VMGroup", DefaultParameterSetName = "Name")]
internal sealed class RemoveVMGroup : VirtualizationCmdlet<VMGroup>, ISupportsForce
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "Name")]
    [Parameter(ParameterSetName = "Id")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "Name")]
    [Parameter(ParameterSetName = "Id")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "Name")]
    [Parameter(ParameterSetName = "Id")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "Name", Mandatory = true, Position = 0)]
    public string Name { get; set; }

    [Parameter(ParameterSetName = "Id", Mandatory = true, Position = 0)]
    [ValidateNotNullOrEmpty]
    public Guid Id { get; set; }

    [Parameter(ParameterSetName = "InputObject", Mandatory = true, ValueFromPipeline = true, Position = 0)]
    [ValidateNotNullOrEmpty]
    public VMGroup VMGroup { get; set; }

    [Parameter]
    public SwitchParameter Force { get; set; }

    internal override IList<VMGroup> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        IList<VMGroup> list = new List<VMGroup>();
        if (CurrentParameterSetIs("InputObject"))
        {
            list.Add(VMGroup);
        }
        else
        {
            IEnumerable<Server> servers = ParameterResolvers.GetServers(this, operationWatcher);
            if (CurrentParameterSetIs("Name"))
            {
                IList<string> list2 = new List<string>();
                list2.Add(Name);
                list = VMGroup.GetVMGroupsByName(servers, list2, operationWatcher);
            }
            else
            {
                list = VMGroup.GetVMGroupsById(servers, Id, operationWatcher);
            }
        }
        return list;
    }

    internal override void ProcessOneOperand(VMGroup operand, IOperationWatcher operationWatcher)
    {
        string name = operand.Name;
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RemoveVMGroup, name)) && operationWatcher.ShouldContinue(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldContinue_RemoveVMGroup, name)))
        {
            ((IRemovable)operand).Remove(operationWatcher);
        }
    }
}
