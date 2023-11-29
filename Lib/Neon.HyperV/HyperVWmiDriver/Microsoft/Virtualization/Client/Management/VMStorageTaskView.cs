using System;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class VMStorageTaskView : VMTaskView, IVMStorageTask, IVMTask, IVirtualizationManagementObject, IDisposable
{
    internal static class VMStorageTaskWmiMemberNames
    {
        public const string Parent = "Parent";

        public const string Child = "Child";
    }

    public string Parent => RemoveCanonialPathPrefix(GetProperty<string>("Parent"));

    public string Child => RemoveCanonialPathPrefix(GetProperty<string>("Child"));

    private string RemoveCanonialPathPrefix(string pathString)
    {
        if (pathString != null && pathString.StartsWith("\\??\\", StringComparison.Ordinal))
        {
            pathString = pathString.Substring(4);
        }
        return pathString;
    }
}
