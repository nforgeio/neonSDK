using System;

namespace Microsoft.Virtualization.Client.Management;

internal class NoWmiMappingException : Exception
{
	public NoWmiMappingException()
	{
	}

	public NoWmiMappingException(string message)
		: base(message)
	{
	}

	public NoWmiMappingException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
