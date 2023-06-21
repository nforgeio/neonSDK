using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class VMDriveControllerSettingView : VMDeviceSettingView, IVMDriveControllerSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    private static class WmiMemberNames
    {
        public const string Address = "Address";
    }

    public string Address => GetProperty<string>("Address");

    public IEnumerable<IVMDriveSetting> GetDriveSettings()
    {
        IEnumerable<IVMDriveSetting> relatedObjects = GetRelatedObjects<IVMDriveSetting>(base.Associations.DriveControllerSettingToDriveSetting);
        IEnumerable<IVMDriveSetting> relatedObjects2 = GetRelatedObjects<IVMDriveSetting>(base.Associations.DriveControllerSettingToKeyStorageDriveSetting);
        return relatedObjects.Concat(relatedObjects2);
    }
}
