#define TRACE
namespace Microsoft.Virtualization.Client.Management;

internal class AssignableDeviceServiceView : View, IAssignableDeviceService, IVirtualizationManagementObject
{
    internal static class AssignableDeviceServiceMembers
    {
        public const string DismountAssignableDevice = "DismountAssignableDevice";

        public const string MountAssignableDevice = "MountAssignableDevice";
    }

    public void DismountAssignableDevice(VMDismountSetting dismountSettingData, out string newDeviceInstanceId)
    {
        VMTrace.TraceUserActionInitiated("dismounting an assignable device.");
        object[] array = new object[3]
        {
            dismountSettingData.GetSettingDataEmbeddedInstance(base.Server),
            null,
            null
        };
        uint result = InvokeMethod("DismountAssignableDevice", array);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[2]);
        iVMTask.ClientSideFailedMessage = ErrorMessages.DismountAssignableDeviceFailed;
        iVMTask.WaitForCompletion();
        EndMethod(iVMTask, VirtualizationOperation.DismountAssignableDevice);
        VMTrace.TraceUserActionCompleted("Dismounted assignable device successfully.");
        newDeviceInstanceId = array[1] as string;
        ObjectLocator.GetHostComputerSystem(base.Server).GetPrimordialResourcePool(VMDeviceSettingType.PciExpress).InvalidateAssociationCache();
    }

    public void MountAssignableDevice(string instanceId, string locationPath, out string newDeviceInstanceId)
    {
        VMTrace.TraceUserActionInitiated("mounting an assignable device.");
        object[] array = new object[4] { instanceId, locationPath, null, null };
        uint result = InvokeMethod("MountAssignableDevice", array);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[3]);
        iVMTask.ClientSideFailedMessage = ErrorMessages.MountAssignableDeviceFailed;
        iVMTask.WaitForCompletion();
        EndMethod(iVMTask, VirtualizationOperation.MountAssignableDevice);
        VMTrace.TraceUserActionCompleted("Mounted assignable device successfully.");
        newDeviceInstanceId = array[2] as string;
        ObjectLocator.GetHostComputerSystem(base.Server).GetPrimordialResourcePool(VMDeviceSettingType.PciExpress).InvalidateAssociationCache();
    }
}
