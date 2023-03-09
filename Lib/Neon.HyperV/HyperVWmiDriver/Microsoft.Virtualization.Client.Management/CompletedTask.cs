using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

internal class CompletedTask : IVMTask, IVirtualizationManagementObject, IDisposable
{
	private readonly DateTime m_StartTime = DateTime.Now;

	private readonly Server m_Server;

	private readonly long m_ErrorCode;

	private readonly string m_ErrorDetailsDescription;

	private readonly string m_ErrorSummaryDescription;

	private readonly IEnumerable<IVirtualizationManagementObject> m_AffectedElements = new IVirtualizationManagementObject[0];

	private readonly Exception m_WrappedException;

	private IDictionary<string, object> m_PutProperties;

	private string m_ClientSideFailedMsg;

	private Guid m_InstanceId;

	string IVMTask.InstanceId => m_InstanceId.ToString();

	public DateTime? StartTime => m_StartTime;

	public DateTime ScheduledStartTime => m_StartTime;

	public TimeSpan ElapsedTime => TimeSpan.Zero;

	public int PercentComplete => 100;

	public bool IsCompleted => true;

	public long ErrorCode => m_ErrorCode;

	public string Name => string.Empty;

	public string ErrorDetailsDescription => m_ErrorDetailsDescription;

	public string ErrorSummaryDescription => m_ErrorSummaryDescription;

	public VMTaskStatus Status
	{
		get
		{
			if (m_ErrorCode == View.ErrorCodeSuccess)
			{
				return VMTaskStatus.CompletedSuccessfully;
			}
			return VMTaskStatus.CompletedWithErrors;
		}
	}

	public bool CompletedWithWarnings => false;

	public bool Cancelable => false;

	public int JobType => 0;

	public Exception WrappedException => m_WrappedException;

	public bool IsDeleted => false;

	[SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
	public IDictionary<string, object> PutProperties
	{
		get
		{
			return m_PutProperties;
		}
		set
		{
			m_PutProperties = value;
		}
	}

	public string ClientSideFailedMessage
	{
		get
		{
			return m_ClientSideFailedMsg;
		}
		set
		{
			m_ClientSideFailedMsg = value;
		}
	}

	public IEnumerable<IVirtualizationManagementObject> AffectedElements => m_AffectedElements;

	public Server Server => m_Server;

	public WmiObjectPath ManagementPath => null;

	event EventHandler IVMTask.Completed
	{
		add
		{
		}
		remove
		{
		}
	}

	event EventHandler IVirtualizationManagementObject.Deleted
	{
		add
		{
		}
		remove
		{
		}
	}

	event EventHandler IVirtualizationManagementObject.CacheUpdated
	{
		add
		{
		}
		remove
		{
		}
	}

	public CompletedTask(Server server)
	{
		m_Server = server;
		m_InstanceId = default(Guid);
	}

	public CompletedTask(Server server, IVirtualizationManagementObject affectedElement)
	{
		m_Server = server;
		m_InstanceId = default(Guid);
		m_AffectedElements = new IVirtualizationManagementObject[1] { affectedElement };
	}

	public CompletedTask(Server server, IEnumerable<IVirtualizationManagementObject> affectedElements)
	{
		m_Server = server;
		if (affectedElements != null)
		{
			m_AffectedElements = affectedElements;
		}
	}

	public CompletedTask(Server server, long errorCode)
	{
		m_Server = server;
		m_ErrorCode = errorCode;
	}

	public CompletedTask(Server server, Exception wrappedException)
	{
		m_Server = server;
		m_WrappedException = wrappedException;
		if (wrappedException != null)
		{
			m_ErrorCode = 32768L;
			if (wrappedException is VirtualizationManagementException ex)
			{
				m_ErrorSummaryDescription = ex.Message;
				m_ErrorDetailsDescription = ex.Description;
			}
			if (wrappedException is VirtualizationOperationFailedException ex2)
			{
				m_ErrorCode = ex2.ErrorCode;
			}
			else if (wrappedException is ServerCallFailedException ex3 && ex3.FailureReason == ServerCallFailedReason.NotSupported)
			{
				m_ErrorCode = 32770L;
			}
		}
	}

	public void InvalidatePropertyCache()
	{
	}

	public void UpdatePropertyCache()
	{
	}

	public void UpdatePropertyCache(TimeSpan threshold)
	{
	}

	public void RegisterForInstanceModificationEvents(InstanceModificationEventStrategy strategy)
	{
		throw new NotSupportedException();
	}

	public void UnregisterForInstanceModificationEvents()
	{
	}

	public void InvalidateAssociationCache()
	{
	}

	public void UpdateAssociationCache()
	{
	}

	public void UpdateAssociationCache(TimeSpan threshold)
	{
	}

	public void DiscardPendingPropertyChanges()
	{
	}

	public string GetEmbeddedInstance()
	{
		throw new NotSupportedException();
	}

	public void Cancel()
	{
		throw ThrowHelper.CreateCancelTaskFailedException(-1L, null);
	}

	public bool WaitForCompletion()
	{
		return true;
	}

	public bool WaitForCompletion(TimeSpan timeout)
	{
		return true;
	}

	public List<MsvmError> GetErrors()
	{
		return new List<MsvmError>(0);
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
	private void ObjectInvariant()
	{
	}
}
