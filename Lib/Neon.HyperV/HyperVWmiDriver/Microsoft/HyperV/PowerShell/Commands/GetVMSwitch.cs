using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMSwitch", DefaultParameterSetName = "Name")]
[OutputType(new Type[] { typeof(VMSwitch) })]
internal sealed class GetVMSwitch : VirtualizationCmdlet<VMSwitch>
{
    internal static class ParameterSetNames
    {
        public const string Id = "Id";

        public const string Name = "Name";
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and arrays are easier to use.")]
    [ValidateNotNull]
    [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "Id")]
    [Alias(new string[] { "SwitchId" })]
    public Guid[] Id { get; set; }

    [Parameter(Position = 0, ParameterSetName = "Name")]
    [Alias(new string[] { "SwitchName" })]
    [ValidateNotNullOrEmpty]
    public string Name { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly, and this is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Position = 1)]
    public string[] ResourcePoolName { get; set; }

    [Parameter]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and arrays are easier to use.")]
    public VMSwitchType[] SwitchType { get; set; }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();
        IsParameterSpecified("ResourcePoolName");
    }

    internal override IList<VMSwitch> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        IEnumerable<Server> servers = ParameterResolvers.GetServers(this, operationWatcher);
        string[] switchNames = (string.IsNullOrEmpty(Name) ? null : new string[1] { Name });
        IEnumerable<VMSwitch> source;
        if (IsParameterSpecified("ResourcePoolName"))
        {
            VMResourcePoolType[] ethernetResourceType = new VMResourcePoolType[1] { VMResourcePoolType.Ethernet };
            source = servers.SelectManyWithLogging((Server server) => VMResourcePool.GetVMResourcePools(server, ResourcePoolName, allowWildcards: true, ethernetResourceType), operationWatcher).Cast<VMEthernetResourcePool>().SelectManyWithLogging((VMEthernetResourcePool pool) => pool.GetSwitchesByNames(switchNames, allowWildcards: true, ErrorDisplayMode.WriteWarning, operationWatcher), operationWatcher);
        }
        else
        {
            source = VMSwitch.GetSwitchesByNamesAndServers(servers, switchNames, allowWildcards: true, ErrorDisplayMode.WriteWarning, operationWatcher);
        }
        if (CurrentParameterSetIs("Id"))
        {
            source = source.Where((VMSwitch s) => Id.Contains(s.Id));
        }
        if (IsParameterSpecified("SwitchType"))
        {
            source = source.Where((VMSwitch s) => SwitchType.Contains(s.SwitchType));
        }
        return source.ToList();
    }

    internal override void ValidateOperandList(IList<VMSwitch> operands, IOperationWatcher operationWatcher)
    {
        if (CurrentParameterSetIs("Id") && !operands.Any())
        {
            throw ExceptionHelper.CreateObjectNotFoundException(CmdletErrorMessages.VMSwitch_NoSwitchFound, null);
        }
        base.ValidateOperandList(operands, operationWatcher);
    }

    internal override void ProcessOneOperand(VMSwitch operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}
