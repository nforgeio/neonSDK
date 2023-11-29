using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal abstract class VMDevice : VMComponentObject, IUpdatable
{
    [VirtualizationObjectIdentifier(IdentifierFlags.FriendlyName)]
    public virtual string Name
    {
        get
        {
            return GetDeviceDataUpdater().GetData(UpdatePolicy.EnsureUpdated).FriendlyName;
        }
        internal set
        {
            GetDeviceDataUpdater().GetData(UpdatePolicy.None).FriendlyName = value;
        }
    }

    [VirtualizationObjectIdentifier(IdentifierFlags.UniqueIdentifier)]
    public virtual string Id => GetDeviceDataUpdater().GetData(UpdatePolicy.EnsureUpdated).DeviceId;

    internal abstract string PutDescription { get; }

    internal VMDevice(IVMDeviceSetting setting, ComputeResource parentComputeResource)
        : base(setting, parentComputeResource)
    {
    }

    void IUpdatable.Put(IOperationWatcher operationWatcher)
    {
        PutSelf(operationWatcher);
    }

    internal virtual void PutSelf(IOperationWatcher operationWatcher)
    {
        IVMDeviceSetting data = GetDeviceDataUpdater().GetData(UpdatePolicy.None);
        PutOneDeviceSetting(data, operationWatcher);
    }

    internal virtual void PutOneDeviceSetting(IVMDeviceSetting deviceSetting, IOperationWatcher operationWatcher)
    {
        operationWatcher.PerformPut(deviceSetting, PutDescription, this);
    }

    internal abstract IDataUpdater<IVMDeviceSetting> GetDeviceDataUpdater();

    protected static TDeviceSetting CreateTemplateDeviceSetting<TDeviceSetting>(Server server, VMDeviceSettingType deviceType) where TDeviceSetting : IVMDeviceSetting
    {
        return (TDeviceSetting)ObjectLocator.GetHostComputerSystem(server).GetSettingCapabilities(deviceType, Capabilities.DefaultCapability);
    }

    internal void RemoveInternal<TDeviceSetting>(TDeviceSetting deviceSetting, string taskDescription, IOperationWatcher operationWatcher) where TDeviceSetting : IVMDeviceSetting, IDeleteableAsync
    {
        operationWatcher.PerformDelete(deviceSetting, taskDescription, this);
        GetParentAs<ComputeResource>().InvalidateDeviceSettingsList();
    }
}
