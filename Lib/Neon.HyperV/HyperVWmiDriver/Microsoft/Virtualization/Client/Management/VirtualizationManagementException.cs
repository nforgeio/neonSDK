using System;

namespace Microsoft.Virtualization.Client.Management;

internal class VirtualizationManagementException : Exception
{
	private string m_Description;

	public string Description
	{
		get
		{
			return m_Description;
		}
		set
		{
			m_Description = value;
		}
	}

	public VirtualizationManagementException()
	{
	}

	public VirtualizationManagementException(string message)
		: base(message ?? string.Empty)
	{
	}

	public VirtualizationManagementException(string message, Exception inner)
		: base(message ?? string.Empty, inner)
	{
	}
}
