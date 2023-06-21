#define TRACE
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class VMServiceSettingView : View, IVMServiceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    internal static class WmiMemberNames
    {
        public const string DefaultExternalDataRoot = "DefaultExternalDataRoot";

        public const string DefaultVirtualHardDiskPath = "DefaultVirtualHardDiskPath";

        public const string MinimumMacAddress = "MinimumMacAddress";

        public const string MaximumMacAddress = "MaximumMacAddress";

        public const string MinimumWorldWidePortName = "MinimumWWPNAddress";

        public const string MaximumWorldWidePortName = "MaximumWWPNAddress";

        public const string AssignedWorldWideNodeName = "CurrentWWNNAddress";

        public const string NumaSpanningEnabled = "NumaSpanningEnabled";

        public const string EnhancedSessionModeEnabled = "EnhancedSessionModeEnabled";

        public const string HypervisorRootSchedulerEnabled = "HypervisorRootSchedulerEnabled";

        public const string Put = "ModifyServiceSettings";
    }

    public string DefaultExternalDataRoot
    {
        get
        {
            return GetProperty<string>("DefaultExternalDataRoot");
        }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }
            SetProperty("DefaultExternalDataRoot", value);
        }
    }

    public string DefaultVirtualHardDiskPath
    {
        get
        {
            return GetProperty<string>("DefaultVirtualHardDiskPath");
        }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }
            SetProperty("DefaultVirtualHardDiskPath", value);
        }
    }

    public string MinimumMacAddress
    {
        get
        {
            return GetProperty<string>("MinimumMacAddress");
        }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }
            SetProperty("MinimumMacAddress", value);
        }
    }

    public string MaximumMacAddress
    {
        get
        {
            return GetProperty<string>("MaximumMacAddress");
        }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }
            SetProperty("MaximumMacAddress", value);
        }
    }

    public string MinimumWorldWidePortName
    {
        get
        {
            return GetProperty<string>("MinimumWWPNAddress");
        }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }
            SetProperty("MinimumWWPNAddress", value);
        }
    }

    public string MaximumWorldWidePortName
    {
        get
        {
            return GetProperty<string>("MaximumWWPNAddress");
        }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }
            SetProperty("MaximumWWPNAddress", value);
        }
    }

    public string AssignedWorldWideNodeName
    {
        get
        {
            return GetProperty<string>("CurrentWWNNAddress");
        }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }
            SetProperty("CurrentWWNNAddress", value);
        }
    }

    public bool NumaSpanningEnabled
    {
        get
        {
            return GetProperty<bool>("NumaSpanningEnabled");
        }
        set
        {
            SetProperty("NumaSpanningEnabled", value);
        }
    }

    public bool NumaSpanningSupported => DoesPropertyExist("NumaSpanningEnabled");

    public bool EnhancedSessionModeSupported => DoesPropertyExist("EnhancedSessionModeEnabled");

    public bool EnhancedSessionModeEnabled
    {
        get
        {
            return GetProperty<bool>("EnhancedSessionModeEnabled");
        }
        set
        {
            SetProperty("EnhancedSessionModeEnabled", value);
        }
    }

    public bool HypervisorRootSchedulerEnabled => GetPropertyOrDefault("HypervisorRootSchedulerEnabled", defaultValue: false);

    protected override IVMTask BeginPutInternal(IDictionary<string, object> properties)
    {
        string embeddedInstance = GetEmbeddedInstance(properties);
        IProxy serviceProxy = GetServiceProxy();
        object[] array = new object[2] { embeddedInstance, null };
        string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ModifyVirtualizationSettingsFailed, base.Server);
        uint result = serviceProxy.InvokeMethod("ModifyServiceSettings", array);
        VMTrace.TraceUserActionInitiatedWithProperties("Modifying service settings.", properties);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
        iVMTask.PutProperties = properties;
        iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
        return iVMTask;
    }
}
