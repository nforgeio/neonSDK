using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMHostAssignableDevice")]
[OutputType(new Type[] { typeof(VMHostAssignableDevice) })]
internal sealed class GetVMHostAssignableDevice : VirtualizationCmdlet<VMHostAssignableDevice>
{
    [ValidateNotNullOrEmpty]
    public string InstancePath { get; set; }

    [ValidateNotNullOrEmpty]
    public string LocationPath { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly, and this is by spec.")]
    [ValidateNotNullOrEmpty]
    public string[] ResourcePoolName { get; set; }

    internal override IList<VMHostAssignableDevice> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        IEnumerable<Server> servers = ParameterResolvers.GetServers(this, operationWatcher);
        if (IsParameterSpecified("ResourcePoolName"))
        {
            VMResourcePoolType[] pciExpressResourceType = new VMResourcePoolType[1] { VMResourcePoolType.PciExpress };
            return servers.SelectManyWithLogging((Server server) => VMResourcePool.GetVMResourcePools(server, ResourcePoolName, allowWildcards: true, pciExpressResourceType), operationWatcher).Cast<VMPciExpressResourcePool>().SelectManyWithLogging((VMPciExpressResourcePool pool) => pool.PciExpressDevices, operationWatcher)
                .ToList();
        }
        return servers.SelectManyWithLogging((Server server) => VMHostAssignableDevice.FindHostAssignableDevices(server, InstancePath, LocationPath), operationWatcher).ToList();
    }

    internal override void ProcessOneOperand(VMHostAssignableDevice operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}
