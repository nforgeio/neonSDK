using System;

namespace Microsoft.Virtualization.Client.Management;

internal class TimeSpanConverter : WmiTypeConverter<TimeSpan>
{
    public override TimeSpan ConvertFromWmiType(object value)
    {
        return ManagementDateTimeConverter.ToTimeSpan((string)value);
    }
}
