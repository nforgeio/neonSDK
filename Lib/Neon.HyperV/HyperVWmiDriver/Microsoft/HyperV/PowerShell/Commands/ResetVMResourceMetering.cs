using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Reset", "VMResourceMetering", DefaultParameterSetName = "VMName")]
internal sealed class ResetVMResourceMetering : VirtualizationCmdlet<object>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet
{
    internal static class AdditionalParameterSetNames
    {
        public const string ResourcePool = "ResourcePool";
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName", ValueFromPipelineByPropertyName = true)]
    [Parameter(ParameterSetName = "ResourcePool", ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "ResourcePool")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "ResourcePool")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VirtualMachine[] VM { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMName", Position = 0, Mandatory = true)]
    public string[] VMName { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ResourcePool")]
    [Alias(new string[] { "Name" })]
    public string ResourcePoolName { get; set; }

    [ValidateSet(new string[] { "Ethernet", "Memory", "Processor", "VHD" }, IgnoreCase = true)]
    [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, ParameterSetName = "ResourcePool")]
    public VMResourcePoolType ResourcePoolType { get; set; }

    internal override IList<object> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        IEnumerable<object> source;
        if (CurrentParameterSetIs("ResourcePool"))
        {
            string[] resourcePoolNames = new string[1] { ResourcePoolName };
            VMResourcePoolType[] resourcePoolTypes = ((!IsParameterSpecified("ResourcePoolType")) ? null : new VMResourcePoolType[1] { ResourcePoolType });
            source = ParameterResolvers.GetServers(this, operationWatcher).SelectManyWithLogging((Server server) => VMResourcePool.GetVMResourcePools(server, resourcePoolNames, allowWildcards: true, resourcePoolTypes), operationWatcher).OfType<IMeasurableResourcePool>();
        }
        else
        {
            source = ParameterResolvers.ResolveVirtualMachines(this, operationWatcher);
        }
        return source.ToList();
    }

    internal override void ProcessOneOperand(object operand, IOperationWatcher operationWatcher)
    {
        MetricUtilities.ChangeMetricServiceState(MetricUtilities.MetricServiceState.Reset, (IMeasurable)operand);
    }
}
