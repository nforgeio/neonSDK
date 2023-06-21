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

[Cmdlet("Get", "VMHostNumaNodeStatus", DefaultParameterSetName = "ComputerName")]
[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Numa", Justification = "This is by spec.")]
[OutputType(new Type[] { typeof(GetVMHostNumaNodeStatus) })]
internal sealed class GetVMHostNumaNodeStatus : VirtualizationCmdlet<VMNumaNodeStatus>
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = true, ParameterSetName = "CimSession")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(Position = 0, ParameterSetName = "ComputerName")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(Position = 1, ParameterSetName = "ComputerName")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [ValidateNotNull]
    [Parameter]
    public int? Id { get; set; }

    internal override IList<VMNumaNodeStatus> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        List<VMNumaNodeStatus> list = ParameterResolvers.GetServers(this, operationWatcher).SelectWithLogging(GetCapableHostOrThrow, operationWatcher).SelectMany(VMNumaNodeStatus.GetVMNumaNodeStatus)
            .ToList();
        if (Id.HasValue)
        {
            list = list.Where((VMNumaNodeStatus node) => Id.Value == node.NodeId).ToList();
        }
        return list;
    }

    internal override void ProcessOneOperand(VMNumaNodeStatus operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }

    private static VMHost GetCapableHostOrThrow(Server server)
    {
        VMHost vMHost = VirtualizationObjectLocator.GetVMHost(server);
        if (vMHost.NumaSpanningEnabled)
        {
            throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidOperation_NumaSpanningIsEnabled, vMHost.Server));
        }
        return vMHost;
    }
}
