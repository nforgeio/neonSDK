using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_KvpExchangeComponentSettingData")]
internal interface IVMDataExchangeComponentSetting : IVMIntegrationComponentSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    bool VMIsClustered { get; }

    IEnumerable<DataExchangeItem> GetHostOnlyKeyValuePairItems();
}
