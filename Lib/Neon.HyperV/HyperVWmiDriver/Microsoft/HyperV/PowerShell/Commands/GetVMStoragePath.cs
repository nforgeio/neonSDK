using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMStoragePath")]
[OutputType(new Type[] { typeof(VMStorageResourcePool) })]
internal sealed class GetVMStoragePath : VirtualizationCmdlet<VMStorageResourcePool>
{
    private static readonly IReadOnlyList<string> gm_MatchAnyPath = new List<string> { "*" }.AsReadOnly();

    [ValidateNotNullOrEmpty]
    [Parameter(Position = 0, ValueFromPipeline = true)]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly, and this is by spec.")]
    public string[] Path { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 1)]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly, and this is by spec.")]
    public string[] ResourcePoolName { get; set; }

    [Parameter(Mandatory = true, Position = 2)]
    public VMResourcePoolType ResourcePoolType { get; set; }

    private WildcardPatternMatcher PathMatcher { get; set; }

    protected override void NormalizeParameters()
    {
        base.NormalizeParameters();
        IReadOnlyList<string> path = Path;
        PathMatcher = new WildcardPatternMatcher(path ?? gm_MatchAnyPath);
    }

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
            operationWatcher.WriteWarning(CmdletErrorMessages.GetVMResourcePool_NoneFound);
        }
        return list;
    }

    internal override void ProcessOneOperand(VMStorageResourcePool operand, IOperationWatcher operationWatcher)
    {
        foreach (string item in from path in operand.GetStoragePaths()
            where PathMatcher.MatchesAny(path)
            select path)
        {
            operationWatcher.WriteObject(item);
        }
    }
}
