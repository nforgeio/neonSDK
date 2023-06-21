using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualSystemSettingData")]
internal interface IVMComputerSystemSetting : IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
    [Key]
    string InstanceId { get; }

    string Name { get; set; }

    string Version { get; set; }

    string SystemName { get; }

    string ConfigurationId { get; }

    VirtualSystemType VirtualSystemType { get; }

    VirtualSystemSubType VirtualSystemSubType { get; set; }

    bool IsSaved { get; }

    string Notes { get; set; }

    string Description { get; }

    DateTime CreationTime { get; }

    bool VirtualNumaEnabled { get; set; }

    bool BiosNumLock { get; set; }

    bool GuestControlledCacheTypes { get; set; }

    ushort NetworkBootPreferredProtocol { get; set; }

    IVMComputerSystemSetting ParentSnapshot { get; }

    IEnumerable<IVMComputerSystemSetting> ChildSettings { get; }

    bool SecureBootEnabled { get; set; }

    Guid? SecureBootTemplateId { get; set; }

    ConsoleModeType ConsoleMode { get; set; }

    EnhancedSessionTransportType EnhancedSessionTransportType { get; set; }

    bool LockOnDisconnect { get; set; }

    bool PauseAfterBootFailure { get; set; }

    uint LowMemoryMappedIoSpace { get; set; }

    ulong HighMemoryMappedIoSpace { get; set; }

    bool IsAutomaticCheckpoint { get; }

    bool EnableAutomaticCheckpoints { get; set; }

    string ConfigurationDataRoot { get; set; }

    string SnapshotDataRoot { get; set; }

    UserSnapshotType UserSnapshotType { get; set; }

    string SwapFileDataRoot { get; set; }

    ServiceStartOperation AutomaticStartupAction { get; set; }

    ServiceStopOperation AutomaticShutdownAction { get; set; }

    TimeSpan AutomaticStartupActionDelay { get; set; }

    CriticalErrorAction AutomaticCriticalErrorAction { get; set; }

    TimeSpan AutomaticCriticalErrorActionTimeout { get; set; }

    bool IsSnapshot { get; }

    IVMComputerSystemBase VMComputerSystem { get; }

    BootDevice[] GetBootOrder();

    void SetBootOrder(BootDevice[] bootOrder);

    IEnumerable<IVMBootEntry> GetFirmwareBootOrder();

    void SetFirmwareBootOrder(IEnumerable<IVMBootEntry> bootEntries);

    void Apply();

    IVMTask BeginApply();

    void EndApply(IVMTask applyTask);

    IVMTask BeginDeleteTree();

    void EndDeleteTree(IVMTask deleteTask);

    IVMTask BeginClearSnapshotState();

    void EndClearSnapshotState(IVMTask clearTask);

    byte[] GetThumbnailImage(int widthPixels, int heightPixels);

    long GetSizeOfSystemFiles();

    IEnumerable<IVMDeviceSetting> GetDeviceSettings();

    IEnumerable<IVMDeviceSetting> GetDeviceSettingsLimited(bool update, TimeSpan threshold);

    IVMMemorySetting GetMemorySetting();

    IVMSecuritySetting GetSecuritySetting();

    IVMProcessorSetting GetProcessorSetting();

    IVMSyntheticDisplayControllerSetting GetSyntheticDisplayControllerSetting();

    IVMSyntheticKeyboardControllerSetting GetSyntheticKeyboardControllerSetting();

    IVMSyntheticMouseControllerSetting GetSyntheticMouseControllerSetting();

    IVMStorageSetting GetStorageSetting();
}
