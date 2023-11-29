using System;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class ServiceStatusConverter : ServiceEnumConverter<ServiceStatus>
{
    protected override ServiceStatus ConvertFromString(string value)
    {
        return value switch
        {
            "ok" => ServiceStatus.Ok, 
            "error" => ServiceStatus.Error, 
            "degraded" => ServiceStatus.Degraded, 
            "pred fail" => ServiceStatus.PredictingFailure, 
            "starting" => ServiceStatus.Starting, 
            "stopping" => ServiceStatus.Stopping, 
            "service" => ServiceStatus.Service, 
            "unknown" => ServiceStatus.Unknown, 
            _ => throw new ArgumentOutOfRangeException("value", "Can not convert this Service Status value."), 
        };
    }
}
