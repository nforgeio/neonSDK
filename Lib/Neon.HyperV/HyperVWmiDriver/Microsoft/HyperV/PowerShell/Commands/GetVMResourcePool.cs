using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMResourcePool")]
[OutputType(new Type[] { typeof(VMResourcePool) })]
internal sealed class GetVMResourcePool : VirtualizationCmdlet<VMResourcePool>
{
    [ValidateNotNullOrEmpty]
    [Parameter(Position = 0, ValueFromPipeline = true)]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly, and this is by spec.")]
    public string[] Name { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Position = 1)]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly, and this is by spec.")]
    public VMResourcePoolType[] ResourcePoolType { get; set; }

    internal override IList<VMResourcePool> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        List<VMResourcePool> list = ParameterResolvers.GetServers(this, operationWatcher).SelectManyWithLogging((Server server) => VMResourcePool.GetVMResourcePools(server, Name, allowWildcards: true, ResourcePoolType), operationWatcher).ToList();
        if (list.Count == 0)
        {
            operationWatcher.WriteWarning(CmdletErrorMessages.GetVMResourcePool_NoneFound);
        }
        return list;
    }

    internal override void ProcessOneOperand(VMResourcePool operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}
