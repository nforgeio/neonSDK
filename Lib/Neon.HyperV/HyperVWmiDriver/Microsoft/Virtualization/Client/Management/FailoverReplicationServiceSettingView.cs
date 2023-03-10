#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class FailoverReplicationServiceSettingView : View, IFailoverReplicationServiceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
	internal static class WmiMemberNames
	{
		public const string AllowedAuthenticationType = "AllowedAuthenticationType";

		public const string CertificateThumbprint = "CertificateThumbPrint";

		public const string HttpPort = "HttpPort";

		public const string HttpsPort = "HttpsPort";

		public const string MonitoringInterval = "MonitoringInterval";

		public const string MonitoringStartTime = "MonitoringStartTime";

		public const string RecoveryServerEnabled = "RecoveryServerEnabled";

		public const string Put = "ModifyServiceSettings";
	}

	public RecoveryAuthenticationType AllowedAuthenticationType
	{
		get
		{
			return (RecoveryAuthenticationType)GetProperty<ushort>("AllowedAuthenticationType");
		}
		set
		{
			SetProperty("AllowedAuthenticationType", NumberConverter.Int32ToUInt16((int)value));
		}
	}

	public string CertificateThumbprint
	{
		get
		{
			return GetProperty<string>("CertificateThumbPrint");
		}
		set
		{
			SetProperty("CertificateThumbPrint", value);
		}
	}

	public int HttpPort
	{
		get
		{
			return NumberConverter.UInt16ToInt32(GetProperty<ushort>("HttpPort"));
		}
		set
		{
			SetProperty("HttpPort", NumberConverter.Int32ToUInt16(value));
		}
	}

	public int HttpsPort
	{
		get
		{
			return NumberConverter.UInt16ToInt32(GetProperty<ushort>("HttpsPort"));
		}
		set
		{
			SetProperty("HttpsPort", NumberConverter.Int32ToUInt16(value));
		}
	}

	public uint MonitoringInterval
	{
		get
		{
			return GetProperty<uint>("MonitoringInterval");
		}
		set
		{
			SetProperty("MonitoringInterval", value);
		}
	}

	public TimeSpan MonitoringStartTime
	{
		get
		{
			return GetProperty<TimeSpan>("MonitoringStartTime");
		}
		set
		{
			SetProperty("MonitoringStartTime", value);
		}
	}

	public bool RecoveryServerEnabled
	{
		get
		{
			return GetProperty<bool>("RecoveryServerEnabled");
		}
		set
		{
			SetProperty("RecoveryServerEnabled", value);
		}
	}

	protected override IVMTask BeginPutInternal(IDictionary<string, object> properties)
	{
		string embeddedInstance = GetEmbeddedInstance(properties);
		IProxy failoverReplicationServiceProxy = GetFailoverReplicationServiceProxy();
		object[] array = new object[2] { embeddedInstance, null };
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ModifyFailoverReplicationServiceSettingsFailed, base.Server);
		uint result = failoverReplicationServiceProxy.InvokeMethod("ModifyServiceSettings", array);
		VMTrace.TraceUserActionInitiatedWithProperties("Modifying failover replication service settings.", properties);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
		iVMTask.PutProperties = properties;
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}
}
