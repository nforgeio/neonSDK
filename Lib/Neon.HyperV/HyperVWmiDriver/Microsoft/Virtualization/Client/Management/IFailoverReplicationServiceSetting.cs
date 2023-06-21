using System;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ReplicationServiceSettingData")]
internal interface IFailoverReplicationServiceSetting : IVirtualizationManagementObject, IPutableAsync, IPutable
{
    RecoveryAuthenticationType AllowedAuthenticationType { get; set; }

    string CertificateThumbprint { get; set; }

    int HttpPort { get; set; }

    int HttpsPort { get; set; }

    uint MonitoringInterval { get; set; }

    TimeSpan MonitoringStartTime { get; set; }

    bool RecoveryServerEnabled { get; set; }
}
