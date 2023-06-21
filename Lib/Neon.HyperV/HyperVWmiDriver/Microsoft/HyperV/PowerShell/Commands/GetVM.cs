using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VM", DefaultParameterSetName = "Name")]
[OutputType(new Type[] { typeof(VirtualMachine) })]
internal sealed class GetVM : VirtualizationCmdlet<VirtualMachine>, IVmByNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "Name", ValueFromPipeline = true, Position = 0)]
    [Alias(new string[] { "VMName" })]
    public string[] Name { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "Name")]
    [Parameter(ParameterSetName = "Id")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "Name")]
    [Parameter(ParameterSetName = "Id")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "Name")]
    [Parameter(ParameterSetName = "Id")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [ValidateNotNull]
    [Parameter(ParameterSetName = "Id", ValueFromPipelineByPropertyName = true, ValueFromPipeline = true, Position = 0)]
    public Guid? Id { get; set; }

    [Parameter(ParameterSetName = "ClusterObject", Mandatory = true, Position = 0, ValueFromPipeline = true)]
    [ValidateNotNullOrEmpty]
    [PSTypeName("Microsoft.FailoverClusters.PowerShell.ClusterObject")]
    public PSObject ClusterObject { get; set; }

    internal override IList<VirtualMachine> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        if (CurrentParameterSetIs("Name"))
        {
            return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher, ErrorDisplayMode.WriteWarning);
        }
        if (CurrentParameterSetIs("Id"))
        {
            Guid vmId = Id.Value;
            return ParameterResolvers.GetServers(this, operationWatcher).SelectWithLogging((Server server) => VirtualizationObjectLocator.GetVirtualMachineById(server, vmId), operationWatcher).ToList();
        }
        return GetVirtualMachinesFromClusterObject(ClusterObject, operationWatcher).ToList();
    }

    internal override void ProcessOneOperand(VirtualMachine operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }

    private static IEnumerable<VirtualMachine> GetVirtualMachinesFromClusterObject(PSObject clusterObject, IOperationWatcher operationWatcher)
    {
        if (clusterObject.BaseObject is object[] source)
        {
            return source.Cast<PSObject>().SelectManyWithLogging((PSObject innerElement) => ClusterUtilities.GetVirtualMachinesFromClusterObject(innerElement, operationWatcher), operationWatcher);
        }
        try
        {
            return ClusterUtilities.GetVirtualMachinesFromClusterObject(clusterObject, operationWatcher);
        }
        catch (Exception e)
        {
            ExceptionHelper.DisplayErrorOnException(e, operationWatcher);
            return Enumerable.Empty<VirtualMachine>();
        }
    }
}
