using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVM3dVideoPhysical3dGPUContract : IVM3dVideoPhysical3dGPU, IVMDevice, IVirtualizationManagementObject
{
    public string Name => null;

    public bool EnabledForVirtualization => false;

    public bool CompatibleForVirtualization => false;

    public string GPUID => null;

    public string InstanceID => null;

    public long AdapterIndexID => 0L;

    public string DirectXVersion => null;

    public string PixelShaderVersion => null;

    public long DedicatedVideoMemory => 0L;

    public long DedicatedSystemMemory => 0L;

    public long SharedSystemMemory => 0L;

    public long AvailableVideoMemory => 0L;

    public long TotalVideoMemory => 0L;

    public string DriverProvider => null;

    public DateTime DriverDate => default(DateTime);

    public DateTime DriverInstalled => default(DateTime);

    public string DriverVersion => null;

    public string DriverModelVersion => null;

    public abstract string FriendlyName { get; }

    public abstract string DeviceId { get; }

    public abstract IVMComputerSystem VirtualComputerSystem { get; }

    public abstract IVMDeviceSetting VirtualDeviceSetting { get; }

    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

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
