using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMMigrationNetwork")]
[OutputType(new Type[] { typeof(VMMigrationNetwork) })]
internal sealed class GetVMMigrationNetwork : VirtualizationCmdlet<VMMigrationNetwork>
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ValueFromPipelineByPropertyName = true)]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ValueFromPipeline = true)]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Position = 0)]
    public string[] Subnet { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter]
    public uint[] Priority { get; set; }

    internal override IList<VMMigrationNetwork> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return VirtualizationObjectLocator.GetVMHosts(ParameterResolvers.GetServers(this, operationWatcher), operationWatcher).SelectMany((VMHost service) => service.GetUserManagedVMMigrationNetworks(Subnet, Priority)).ToList();
    }

    internal override void ProcessOneOperand(VMMigrationNetwork operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}
