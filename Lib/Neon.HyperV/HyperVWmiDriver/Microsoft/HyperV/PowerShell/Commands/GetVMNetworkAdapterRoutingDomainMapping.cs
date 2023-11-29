using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMNetworkAdapterRoutingDomainMapping", DefaultParameterSetName = "VMName", SupportsShouldProcess = false)]
[OutputType(new Type[] { typeof(VMNetworkAdapterRoutingDomainSetting) })]
internal sealed class GetVMNetworkAdapterRoutingDomainMapping : VirtualizationCmdlet<VMNetworkAdapterRoutingDomainSetting>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, IVMInternalNetworkAdapterCmdlet, IVMNetworkAdapterBaseCmdlet, IVMNetworkAdapterBaseByObjectCmdlet, IVMSnapshotCmdlet
{
    [Parameter(Mandatory = false)]
    public Guid RoutingDomainID { get; set; }

    [Parameter(Mandatory = false)]
    public string RoutingDomainName { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMSnapshot")]
    [Alias(new string[] { "VMCheckpoint" })]
    public VMSnapshot VMSnapshot { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0)]
    public string[] VMName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and array is easier for users to use.")]
    [Parameter(ParameterSetName = "ResourceObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VMNetworkAdapterBase[] VMNetworkAdapter { get; set; }

    [Parameter(ParameterSetName = "ManagementOS", Mandatory = true)]
    public SwitchParameter ManagementOS { get; set; }

    [Parameter(ParameterSetName = "VMObject")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "ManagementOS")]
    public string VMNetworkAdapterName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "ManagementOS")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "ManagementOS")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "ManagementOS")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VirtualMachine[] VM { get; set; }

    internal override IList<VMNetworkAdapterRoutingDomainSetting> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        IEnumerable<VMNetworkAdapterRoutingDomainSetting> source = ParameterResolvers.ResolveNetworkAdapters(this, VMNetworkAdapterName, operationWatcher).SelectManyWithLogging((VMNetworkAdapterBase adapter) => adapter.RoutingDomainList, operationWatcher);
        if (RoutingDomainID != Guid.Empty)
        {
            source = source.Where((VMNetworkAdapterRoutingDomainSetting routingDomain) => routingDomain.RoutingDomainID == RoutingDomainID);
        }
        if (!string.IsNullOrEmpty(RoutingDomainName))
        {
            source = source.Where((VMNetworkAdapterRoutingDomainSetting routingDomain) => string.Equals(routingDomain.RoutingDomainName, RoutingDomainName, StringComparison.OrdinalIgnoreCase));
        }
        return source.ToList();
    }

    internal override void ProcessOneOperand(VMNetworkAdapterRoutingDomainSetting operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}
