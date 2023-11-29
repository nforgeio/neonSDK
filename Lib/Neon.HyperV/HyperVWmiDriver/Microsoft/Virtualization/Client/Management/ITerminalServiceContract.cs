using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class ITerminalServiceContract : ITerminalService, IVirtualizationManagementObject
{
    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

    public IVMTask BeginGrantVMConnectAccess(IVMComputerSystem virtualMachine, ICollection<string> trustees)
    {
        return null;
    }

    public IVMTask BeginRevokeVMConnectAccess(IVMComputerSystem virtualMachine, ICollection<string> trustees)
    {
        return null;
    }

    public void EndGrantVMConnectAccess(IVMTask task)
    {
    }

    public void EndRevokeVMConnectAccess(IVMTask task)
    {
    }

    public IEnumerable<IInteractiveSessionAccess> GetVMConnectAccess(IVMComputerSystem virtualMachine)
    {
        return null;
    }

    public abstract void InvalidatePropertyCache();

    public abstract void UpdatePropertyCache();

    public abstract void UpdatePropertyCache(TimeSpan threshold);

    public abstract void RegisterForInstanceModificationEvents(InstanceModificationEventStrategy strategy);

    public abstract void UnregisterForInstanceModificationEvents();

    public abstract void InvalidateAssociationCache();

    public abstract void UpdateAssociationCache();

    public abstract void UpdateAssociationCache(TimeSpan threshold);

    public abstract string GetEmbeddedInstance();

    public abstract void DiscardPendingPropertyChanges();
}
