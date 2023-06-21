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

[Cmdlet("Add", "VMHostAssignableDevice", DefaultParameterSetName = "Path")]
internal sealed class AddVmHostAssignableDevice : VirtualizationCmdlet<Tuple<VMPciExpressResourcePool, IEnumerable<VMHostAssignableDevice>>>, ISupportsForce
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "Path")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "Path")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "Path")]
    [ValidateNotNullOrEmpty]
    public override PSCredential[] Credential { get; set; }

    [Parameter]
    public SwitchParameter Force { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "Path")]
    public string InstancePath { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "Path")]
    public string LocationPath { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly, and this is by spec.")]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Object")]
    public VMHostAssignableDevice[] VMHostAssignableDevice { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly, and this is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true)]
    public string[] ResourcePoolName { get; set; }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();
        if (base.ParameterSetName == "Path" && string.IsNullOrEmpty(InstancePath) && string.IsNullOrEmpty(LocationPath))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_NoPciExpressDeviceSpecified);
        }
    }

    internal override IList<Tuple<VMPciExpressResourcePool, IEnumerable<VMHostAssignableDevice>>> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        List<Server> inputs = ParameterResolvers.GetServers(this, operationWatcher).ToList();
        VMResourcePoolType[] pciExpressResourceType = new VMResourcePoolType[1] { VMResourcePoolType.PciExpress };
        IEnumerable<VMPciExpressResourcePool> source = inputs.SelectManyWithLogging((Server server) => VMResourcePool.GetVMResourcePools(server, ResourcePoolName, allowWildcards: true, pciExpressResourceType), operationWatcher).Cast<VMPciExpressResourcePool>();
        IList<VMHostAssignableDevice> devices;
        if (CurrentParameterSetIs("Path"))
        {
            devices = inputs.SelectManyWithLogging((Server server) => global::Microsoft.HyperV.PowerShell.VMHostAssignableDevice.FindHostAssignableDevices(server, InstancePath, LocationPath), operationWatcher).ToList();
        }
        else
        {
            devices = VMHostAssignableDevice;
        }
        return source.Select((VMPciExpressResourcePool pool) => Tuple.Create(pool, devices.AsEnumerable())).ToList();
    }

    internal override void ProcessOneOperand(Tuple<VMPciExpressResourcePool, IEnumerable<VMHostAssignableDevice>> operand, IOperationWatcher operationWatcher)
    {
        VMPciExpressResourcePool item = operand.Item1;
        IEnumerable<VMHostAssignableDevice> item2 = operand.Item2;
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_AddVMHostAssignableDevice, item.Name)) && (!item2.Select((VMHostAssignableDevice device) => device.Server.IsClientSku).Any() || operationWatcher.ShouldContinue(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldContinue_AddVMHostAssignableDevice_ClientSKU))))
        {
            item.AddPciExpressDevices(item2, operationWatcher);
        }
    }
}
