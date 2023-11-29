#define TRACE
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class ResourcePoolSettingView : View, IResourcePoolSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    internal static class WmiResourcePoolPropertyNames
    {
        public const string FriendlyName = "ElementName";

        public const string PoolId = "PoolID";

        public const string Notes = "Notes";

        public const string Put = "ModifyPoolSettings";
    }

    public string FriendlyName
    {
        get
        {
            return GetProperty<string>("ElementName");
        }
        set
        {
            SetProperty("ElementName", value ?? string.Empty);
        }
    }

    public string PoolId
    {
        get
        {
            return GetProperty<string>("PoolID") ?? string.Empty;
        }
        set
        {
            SetProperty("PoolID", value ?? string.Empty);
        }
    }

    public IResourcePool ResourcePool => GetRelatedObject<IResourcePool>(base.Associations.PoolSettingToResourcePool);

    public string Notes
    {
        get
        {
            return GetProperty<string>("Notes") ?? string.Empty;
        }
        set
        {
            SetProperty("Notes", value ?? string.Empty);
        }
    }

    protected override IVMTask BeginPutInternal(IDictionary<string, object> properties)
    {
        string arg = (base.Proxy.GetProperty("PoolID") as string) ?? string.Empty;
        string embeddedInstance = GetEmbeddedInstance(properties);
        IProxy resourcePoolConfigurationServiceProxy = GetResourcePoolConfigurationServiceProxy();
        object[] array = new object[3] { ResourcePool, embeddedInstance, null };
        string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ModifyResourcePoolSettingsFailed, arg);
        VMTrace.TraceUserActionInitiatedWithProperties(string.Format(CultureInfo.InvariantCulture, "Modifying Resource Pool settings with PoolID '{0}'", arg), properties);
        uint result = resourcePoolConfigurationServiceProxy.InvokeMethod("ModifyPoolSettings", array);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[2]);
        iVMTask.PutProperties = properties;
        iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
        return iVMTask;
    }
}
