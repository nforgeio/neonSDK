using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMConnectAccess", DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMConnectAce) })]
internal sealed class GetVMConnectAccess : VirtualizationCmdlet<VirtualMachine>, IVmByVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMIdCmdlet
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMId", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Position = 0, Mandatory = true)]
    public Guid[] VMId { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0)]
    public string[] VMName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
    [ValidateNotNullOrEmpty]
    [Parameter(ValueFromPipelineByPropertyName = true)]
    [Alias(new string[] { "UserId", "Sid" })]
    public string[] UserName { get; set; }

    internal override IList<VirtualMachine> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher);
    }

    internal override void ProcessOneOperand(VirtualMachine operand, IOperationWatcher operationWatcher)
    {
        foreach (VMConnectAce item in operand.GetVMConnectAccess())
        {
            if (MatchesNameFilter(item))
            {
                operationWatcher.WriteObject(item);
            }
        }
    }

    private bool MatchesNameFilter(VMConnectAce ace)
    {
        if (UserName == null)
        {
            return true;
        }
        string[] userName = UserName;
        foreach (string a in userName)
        {
            if (string.Equals(a, ace.UserName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (ace.UserId != null && string.Equals(a, ace.UserId.Value, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}
