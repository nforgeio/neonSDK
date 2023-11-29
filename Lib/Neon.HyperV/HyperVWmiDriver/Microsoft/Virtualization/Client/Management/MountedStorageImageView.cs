#define TRACE
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class MountedStorageImageView : View, IMountedStorageImage, IVirtualizationManagementObject
{
    internal static class WmiMemberNames
    {
        public const string LunId = "Lun";

        public const string ImagePath = "Name";

        public const string PathId = "PathId";

        public const string PortNumber = "PortNumber";

        public const string TargetId = "TargetId";

        public const string DetachVirtualHardDisk = "DetachVirtualHardDisk";
    }

    public string ImagePath => GetProperty<string>("Name");

    public byte LunId => GetProperty<byte>("Lun");

    public byte PathId => GetProperty<byte>("PathId");

    public byte PortNumber => GetProperty<byte>("PortNumber");

    public byte TargetId => GetProperty<byte>("TargetId");

    public IWin32DiskDrive GetDiskDrive()
    {
        string format = "SELECT * FROM {0} WHERE {1}=\"{2}\" AND {3}=\"{4}\" AND {5}=\"{6}\" AND {7}=\"{8}\"";
        format = string.Format(CultureInfo.InvariantCulture, format, "Win32_DiskDrive", "SCSIBus", PathId, "SCSILogicalUnit", LunId, "SCSIPort", PortNumber, "SCSITargetId", TargetId);
        QueryAssociation association = QueryAssociation.CreateFromQuery(Server.CimV2Namespace, format);
        return GetRelatedObject<IWin32DiskDrive>(association);
    }

    public IVMTask BeginDetachVirtualHardDisk()
    {
        string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.DetachVirtualHardDiskFailed, ImagePath);
        object[] array = new object[1];
        VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting detaching virtual hard disk '{0}'", ImagePath));
        uint result = InvokeMethod("DetachVirtualHardDisk", array);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[0]);
        iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
        return iVMTask;
    }

    public void EndDetachVirtualHardDisk(IVMTask task)
    {
        EndMethod(task, VirtualizationOperation.DetachVirtualHardDisk);
        VMTrace.TraceUserActionCompleted("Detaching virtual hard disk completed successfully.");
    }
}
