#define TRACE
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class ResourcePoolAllocationSettingView : VMDeviceSettingView, IResourcePoolAllocationSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    internal static class WmiPropertyNames
    {
        public const string Parent = "Parent";

        public const string HostResource = "HostResource";

        public const string Put = "ModifyPoolResources";
    }

    private bool m_IsPoolRasd;

    public bool IsPoolRasd
    {
        get
        {
            return m_IsPoolRasd;
        }
        set
        {
            m_IsPoolRasd = value;
        }
    }

    public WmiObjectPath Parent
    {
        get
        {
            string property = GetProperty<string>("Parent");
            if (string.IsNullOrEmpty(property))
            {
                return null;
            }
            return new WmiObjectPath(property);
        }
        set
        {
            string value2 = string.Empty;
            if (value != null)
            {
                value2 = value.ToString();
            }
            SetProperty("Parent", value2);
        }
    }

    public IResourcePool ChildResourcePool => GetRelatedObject<IResourcePool>(base.Associations.PoolSettingToResourcePool, throwIfNotFound: false);

    public IResourcePool ParentResourcePool => GetRelatedObject<IResourcePool>(base.Associations.ResourceAllocationFromPool, throwIfNotFound: false);

    protected override IVMTask BeginPutInternal(IDictionary<string, object> properties)
    {
        if (IsPoolRasd)
        {
            string arg = string.Empty;
            IResourcePool childResourcePool = ChildResourcePool;
            if (childResourcePool != null)
            {
                arg = childResourcePool.PoolId;
            }
            string embeddedInstance = GetEmbeddedInstance(properties);
            IProxy resourcePoolConfigurationServiceProxy = GetResourcePoolConfigurationServiceProxy();
            object[] array = new object[4]
            {
                childResourcePool,
                new IResourcePool[1] { ParentResourcePool },
                new string[1] { embeddedInstance },
                null
            };
            string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ModifyPoolResourcesFailed, arg);
            VMTrace.TraceUserActionInitiatedWithProperties(string.Format(CultureInfo.InvariantCulture, "Modifying allocation properties of resource pool '{0}'.", arg), properties);
            uint result = resourcePoolConfigurationServiceProxy.InvokeMethod("ModifyPoolResources", array);
            IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[3]);
            iVMTask.PutProperties = properties;
            iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
            return iVMTask;
        }
        return base.BeginPutInternal(properties);
    }
}
