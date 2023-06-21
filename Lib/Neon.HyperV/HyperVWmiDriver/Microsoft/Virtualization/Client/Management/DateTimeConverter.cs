using System;

namespace Microsoft.Virtualization.Client.Management;

internal class DateTimeConverter : WmiTypeConverter<DateTime>
{
    public override DateTime ConvertFromWmiType(object value)
    {
        return ManagementDateTimeConverter.ToDateTime((string)value);
    }
}
