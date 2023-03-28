using System;

namespace Microsoft.Virtualization.Client.Management;

internal class ServerCallFailedException : VirtualizationManagementException
{
	private ServerCallFailedReason m_Reason;

	public ServerCallFailedReason FailureReason
	{
		get
		{
			return m_Reason;
		}
		set
		{
			m_Reason = value;
		}
	}

	public ServerCallFailedException()
	{
	}

	public ServerCallFailedException(string message)
		: base(message)
	{
	}

	public ServerCallFailedException(string message, Exception inner)
		: base(message, inner)
	{
	}

	public ServerCallFailedException(string message, ServerCallFailedReason failureReason, Exception inner)
		: base(message, inner)
	{
		FailureReason = failureReason;
	}
}
