using System;

namespace Microsoft.Virtualization.Client.Management;

internal class ClassDefinitionMismatchException : VirtualizationManagementException
{
	public ClassDefinitionMismatchException()
	{
	}

	public ClassDefinitionMismatchException(string message)
		: base(message)
	{
	}

	public ClassDefinitionMismatchException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
