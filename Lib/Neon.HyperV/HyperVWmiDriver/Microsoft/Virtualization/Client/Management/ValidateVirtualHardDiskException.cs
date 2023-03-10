using System;

namespace Microsoft.Virtualization.Client.Management;

internal class ValidateVirtualHardDiskException : VirtualizationOperationFailedException
{
	private string m_ParentPath;

	private string m_ChildPath;

	public string ParentPath
	{
		get
		{
			return m_ParentPath;
		}
		set
		{
			m_ParentPath = value;
		}
	}

	public string ChildPath
	{
		get
		{
			return m_ChildPath;
		}
		set
		{
			m_ChildPath = value;
		}
	}

	public ValidateVirtualHardDiskException()
	{
	}

	public ValidateVirtualHardDiskException(string message)
		: base(message)
	{
	}

	public ValidateVirtualHardDiskException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
