using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal abstract class VirtualMachineBase : ComputeResource
{
    public VMComPort ComPort1
    {
        get
        {
            VMComPort[] array = GetComPorts().ToArray();
            if (array.Length != 0)
            {
                return array[0];
            }
            return null;
        }
    }

    public VMComPort ComPort2
    {
        get
        {
            VMComPort[] array = GetComPorts().ToArray();
            if (array.Length != 0)
            {
                return array[1];
            }
            return null;
        }
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    public DvdDrive[] DVDDrives
    {
        get
        {
            ControllerType value = ((Generation == 2) ? ControllerType.SCSI : ControllerType.IDE);
            return FindDrives(value, null, null).OfType<DvdDrive>().ToArray();
        }
    }

    public List<VMFibreChannelHba> FibreChannelHostBusAdapters => GetFibreChannelHbas().ToList();

    public VMFloppyDiskDrive FloppyDrive
    {
        get
        {
            if (Generation == 1)
            {
                return GetFloppyDrive();
            }
            return null;
        }
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    public HardDiskDrive[] HardDrives => FindDrives(null, null, null).OfType<HardDiskDrive>().ToArray();

    public VMRemoteFx3DVideoAdapter RemoteFxAdapter => GetSynthetic3DDisplayController();

    public List<VMIntegrationComponent> VMIntegrationService => GetVMIntegrationComponents().ToList();

    public bool DynamicMemoryEnabled => GetMemory().DynamicMemoryEnabled;

    public long MemoryMaximum => GetMemory().Maximum;

    public long MemoryMinimum => GetMemory().Minimum;

    public long MemoryStartup => GetMemory().Startup;

    public long ProcessorCount => GetProcessor().Count;

    public bool BatteryPassthroughEnabled => GetBattery() != null;

    public int Generation => (int)GetSettings(UpdatePolicy.EnsureUpdated).VirtualSystemSubType;

    public bool IsClustered => GetVMIntegrationComponents().OfType<DataExchangeComponent>().FirstOrDefault()?.IsClustered ?? false;

    public virtual string Notes
    {
        get
        {
            return GetSettings(UpdatePolicy.EnsureUpdated).Notes;
        }
        internal set
        {
            throw new NotImplementedException("Not all derived classes should be allowed to set the Notes property.");
        }
    }

    public Guid? ParentSnapshotId => GetParentSnapshot()?.Id;

    public string ParentSnapshotName => GetParentSnapshot()?.Name;

    public string Path
    {
        get
        {
            return GetSettings(UpdatePolicy.EnsureUpdated).ConfigurationDataRoot;
        }
        internal set
        {
            GetSettings(UpdatePolicy.None).ConfigurationDataRoot = value;
        }
    }

    public long SizeOfSystemFiles => GetSettings(UpdatePolicy.EnsureUpdated).GetSizeOfSystemFiles();

    public abstract VMState State { get; }

    public virtual string Version => GetSettings(UpdatePolicy.EnsureUpdated).Version;

    public bool GuestControlledCacheTypes
    {
        get
        {
            return GetSettings(UpdatePolicy.EnsureUpdated).GuestControlledCacheTypes;
        }
        internal set
        {
            GetSettings(UpdatePolicy.None).GuestControlledCacheTypes = value;
        }
    }

    public uint LowMemoryMappedIoSpace
    {
        get
        {
            return GetSettings(UpdatePolicy.EnsureUpdated).LowMemoryMappedIoSpace;
        }
        internal set
        {
            GetSettings(UpdatePolicy.None).LowMemoryMappedIoSpace = value;
        }
    }

    public ulong HighMemoryMappedIoSpace
    {
        get
        {
            return GetSettings(UpdatePolicy.EnsureUpdated).HighMemoryMappedIoSpace;
        }
        internal set
        {
            GetSettings(UpdatePolicy.None).HighMemoryMappedIoSpace = value;
        }
    }

    public OnOffState? LockOnDisconnect
    {
        get
        {
            return GetSettings(UpdatePolicy.EnsureUpdated).LockOnDisconnect.ToOnOffState();
        }
        internal set
        {
            GetSettings(UpdatePolicy.None).LockOnDisconnect = value.GetValueOrDefault(OnOffState.Off).ToBool();
        }
    }

    internal bool VirtualNumaEnabled
    {
        get
        {
            return GetSettings(UpdatePolicy.EnsureUpdated).VirtualNumaEnabled;
        }
        set
        {
            GetSettings(UpdatePolicy.None).VirtualNumaEnabled = value;
        }
    }

    internal VirtualMachineBase(IVMComputerSystemBase computerSystem, IVMComputerSystemSetting vmSetting)
        : base(computerSystem, vmSetting)
    {
    }

    internal VirtualMachine GetVirtualMachine()
    {
        VirtualMachine virtualMachine = this as VirtualMachine;
        if (virtualMachine == null)
        {
            virtualMachine = (this as VMSnapshot).VM;
        }
        return virtualMachine;
    }

    internal VMSnapshot GetParentSnapshot()
    {
        VMSnapshot result = null;
        IVMComputerSystemSetting parentSnapshot = GetSettings(UpdatePolicy.EnsureAssociatorsUpdated).ParentSnapshot;
        if (parentSnapshot != null)
        {
            result = new VMSnapshot(parentSnapshot, GetVirtualMachine());
        }
        return result;
    }

    internal abstract void Export(IOperationWatcher operationWatcher, string path, CaptureLiveState? captureLiveState);

    internal void ExportInternal(IOperationWatcher operationWatcher, CaptureLiveState? captureLiveState, string path, IVMComputerSystemSetting snapshotSetting, string taskDescription)
    {
        IVMService service = ObjectLocator.GetVirtualizationService(base.Server);
        IVMComputerSystem vm = GetVirtualMachine().GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.EnsureAssociatorsUpdated);
        IVMExportSetting exportSetting = vm.ExportSetting;
        exportSetting.CopyVmStorage = true;
        exportSetting.CopyVmRuntimeInformation = true;
        exportSetting.CreateVmExportSubdirectory = true;
        if (snapshotSetting == null)
        {
            exportSetting.CopySnapshotConfiguration = SnapshotExportMode.ExportAllSnapshots;
        }
        else
        {
            exportSetting.CopySnapshotConfiguration = SnapshotExportMode.ExportOneSnapshot;
            exportSetting.SnapshotVirtualSystem = snapshotSetting;
        }
        exportSetting.CaptureLiveState = (CaptureLiveStateMode)captureLiveState.GetValueOrDefault(CaptureLiveState.CaptureSavedState);
        operationWatcher.PerformOperation(() => service.BeginExportSystemDefinition(vm, path, exportSetting), service.EndExportSystemDefinition, taskDescription, this);
    }

    private IEnumerable<IVMDeviceSetting> GetDeviceSettingsLimited()
    {
        return GetSettings(UpdatePolicy.EnsureAssociatorsUpdated).GetDeviceSettingsLimited(update: false, Constants.UpdateThreshold);
    }

    internal VMFirmware GetFirmware()
    {
        VMFirmware vMFirmware = null;
        IVMComputerSystemSetting settings = GetSettings(UpdatePolicy.EnsureUpdated);
        if (Generation == 2)
        {
            return new VMFirmware(settings, this);
        }
        throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.VMFirmware_Generation1NotSupported, base.Name));
    }

    internal VMBios GetBios()
    {
        VMBios vMBios = null;
        IVMComputerSystemSetting settings = GetSettings(UpdatePolicy.EnsureUpdated);
        if (Generation == 1)
        {
            return new VMBios(settings, this);
        }
        throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.VMBios_Generation2NotSupported, base.Name));
    }

    internal IEnumerable<Drive> GetDrives()
    {
        return GetDriveControllers().SelectMany((VMDriveController controller) => controller.Drives);
    }

    internal IEnumerable<HardDiskDrive> GetHardDiskDrives()
    {
        return GetDrives().OfType<HardDiskDrive>();
    }

    internal IEnumerable<HardDiskDrive> GetVirtualHardDiskDrives()
    {
        return from hardDiskDrive in GetHardDiskDrives()
            where hardDiskDrive.AttachedDiskType == AttachedDiskType.Virtual
            select hardDiskDrive;
    }

    internal IReadOnlyList<Drive> FindDrives(ControllerType? controllerType, int? controllerNumber, int? controllerLocation)
    {
        IEnumerable<VMDriveController> source = GetDriveControllers(controllerType);
        if (controllerNumber.HasValue)
        {
            source = source.Where((VMDriveController controller) => controller.ControllerNumber == controllerNumber.Value);
        }
        IEnumerable<Drive> source2 = source.SelectMany((VMDriveController controller) => controller.Drives);
        if (controllerLocation.HasValue)
        {
            source2 = source2.Where((Drive drive) => drive.ControllerLocation == controllerLocation.Value);
        }
        return source2.ToList();
    }

    internal IReadOnlyList<VMComPort> GetComPorts()
    {
        return (from port in GetDeviceSettings().OfType<IVMSerialPortSetting>()
            select new VMComPort(port, this)).ToList();
    }

    internal IReadOnlyList<VMFibreChannelHba> GetFibreChannelHbas()
    {
        return (from hba in GetDeviceSettings().OfType<IFibreChannelPortSetting>()
            select new VMFibreChannelHba(hba, hba.GetConnectionConfiguration(), this)).ToList();
    }

    internal IReadOnlyList<VMDriveController> GetDriveControllers()
    {
        List<VMDriveController> list = new List<VMDriveController>();
        if (Generation == 1)
        {
            list.AddRange(GetIdeControllers());
        }
        list.AddRange(GetScsiControllers());
        list.AddRange(GetPmemControllers());
        return list;
    }

    internal IReadOnlyList<VMDriveController> GetDriveControllers(ControllerType? controllerType)
    {
        if (Generation == 2 && controllerType == ControllerType.IDE)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(ErrorMessages.VMIdeController_Generation2NotSupported);
        }
        List<VMDriveController> list = new List<VMDriveController>();
        if (Generation != 2 && (!controllerType.HasValue || controllerType == ControllerType.IDE))
        {
            list.AddRange(GetIdeControllers());
        }
        if (!controllerType.HasValue || controllerType == ControllerType.SCSI)
        {
            list.AddRange(GetScsiControllers());
        }
        if (!controllerType.HasValue || controllerType == ControllerType.PMEM)
        {
            list.AddRange(GetPmemControllers());
        }
        return list;
    }

    internal IReadOnlyList<VMIdeController> GetIdeControllers()
    {
        List<VMIdeController> list = null;
        if (Generation == 1)
        {
            return (from IVMDriveControllerSetting setting in from setting in GetDeviceSettings()
                    where setting.VMDeviceSettingType == VMDeviceSettingType.IdeController
                    select setting
                select new VMIdeController(setting, this)).ToList();
        }
        throw ExceptionHelper.CreateInvalidArgumentException(ErrorMessages.VMIdeController_Generation2NotSupported);
    }

    internal IReadOnlyList<VMScsiController> GetScsiControllers()
    {
        return (from setting in GetDeviceSettings()
            where setting.VMDeviceSettingType == VMDeviceSettingType.ScsiSyntheticController
            select setting).Cast<IVMScsiControllerSetting>().Select((IVMScsiControllerSetting setting, int controllerNumber) => new VMScsiController(setting, controllerNumber, this)).ToList();
    }

    internal VMScsiController FindScsiControllerById(string controllerId)
    {
        return GetScsiControllers().FirstOrDefault((VMScsiController controller) => string.Equals(controller.DeviceID, controllerId, StringComparison.OrdinalIgnoreCase)) ?? throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.VMScsiController_NotFound, null);
    }

    internal IReadOnlyList<VMPmemController> GetPmemControllers()
    {
        return (from setting in GetDeviceSettings()
            where setting.VMDeviceSettingType == VMDeviceSettingType.PmemController
            select setting).Cast<IVMPmemControllerSetting>().Select((IVMPmemControllerSetting setting, int controllerNumber) => new VMPmemController(setting, controllerNumber, this)).ToList();
    }

    internal VMPmemController FindPmemControllerById(string controllerId)
    {
        return GetPmemControllers().FirstOrDefault((VMPmemController controller) => string.Equals(controller.DeviceID, controllerId, StringComparison.OrdinalIgnoreCase)) ?? throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.VMPmemController_NotFound, null);
    }

    internal VMFloppyDiskDrive GetFloppyDrive()
    {
        VMFloppyDiskDrive result = null;
        if (Generation == 1)
        {
            IVMDriveSetting iVMDriveSetting = (IVMDriveSetting)GetDeviceSettingsLimited().FirstOrDefault((IVMDeviceSetting drive) => drive.VMDeviceSettingType == VMDeviceSettingType.DisketteSyntheticDrive);
            if (iVMDriveSetting != null)
            {
                IVirtualDiskSetting insertedDisk = iVMDriveSetting.GetInsertedDisk();
                result = new VMFloppyDiskDrive(iVMDriveSetting, insertedDisk, this);
            }
            return result;
        }
        throw ExceptionHelper.CreateInvalidArgumentException(ErrorMessages.VMFloppyDiskDrive_Generation2NotSupported);
    }

    internal IReadOnlyList<VMGpuPartitionAdapter> GetGpuPartitionAdapters(string deviceId)
    {
        IEnumerable<VMGpuPartitionAdapter> source = from setting in GetDeviceSettings().OfType<IVMGpuPartitionAdapterSetting>()
            select new VMGpuPartitionAdapter(setting, this);
        if (!string.IsNullOrEmpty(deviceId))
        {
            source = source.Where((VMGpuPartitionAdapter adapter) => string.Equals(adapter.Id, deviceId, StringComparison.OrdinalIgnoreCase));
        }
        return source.ToList();
    }

    internal IReadOnlyList<VMIntegrationComponent> GetVMIntegrationComponents()
    {
        return (from ic in GetDeviceSettings().OfType<IVMIntegrationComponentSetting>()
            select VMIntegrationComponent.CreateIntegrationComponent(ic, this)).ToList();
    }

    internal IReadOnlyList<VMAssignedDevice> GetAssignedDevices(string instancePath, string locationPath)
    {
        return PciExpressUtilities.FilterAssignableDevices(from device in GetDeviceSettings().OfType<IVMAssignableDeviceSetting>()
            select new VMAssignedDevice(device, this), instancePath, locationPath).ToList();
    }

    internal VMVideo GetSyntheticDisplayController()
    {
        IVMSyntheticDisplayControllerSetting syntheticDisplayControllerSetting = GetSettings(UpdatePolicy.EnsureAssociatorsUpdated).GetSyntheticDisplayControllerSetting();
        if (syntheticDisplayControllerSetting != null)
        {
            return new VMVideo(syntheticDisplayControllerSetting, this);
        }
        return null;
    }

    internal VMKeyboard GetSyntheticKeyboardController()
    {
        IVMSyntheticKeyboardControllerSetting syntheticKeyboardControllerSetting = GetSettings(UpdatePolicy.EnsureAssociatorsUpdated).GetSyntheticKeyboardControllerSetting();
        if (syntheticKeyboardControllerSetting != null)
        {
            return new VMKeyboard(syntheticKeyboardControllerSetting, this);
        }
        return null;
    }

    internal VMMouse GetSyntheticMouseController()
    {
        IVMSyntheticMouseControllerSetting syntheticMouseControllerSetting = GetSettings(UpdatePolicy.EnsureAssociatorsUpdated).GetSyntheticMouseControllerSetting();
        if (syntheticMouseControllerSetting != null)
        {
            return new VMMouse(syntheticMouseControllerSetting, this);
        }
        return null;
    }

    internal VMRemoteFx3DVideoAdapter GetSynthetic3DDisplayController()
    {
        IVMSynthetic3DDisplayControllerSetting iVMSynthetic3DDisplayControllerSetting = GetDeviceSettings().OfType<IVMSynthetic3DDisplayControllerSetting>().FirstOrDefault();
        if (iVMSynthetic3DDisplayControllerSetting == null)
        {
            return null;
        }
        return new VMRemoteFx3DVideoAdapter(iVMSynthetic3DDisplayControllerSetting, this);
    }

    internal S3DisplayController GetS3DisplayController()
    {
        IVMS3DisplayControllerSetting iVMS3DisplayControllerSetting = GetDeviceSettings().OfType<IVMS3DisplayControllerSetting>().FirstOrDefault();
        if (iVMS3DisplayControllerSetting == null)
        {
            return null;
        }
        return new S3DisplayController(iVMS3DisplayControllerSetting, this);
    }

    internal DataExchangeComponent GetDataExchangeComponent()
    {
        IVMDataExchangeComponentSetting iVMDataExchangeComponentSetting = GetDeviceSettings().OfType<IVMDataExchangeComponentSetting>().FirstOrDefault();
        if (iVMDataExchangeComponentSetting == null)
        {
            return null;
        }
        return new DataExchangeComponent(iVMDataExchangeComponentSetting, this);
    }

    internal VMBattery GetBattery()
    {
        IVMBatterySetting iVMBatterySetting = GetDeviceSettings().OfType<IVMBatterySetting>().FirstOrDefault();
        if (iVMBatterySetting == null)
        {
            return null;
        }
        return new VMBattery(iVMBatterySetting, this);
    }

    internal override void InvalidateDeviceSettingsList()
    {
        IVMComputerSystemSetting settings = GetSettings(UpdatePolicy.None);
        settings.InvalidateAssociationCache();
        if (settings.VirtualSystemSubType == VirtualSystemSubType.Type2)
        {
            settings.InvalidatePropertyCache();
        }
    }

    internal VMStorageSetting GetStorageSetting()
    {
        return new VMStorageSetting(GetSettings(UpdatePolicy.EnsureAssociatorsUpdated).GetStorageSetting(), this);
    }

    internal VMSecurity GetSecurity()
    {
        return new VMSecurity(GetSettings(UpdatePolicy.EnsureAssociatorsUpdated).GetSecuritySetting(), new Version(Version));
    }
}
