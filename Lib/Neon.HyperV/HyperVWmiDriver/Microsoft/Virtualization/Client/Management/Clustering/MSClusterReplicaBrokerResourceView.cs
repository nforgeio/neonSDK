using System;

namespace Microsoft.Virtualization.Client.Management.Clustering;

internal sealed class MSClusterReplicaBrokerResourceView : MSClusterResourceView, IMSClusterReplicaBrokerResource, IMSClusterResource, IMSClusterResourceBase, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    private static class WmiClusterResourceReplicaPrivateProperties
    {
        public const string AuthenticationType = "AuthenticationType";

        public const string Authorization = "Authorization";

        public const string CertificateThumbprint = "CertificateThumbPrint";

        public const string HttpPort = "HttpPort";

        public const string HttpsPort = "HttpsPort";

        public const string ListenerPortMapping = "ListenerPortMapping";

        public const string MonitoringInterval = "MonitoringInterval";

        public const string MonitoringStartTime = "MonitoringStartTime";

        public const string RecoveryServerEnabled = "RecoveryServerEnabled";
    }

    public FailoverReplicationAuthenticationType AuthenticationType
    {
        get
        {
            return (FailoverReplicationAuthenticationType)(uint)GetInternalProperty("AuthenticationType");
        }
        set
        {
            SetInternalProperty("AuthenticationType", value);
        }
    }

    public string Authorization
    {
        get
        {
            string empty = string.Empty;
            empty = (string)GetInternalProperty("Authorization");
            if (empty == null)
            {
                return string.Empty;
            }
            return empty;
        }
        set
        {
            SetInternalProperty("Authorization", value);
        }
    }

    public string CertificateThumbprint
    {
        get
        {
            string empty = string.Empty;
            empty = (string)GetInternalProperty("CertificateThumbPrint");
            if (empty == null)
            {
                return string.Empty;
            }
            return empty;
        }
        set
        {
            SetInternalProperty("CertificateThumbPrint", value);
        }
    }

    public int HttpPort
    {
        get
        {
            return Convert.ToInt32((uint)GetInternalProperty("HttpPort"));
        }
        set
        {
            SetInternalProperty("HttpPort", value);
        }
    }

    public int HttpsPort
    {
        get
        {
            return Convert.ToInt32((uint)GetInternalProperty("HttpsPort"));
        }
        set
        {
            SetInternalProperty("HttpsPort", value);
        }
    }

    public string ListenerPortMapping
    {
        get
        {
            return GetInternalProperty("ListenerPortMapping") as string;
        }
        set
        {
            SetInternalProperty("ListenerPortMapping", value);
        }
    }

    public uint MonitoringInterval
    {
        get
        {
            return (uint)GetInternalProperty("MonitoringInterval");
        }
        set
        {
            SetInternalProperty("MonitoringInterval", value);
        }
    }

    public uint MonitoringStartTime
    {
        get
        {
            return (uint)GetInternalProperty("MonitoringStartTime");
        }
        set
        {
            SetInternalProperty("MonitoringStartTime", value);
        }
    }

    public bool RecoveryServerEnabled
    {
        get
        {
            return Convert.ToBoolean((uint)GetInternalProperty("RecoveryServerEnabled"));
        }
        set
        {
            int num = Convert.ToInt32(value);
            SetInternalProperty("RecoveryServerEnabled", num);
        }
    }

    public string GetCapName()
    {
        object[] array = new object[2] { null, false };
        InvokeMethodWithReturn<bool>("GetDependencies", array);
        string obj = (string)array[0];
        if (string.IsNullOrEmpty(obj))
        {
            throw ThrowHelper.CreateInvalidPropertyValueException("Cap Resource Name", typeof(string), null, null);
        }
        string[] array2 = obj.Split('[', ']');
        if (array2.Length < 2)
        {
            throw ThrowHelper.CreateInvalidPropertyValueException("Cap Resource Name", typeof(string), null, null);
        }
        return array2[1];
    }
}
