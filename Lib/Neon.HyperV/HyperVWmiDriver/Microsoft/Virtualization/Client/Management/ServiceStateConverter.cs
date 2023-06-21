using System;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class ServiceStateConverter : ServiceEnumConverter<ServiceState>
{
    protected override ServiceState ConvertFromString(string value)
    {
        return value switch
        {
            "stopped" => ServiceState.Stopped, 
            "start pending" => ServiceState.StartPending, 
            "stop pending" => ServiceState.StopPending, 
            "running" => ServiceState.Running, 
            "unknown" => ServiceState.Unknown, 
            _ => throw new ArgumentOutOfRangeException("value"), 
        };
    }
}
