using System;

namespace Microsoft.Virtualization.Client.Management;

internal class ObjectNotFoundException : VirtualizationManagementException
{
	public ObjectNotFoundException()
	{
	}

	public ObjectNotFoundException(string message)
		: base(message)
	{
	}

	public ObjectNotFoundException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
