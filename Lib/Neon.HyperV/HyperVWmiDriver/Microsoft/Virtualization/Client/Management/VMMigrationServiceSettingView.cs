#define TRACE
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class VMMigrationServiceSettingView : View, IVMMigrationServiceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    internal static class WmiMemberNames
    {
        public const string EnableVirtualSystemMigration = "EnableVirtualSystemMigration";

        public const string MaximumActiveVirtualSystemMigration = "MaximumActiveVirtualSystemMigration";

        public const string MaximumActiveStorageMigration = "MaximumActiveStorageMigration";

        public const string AuthenticationType = "AuthenticationType";

        public const string EnableCompression = "EnableCompression";

        public const string EnableSmbTransport = "EnableSmbTransport";
    }

    public bool EnableVirtualSystemMigration
    {
        get
        {
            return GetProperty<bool>("EnableVirtualSystemMigration");
        }
        set
        {
            SetProperty("EnableVirtualSystemMigration", value);
        }
    }

    public long MaximumActiveVirtualSystemMigration
    {
        get
        {
            return NumberConverter.UInt32ToInt64(GetProperty<uint>("MaximumActiveVirtualSystemMigration"));
        }
        set
        {
            uint num = NumberConverter.Int64ToUInt32(value);
            SetProperty("MaximumActiveVirtualSystemMigration", num);
        }
    }

    public long MaximumActiveStorageMigration
    {
        get
        {
            return NumberConverter.UInt32ToInt64(GetProperty<uint>("MaximumActiveStorageMigration"));
        }
        set
        {
            uint num = NumberConverter.Int64ToUInt32(value);
            SetProperty("MaximumActiveStorageMigration", num);
        }
    }

    public int AuthenticationType
    {
        get
        {
            return (int)GetProperty<uint>("AuthenticationType");
        }
        set
        {
            SetProperty("AuthenticationType", (uint)value);
        }
    }

    public bool EnableCompression
    {
        get
        {
            return GetProperty<bool>("EnableCompression");
        }
        set
        {
            SetProperty("EnableCompression", value);
        }
    }

    public bool EnableSmbTransport
    {
        get
        {
            return GetProperty<bool>("EnableSmbTransport");
        }
        set
        {
            SetProperty("EnableSmbTransport", value);
        }
    }

    public bool SmbTransportOptionAvailable => DoesPropertyExist("EnableSmbTransport");

    public bool CompressionOptionAvailable => DoesPropertyExist("EnableCompression");

    public IEnumerable<IVMMigrationNetworkSetting> NetworkSettings => GetRelatedObjects<IVMMigrationNetworkSetting>(base.Associations.MigrationServiceSettingComponent);

    public IEnumerable<IVMMigrationNetworkSetting> GetUserManagedNetworkSettings()
    {
        return NetworkSettings?.Where((IVMMigrationNetworkSetting setting) => setting.Tags.Contains("Microsoft:UserManaged"));
    }

    protected override IVMTask BeginPutInternal(IDictionary<string, object> properties)
    {
        string embeddedInstance = GetEmbeddedInstance(properties);
        IProxy proxy = base.ProxyFactory.GetProxy(ObjectKeyCreator.CreateMigrationServiceObjectKey(base.Server), delayInitializePropertyCache: true);
        object[] array = new object[2] { embeddedInstance, null };
        string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ModifyVirtualizationSettingsFailed, base.Server);
        VMTrace.TraceUserActionInitiatedWithProperties("Modifying migration service settings.", properties);
        uint result = proxy.InvokeMethod("ModifyServiceSettings", array);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
        iVMTask.PutProperties = properties;
        iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
        return iVMTask;
    }
}
