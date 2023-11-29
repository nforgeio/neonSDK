using System;

namespace Microsoft.Virtualization.Client.Management;

internal class GuidStringConverter : WmiTypeConverter<Guid>
{
    public override Guid ConvertFromWmiType(object value)
    {
        return new Guid((string)value);
    }
}
