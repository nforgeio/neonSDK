namespace Microsoft.Virtualization.Client.Management;

internal abstract class WmiTypeConverter<ClientType>
{
    public abstract ClientType ConvertFromWmiType(object value);
}
