using System;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class VMMigrationTaskView : VMTaskView, IVMMigrationTask, IVMTask, IVirtualizationManagementObject, IDisposable
{
    internal static class VMMigrationWmiMemberNames
    {
        public const string DestinationHost = "DestinationHost";

        public const string VmComputerSystemInstanceId = "VirtualSystemName";

        public const string MigrationType = "MigrationType";
    }

    public string DestinationHost => GetProperty<string>("DestinationHost");

    public string VmComputerSystemInstanceId => GetProperty<string>("VirtualSystemName");
}
