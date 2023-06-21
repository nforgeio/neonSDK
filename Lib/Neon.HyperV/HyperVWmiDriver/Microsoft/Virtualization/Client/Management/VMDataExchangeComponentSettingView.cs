#define TRACE
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class VMDataExchangeComponentSettingView : VMIntegrationComponentSettingView, IVMDataExchangeComponentSetting, IVMIntegrationComponentSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    internal static class WmiDataExchangeComponentSettingPropertyNames
    {
        public const string HostOnlyItems = "HostOnlyItems";
    }

    public const string VirtualMachineIsClusteredPropertyName = "VirtualMachineIsClustered";

    public override VMDeviceSettingType VMDeviceSettingType => VMDeviceSettingType.DataExchange;

    public bool VMIsClustered
    {
        get
        {
            try
            {
                return GetHostOnlyKeyValuePairItems().Any((DataExchangeItem dataItem) => string.Equals(dataItem.Name, "VirtualMachineIsClustered", StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                VMTrace.TraceWarning("Failed to determine if VM is clustered.", ex);
            }
            return false;
        }
    }

    public IEnumerable<DataExchangeItem> GetHostOnlyKeyValuePairItems()
    {
        try
        {
            return from embeddedInstance in GetProperty<string[]>("HostOnlyItems")
                select EmbeddedInstance.ConvertTo<DataExchangeItem>(base.Server, embeddedInstance);
        }
        catch (Exception ex)
        {
            VMTrace.TraceWarning("Failed to get host only KVP items.", ex);
            return Enumerable.Empty<DataExchangeItem>();
        }
    }
}
