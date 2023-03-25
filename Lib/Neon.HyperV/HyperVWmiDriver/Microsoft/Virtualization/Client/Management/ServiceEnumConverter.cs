using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class ServiceEnumConverter<ClientType> : WmiTypeConverter<ClientType>
{
	public sealed override ClientType ConvertFromWmiType(object value)
	{
		if (!(value is string text))
		{
			throw new ArgumentException("The value must be a string.", "value");
		}
		return ConvertFromString(text.ToLowerInvariant());
	}

	protected abstract ClientType ConvertFromString(string value);

	protected virtual string ConvertToString(ClientType value)
	{
		throw new NotImplementedException("The only properties expected to use this enumeration are readonly.");
	}
}
