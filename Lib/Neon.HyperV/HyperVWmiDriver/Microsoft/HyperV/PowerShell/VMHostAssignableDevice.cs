using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMHostAssignableDevice : VirtualizationObject
{
    private readonly IVMAssignableDevice m_Device;

    public string InstanceID => m_Device.DeviceInstancePath;

    public string LocationPath => m_Device.LocationPath;

    internal IVMAssignableDevice AssignableDevice => m_Device;

    internal VMHostAssignableDevice(IVMAssignableDevice device)
        : base(device)
    {
        m_Device = device;
    }

    internal static void Mount(VMHostAssignableDevice device)
    {
        ObjectLocator.GetAssignableDeviceService(device.Server).MountAssignableDevice(device.InstanceID, device.LocationPath, out var _);
    }

    internal static VMHostAssignableDevice Dismount(Server server, string instancePath, string locationPath, bool requireAcsSupport, bool requireDeviceMitigations)
    {
        IHostComputerSystem hostComputerSystem = ObjectLocator.GetHostComputerSystem(server);
        ObjectLocator.GetAssignableDeviceService(server).DismountAssignableDevice(new VMDismountSetting
        {
            DeviceInstancePath = instancePath,
            DeviceLocationPath = locationPath,
            RequireAcsSupport = requireAcsSupport,
            RequireDeviceMitigations = requireDeviceMitigations
        }, out var newDeviceInstanceId);
        return new VMHostAssignableDevice(PciExpressUtilities.FilterAssignableDevices(hostComputerSystem.GetHostAssignableDevices(TimeSpan.Zero), newDeviceInstanceId, null).FirstOrDefault() ?? throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.CannotFindDismountedDevice, null));
    }

    internal static IEnumerable<VMHostAssignableDevice> FindHostAssignableDevices(Server server, string instancePath, string locationPath)
    {
        return (from device in PciExpressUtilities.FilterAssignableDevices(ObjectLocator.GetHostComputerSystem(server).GetHostAssignableDevices(Constants.UpdateThreshold), instancePath, locationPath)
            select new VMHostAssignableDevice(device)).ToList();
    }
}
