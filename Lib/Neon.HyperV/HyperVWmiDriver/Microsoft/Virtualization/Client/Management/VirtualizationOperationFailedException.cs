using System;

namespace Microsoft.Virtualization.Client.Management;

internal class VirtualizationOperationFailedException : VirtualizationManagementException
{
	private VirtualizationOperation m_Operation;

	private long m_ErrorCode;

	private bool m_OperationCanceled;

	public VirtualizationOperation Operation
	{
		get
		{
			return m_Operation;
		}
		set
		{
			m_Operation = value;
		}
	}

	public long ErrorCode
	{
		get
		{
			return m_ErrorCode;
		}
		set
		{
			m_ErrorCode = value;
		}
	}

	public bool Canceled
	{
		get
		{
			return m_OperationCanceled;
		}
		set
		{
			m_OperationCanceled = value;
		}
	}

	public VirtualizationOperationFailedException()
	{
	}

	public VirtualizationOperationFailedException(string message)
		: base(message)
	{
	}

	public VirtualizationOperationFailedException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
