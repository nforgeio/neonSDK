using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Rename", "VMResourcePool", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMResourcePool) })]
internal sealed class RenameVMResourcePool : VirtualizationCmdlet<VMResourcePool>, ISupportsPassthrough
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    public string Name { get; set; }

    [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
    public VMResourcePoolType ResourcePoolType { get; set; }

    [Parameter(Mandatory = true, Position = 2)]
    public string NewName { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    internal override IList<VMResourcePool> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        string[] poolNames = new string[1] { Name };
        VMResourcePoolType[] poolTypes = new VMResourcePoolType[1] { ResourcePoolType };
        List<VMResourcePool> list = ParameterResolvers.GetServers(this, operationWatcher).SelectManyWithLogging((Server server) => VMResourcePool.GetVMResourcePools(server, poolNames, allowWildcards: true, poolTypes), operationWatcher).ToList();
        if (list.Count == 0)
        {
            throw ExceptionHelper.CreateObjectNotFoundException(CmdletErrorMessages.GetVMResourcePool_NoneFound, null);
        }
        return list;
    }

    internal override void ProcessOneOperand(VMResourcePool operand, IOperationWatcher operationWatcher)
    {
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RenameVMResourcePool, operand.Name, operand.ResourcePoolType, NewName)))
        {
            operand.Name = NewName;
            ((IUpdatable)operand).Put(operationWatcher);
            if (Passthru.IsPresent)
            {
                operationWatcher.WriteObject(operand);
            }
        }
    }
}
