#define TRACE
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class VMSecuritySettingView : View, IVMSecuritySetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    internal static class WmiPropertyNames
    {
        public const string TpmEnabled = "TpmEnabled";

        public const string KsdEnabled = "KsdEnabled";

        public const string EncryptStateAndVmMigrationTraffic = "EncryptStateAndVmMigrationTraffic";

        public const string DataProtectionRequested = "DataProtectionRequested";

        public const string ShieldingRequested = "ShieldingRequested";

        public const string VirtualizationBasedSecurityOptOut = "VirtualizationBasedSecurityOptOut";

        public const string BindToHostTpm = "BindToHostTpm";
    }

    public bool TpmEnabled
    {
        get
        {
            return GetProperty<bool>("TpmEnabled");
        }
        set
        {
            SetProperty("TpmEnabled", value);
        }
    }

    public bool KsdEnabled
    {
        get
        {
            return GetProperty<bool>("KsdEnabled");
        }
        set
        {
            SetProperty("KsdEnabled", value);
        }
    }

    public bool ShieldingRequested
    {
        get
        {
            return GetProperty<bool>("ShieldingRequested");
        }
        set
        {
            SetProperty("ShieldingRequested", value);
        }
    }

    public bool EncryptStateAndVmMigrationTraffic
    {
        get
        {
            return GetPropertyOrDefault("EncryptStateAndVmMigrationTraffic", defaultValue: false);
        }
        set
        {
            SetProperty("EncryptStateAndVmMigrationTraffic", value);
        }
    }

    public bool DataProtectionRequested
    {
        get
        {
            return GetProperty<bool>("DataProtectionRequested");
        }
        set
        {
            SetProperty("DataProtectionRequested", value);
        }
    }

    public bool VirtualizationBasedSecurityOptOut
    {
        get
        {
            return GetPropertyOrDefault("VirtualizationBasedSecurityOptOut", defaultValue: false);
        }
        set
        {
            SetProperty("VirtualizationBasedSecurityOptOut", value);
        }
    }

    public bool BindToHostTpm
    {
        get
        {
            return GetPropertyOrDefault("BindToHostTpm", defaultValue: false);
        }
        set
        {
            SetProperty("BindToHostTpm", value);
        }
    }

    protected override IVMTask BeginPutInternal(IDictionary<string, object> properties)
    {
        string embeddedInstance = GetEmbeddedInstance();
        IProxy proxy = base.ProxyFactory.GetProxy(ObjectKeyCreator.CreateSecurityServiceObjectKey(base.Server), delayInitializePropertyCache: true);
        object[] array = new object[2] { embeddedInstance, null };
        VMTrace.TraceUserActionInitiatedWithProperties("Modifying security settings.", properties);
        uint result = proxy.InvokeMethod("ModifySecuritySettings", array);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
        iVMTask.PutProperties = properties;
        iVMTask.ClientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ModifySecuritySettingsFailed);
        return iVMTask;
    }
}
