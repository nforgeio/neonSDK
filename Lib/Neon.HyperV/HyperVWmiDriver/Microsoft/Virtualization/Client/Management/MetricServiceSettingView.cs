#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class MetricServiceSettingView : View, IMetricServiceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    internal static class WmiMemberNames
    {
        public const string MetricsFlushInterval = "MetricsFlushInterval";

        public const string Put = "ModifyServiceSettings";
    }

    public TimeSpan FlushInterval
    {
        get
        {
            return GetProperty<TimeSpan>("MetricsFlushInterval");
        }
        set
        {
            SetProperty("MetricsFlushInterval", value);
        }
    }

    protected override IVMTask BeginPutInternal(IDictionary<string, object> properties)
    {
        string embeddedInstance = GetEmbeddedInstance(properties);
        IProxy proxy = base.ProxyFactory.GetProxy(ObjectKeyCreator.CreateMetricServiceObjectKey(base.Server), delayInitializePropertyCache: true);
        object[] array = new object[2] { embeddedInstance, null };
        string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ModifyVirtualizationSettingsFailed, base.Server);
        uint result = proxy.InvokeMethod("ModifyServiceSettings", array);
        VMTrace.TraceUserActionInitiatedWithProperties("Modifying migration service settings.", properties);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
        iVMTask.PutProperties = properties;
        iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
        return iVMTask;
    }
}
