using System;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class VMNetworkTaskView : VMTaskView, IVMNetworkTask, IVMTask, IVirtualizationManagementObject, IDisposable
{
    internal override void Initialize(IProxy proxy, ObjectKey key)
    {
        InitializeInternal(proxy, key, registerWithTaskConnectionTester: false);
    }

    public override void InformServerDisconnected(string disconnectedErrorMsg)
    {
        base.InformServerDisconnected(disconnectedErrorMsg);
    }
}
