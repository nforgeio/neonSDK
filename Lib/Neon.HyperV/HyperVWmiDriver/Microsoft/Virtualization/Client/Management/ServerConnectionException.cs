using System;

namespace Microsoft.Virtualization.Client.Management;

internal class ServerConnectionException : VirtualizationManagementException
{
	private ServerConnectionIssue m_Issue;

	public ServerConnectionIssue Issue
	{
		get
		{
			return m_Issue;
		}
		set
		{
			m_Issue = value;
		}
	}

	public ServerConnectionException()
	{
	}

	public ServerConnectionException(string message)
		: base(message)
	{
	}

	public ServerConnectionException(string message, Exception inner)
		: base(message, inner)
	{
	}

	public ServerConnectionException(string message, ServerConnectionIssue issue, Exception inner)
		: base(message, inner)
	{
		Issue = issue;
	}
}
