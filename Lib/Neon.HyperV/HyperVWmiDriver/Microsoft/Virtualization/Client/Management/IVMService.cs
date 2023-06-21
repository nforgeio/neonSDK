using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualSystemManagementService")]
internal interface IVMService : IVirtualizationManagementObject, IPutable, IEthernetSwitchFeatureService
{
    IVMServiceSetting Setting { get; }

    IVMServiceCapabilities AllCapabilities { get; }

    IVMComputerSystemSetting GetSettingsCapabilities(SettingsDefineCapabilities capability);

    IVMTask BeginImportSystemDefinition(string systemDefinition, string snapshotFolder, bool generateNewId);

    IVMPlannedComputerSystem EndImportSystemDefinition(IVMTask task, bool returnImportedVM);

    IVMTask BeginImportSnapshotDefinitions(IVMPlannedComputerSystem pvm, string snapshotLocation);

    IEnumerable<IVMComputerSystemSetting> EndImportSnapshotDefinitions(IVMTask task, bool returnSnapshots);

    IVMTask BeginExportSystemDefinition(IVMComputerSystem computerSystem, string exportDirectory, IVMExportSetting exportSetting);

    void EndExportSystemDefinition(IVMTask task);

    IVMTask BeginValidatePlannedVirtualSystem(IVMPlannedComputerSystem plannedComputerSystem);

    List<MsvmError> EndValidatePlannedVirtualSystem(IVMTask task);

    IVMTask BeginRealizePlannedVirtualSystem(IVMPlannedComputerSystem plannedComputerSystem);

    IVMComputerSystem EndRealizePlannedVirtualSystem(IVMTask task, bool returnRealizedVm);

    List<ISummaryInformationSnapshot> GetDefinitionFileSummaryInformation(string[] paths);

    IVMTask BeginCreateVirtualSystem(string name, string location, VirtualSystemSubType generation, Version version = null);

    IVMTask BeginCreateVirtualSystem(IVMComputerSystemSetting systemSetting, IEnumerable<IVMDeviceSetting> resourceSettings);

    IVMTask BeginCreateVirtualSystem(IVMComputerSystemSetting systemSetting, IEnumerable<IVMDeviceSetting> resourceSettings, IVMComputerSystemSetting referenceSetting);

    T EndCreateVirtualSystem<T>(IVMTask task) where T : class, IVMComputerSystemBase;

    IVMTask BeginCreatePlannedComputerSystem(IVMComputerSystemSetting systemSetting, IEnumerable<IVMDeviceSetting> resourceSettings, IVMComputerSystemSetting referenceSetting);

    IVMPlannedComputerSystem EndCreatePlannedComputerSystem(IVMTask task);

    string[] GenerateWorldWidePortNames(int count);

    Task<IVMBootEntry[]> AddBootSourceSettingsAsync(IVMComputerSystemBase computerSystem, BootSourceSetting[] bootSourceSettings);

    IEnumerable<string> GetSupportedVmVersions();

    IEnumerable<SecureBootTemplate> GetSecureBootTemplates();

    bool TryGetSecureBootTemplate(Guid id, out SecureBootTemplate template);
}
