using System.Collections.Generic;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMPciExpressResourcePool : VMResourcePool
{
    private static readonly IEqualityComparer<object> gm_PciExpressDeviceComparer = new VirtualizationManagementObjectEqualityComparer<IVMAssignableDevice>();

    public IEnumerable<VMHostAssignableDevice> PciExpressDevices => from device in ((IPciExpressResourcePool)m_ResourcePool.GetData(UpdatePolicy.EnsureAssociatorsUpdated)).GetPciExpressDevices()
        select new VMHostAssignableDevice(device);

    protected override IEqualityComparer<object> HostResourceComparer => gm_PciExpressDeviceComparer;

    protected override string MissingResourcesError => ErrorMessages.VMPciExpressResourcePool_MissingResource;

    internal VMPciExpressResourcePool(IResourcePool resourcePool)
        : base(resourcePool)
    {
    }

    internal void AddPciExpressDevices(IEnumerable<VMHostAssignableDevice> pcieDevicesToAdd, IOperationWatcher operationWatcher)
    {
        AddHostResources(pcieDevicesToAdd.Select((VMHostAssignableDevice device) => device.AssignableDevice), TaskDescriptions.AddPciExpressDevice_ToResourcePool, operationWatcher);
    }

    internal void RemovePciExpressDevices(IEnumerable<VMHostAssignableDevice> pcieDevicesToRemove, IOperationWatcher operationWatcher)
    {
        RemoveHostResources(pcieDevicesToRemove.Select((VMHostAssignableDevice device) => device.AssignableDevice).ToList().AsReadOnly(), TaskDescriptions.RemovePciExpressDevice_FromResourcePool, operationWatcher);
    }

    protected override IEnumerable<object> GetHostResources(IResourcePoolAllocationSetting poolAllocationSetting)
    {
        IPciExpressPoolAllocationSetting obj = poolAllocationSetting as IPciExpressPoolAllocationSetting;
        obj.UpdatePropertyCache(Constants.UpdateThreshold);
        return obj.GetPciExpressDevices();
    }

    protected override bool HasHostResource(IResourcePoolAllocationSetting poolAllocationSetting, object hostResource)
    {
        IVMAssignableDevice pcieDeviceToCheck = (IVMAssignableDevice)hostResource;
        IPciExpressPoolAllocationSetting obj = (IPciExpressPoolAllocationSetting)poolAllocationSetting;
        obj.UpdatePropertyCache(Constants.UpdateThreshold);
        return obj.GetPciExpressDevices().Any((IVMAssignableDevice pcieDevice) => gm_PciExpressDeviceComparer.Equals(pcieDevice, pcieDeviceToCheck));
    }

    protected override void SetHostResourceInAllocationFromParentPool(IEnumerable<object> hostResources, IResourcePool parentPool, IResourcePoolAllocationSetting parentPoolAllocationSetting)
    {
        IEnumerable<IVMAssignableDevice> enumerable2;
        if (hostResources == null)
        {
            IEnumerable<IVMAssignableDevice> enumerable = new List<IVMAssignableDevice>();
            enumerable2 = enumerable;
        }
        else
        {
            enumerable2 = hostResources.Cast<IVMAssignableDevice>();
        }
        IEnumerable<IVMAssignableDevice> source = enumerable2;
        IPciExpressResourcePool pciExpressResourcePool = (IPciExpressResourcePool)parentPool;
        IPciExpressPoolAllocationSetting pciExpressPoolAllocationSetting = (IPciExpressPoolAllocationSetting)parentPoolAllocationSetting;
        if (pciExpressResourcePool.Primordial)
        {
            pciExpressPoolAllocationSetting.SetPciExpressDevices(source.ToList());
        }
        else
        {
            pciExpressPoolAllocationSetting.SetPciExpressDevices(source.Where(pciExpressResourcePool.HasPciExpressDevice).ToList());
        }
    }
}
