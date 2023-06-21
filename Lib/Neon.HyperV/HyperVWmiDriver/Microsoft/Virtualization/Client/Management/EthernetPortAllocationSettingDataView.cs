#define TRACE
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class EthernetPortAllocationSettingDataView : GsmPoolAllocationSettingView, IEthernetPortAllocationSettingData, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    internal static class CommonBaseMembers
    {
        public const string HostResource = "HostResource";

        public const string Address = "Address";

        public const string TestReplicaPoolId = "TestReplicaPoolID";

        public const string TestReplicaSwitchName = "TestReplicaSwitchName";
    }

    public WmiObjectPath[] HostResources
    {
        get
        {
            WmiObjectPath[] result = null;
            string[] property = GetProperty<string[]>("HostResource");
            if (property != null && property.Length != 0 && !string.IsNullOrEmpty(property[0]))
            {
                try
                {
                    result = property.Select((string p) => GetWmiObjectPathFromPath(p)).ToArray();
                    return result;
                }
                catch (ArgumentException ex)
                {
                    VMTrace.TraceError("HostResource of Msvm_EthernetPortAllocationSettingData is not formatted properly.", ex);
                    return result;
                }
            }
            return result;
        }
        set
        {
            string[] value2 = ((value != null) ? value.Select((WmiObjectPath path) => path.ToString()).ToArray() : new string[0]);
            SetProperty("HostResource", value2);
        }
    }

    public WmiObjectPath HostResource
    {
        get
        {
            if (HostResources != null && HostResources.Length != 0)
            {
                return HostResources[0];
            }
            return null;
        }
        set
        {
            HostResources = ((value == null) ? null : new WmiObjectPath[1] { value });
        }
    }

    public string Address
    {
        get
        {
            return GetProperty<string>("Address");
        }
        set
        {
            SetProperty("Address", value);
        }
    }

    public string TestReplicaPoolId
    {
        get
        {
            return GetProperty<string>("TestReplicaPoolID");
        }
        set
        {
            SetProperty("TestReplicaPoolID", value);
        }
    }

    public string TestReplicaSwitchName
    {
        get
        {
            return GetProperty<string>("TestReplicaSwitchName");
        }
        set
        {
            SetProperty("TestReplicaSwitchName", value);
        }
    }

    public override VMDeviceSettingType VMDeviceSettingType => VMDeviceSettingType.EthernetConnection;

    public IEnumerable<IEthernetSwitchPortFeature> Features => GetRelatedObjects<IEthernetSwitchPortFeature>(base.Associations.EthernetPortToFsd);
}
