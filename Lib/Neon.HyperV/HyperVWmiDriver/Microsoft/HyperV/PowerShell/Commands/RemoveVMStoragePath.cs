using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Remove", "VMStoragePath")]
[OutputType(new Type[] { typeof(VMStorageResourcePool) })]
internal sealed class RemoveVMStoragePath : VirtualizationCmdlet<VMStorageResourcePool>, ISupportsPassthrough
{
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly, and this is by spec.")]
    public string[] Path { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 1)]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly, and this is by spec.")]
    public string[] ResourcePoolName { get; set; }

    [ValidateSet(new string[] { "VHD", "ISO", "VFD" }, IgnoreCase = true)]
    [Parameter(Mandatory = true, Position = 2)]
    public VMResourcePoolType ResourcePoolType { get; set; }

    [Parameter]
    public SwitchParameter Passthru { get; set; }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();
    }

    internal override IList<VMStorageResourcePool> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        VMResourcePoolType[] poolTypes = new VMResourcePoolType[1] { ResourcePoolType };
        List<VMStorageResourcePool> list = ParameterResolvers.GetServers(this, operationWatcher).SelectManyWithLogging((Server server) => VMResourcePool.GetVMResourcePools(server, ResourcePoolName, allowWildcards: true, poolTypes), operationWatcher).Cast<VMStorageResourcePool>()
            .ToList();
        if (list.Count == 0)
        {
            throw ExceptionHelper.CreateObjectNotFoundException(CmdletErrorMessages.GetVMResourcePool_NoneFound, null);
        }
        return list;
    }

    internal override void ProcessOneOperand(VMStorageResourcePool operand, IOperationWatcher operationWatcher)
    {
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_AddVMStoragePath, operand.Name, operand.ResourcePoolType)))
        {
            operand.RemoveStoragePaths(Path, operationWatcher);
            if (Passthru.IsPresent)
            {
                operationWatcher.WriteObject(operand);
            }
        }
    }
}
