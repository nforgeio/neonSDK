using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMGroup", DefaultParameterSetName = "Name")]
[OutputType(new Type[] { typeof(VMGroup) })]
internal sealed class GetVMGroup : VirtualizationCmdlet<VMGroup>
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

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "Name", Position = 0)]
    public string[] Name { get; set; }

    [Parameter(ParameterSetName = "Id", Position = 0)]
    [ValidateNotNullOrEmpty]
    public Guid Id { get; set; }

    internal override IList<VMGroup> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        IEnumerable<Server> servers = ParameterResolvers.GetServers(this, operationWatcher);
        if (CurrentParameterSetIs("Name"))
        {
            return VMGroup.GetVMGroupsByName(servers, Name, operationWatcher);
        }
        return VMGroup.GetVMGroupsById(servers, Id, operationWatcher);
    }

    internal override void ProcessOneOperand(VMGroup operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}
