using System;

namespace Microsoft.Virtualization.Client.Management;

internal class CancelTaskFailedException : VirtualizationOperationFailedException
{
	public CancelTaskFailedException()
	{
	}

	public CancelTaskFailedException(string message)
		: base(message)
	{
	}

	public CancelTaskFailedException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
