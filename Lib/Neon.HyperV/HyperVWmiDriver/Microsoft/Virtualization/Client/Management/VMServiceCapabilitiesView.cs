using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualSystemManagementCapabilities")]
internal class VMServiceCapabilitiesView : View, IVMServiceCapabilities, IVirtualizationManagementObject
{
    public IEnumerable<IVMComputerSystemSetting> ComputerSystemSettings => GetRelatedObjects<IVMComputerSystemSetting>(base.Associations.SettingsDefineCapabilities);

    public IVMComputerSystemSetting DefaultComputerSystemSetting => GetCapabilitiesSettings(CapabilitiesValueRole.Default, CapabilitiesValueRange.Point).Single();

    public IEnumerable<IVMComputerSystemSetting> SupportedVersionSettings => from s in GetCapabilitiesSettings(CapabilitiesValueRole.Supported, CapabilitiesValueRange.Point)
        where !string.IsNullOrEmpty(s.Version)
        select s;

    public IEnumerable<IVMComputerSystemSetting> SupportedSecureBootTemplateSettings => from s in GetCapabilitiesSettings(CapabilitiesValueRole.Supported, CapabilitiesValueRange.Point)
        where s.SecureBootTemplateId.HasValue
        select s;

    private IEnumerable<IVMComputerSystemSetting> GetCapabilitiesSettings(CapabilitiesValueRole role, CapabilitiesValueRange range)
    {
        WmiRelationship settingsDefineCapabilitiesRelationship = base.Associations.SettingsDefineCapabilitiesRelationship;
        return (from sdc in GetRelatedObjects<ISettingsDefineCapabilities>(settingsDefineCapabilitiesRelationship)
            where sdc.ValueRole == role && sdc.ValueRange == range
            select sdc.PartComponent).OfType<IVMComputerSystemSetting>();
    }
}
