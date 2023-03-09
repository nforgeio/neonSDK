using System;

namespace Microsoft.Virtualization.Client.Management;

internal class ServerObjectDeletedException : ObjectNotFoundException
{
	public ServerObjectDeletedException()
	{
	}

	public ServerObjectDeletedException(string message)
		: base(message)
	{
	}

	public ServerObjectDeletedException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
