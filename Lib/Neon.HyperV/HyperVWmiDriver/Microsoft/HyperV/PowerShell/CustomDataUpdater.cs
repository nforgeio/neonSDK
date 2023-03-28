using System;
using System.Globalization;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal abstract class CustomDataUpdater<T> : IDataUpdater<T>
{
	private T m_Value;

	private readonly Server m_Server;

	private DateTime m_TimeOfLastUpdate;

	private readonly object m_SyncObject = new object();

	private bool m_Initialized;

	private bool m_IsDeleted;

	public bool IsDeleted
	{
		get
		{
			return m_IsDeleted;
		}
		private set
		{
			if (!m_IsDeleted && value)
			{
				this.Deleted?.Invoke(this, EventArgs.Empty);
			}
			m_IsDeleted = value;
		}
	}

	public bool IsTemplate => false;

	public event EventHandler Deleted;

	public CustomDataUpdater(Server server)
	{
		m_Server = server;
	}

	public CustomDataUpdater(Server server, T initialValue)
		: this(server)
	{
		m_Value = initialValue;
		m_Initialized = true;
		m_TimeOfLastUpdate = DateTime.Now;
	}

	public T GetData(UpdatePolicy policy)
	{
		switch (policy)
		{
		case UpdatePolicy.EnsureUpdated:
			EnsureUpdated(Constants.UpdateThreshold);
			break;
		case UpdatePolicy.EnsureAssociatorsUpdated:
			throw new NotImplementedException("The value has no related objects.");
		default:
			throw new ArgumentOutOfRangeException("policy", string.Format(CultureInfo.CurrentCulture, ErrorMessages.ArgumentOutOfRange_InvalidEnumValue, policy.ToString(), typeof(UpdatePolicy).Name));
		case UpdatePolicy.None:
			break;
		}
		return m_Value;
	}

	protected void EnsureUpdated(TimeSpan threshold)
	{
		lock (m_SyncObject)
		{
			if (!m_Initialized || DateTime.Now - m_TimeOfLastUpdate >= threshold || m_TimeOfLastUpdate < m_Server.LastCacheFlushTime)
			{
				if (TryRefreshValue(out var value))
				{
					IsDeleted = true;
				}
				else
				{
					m_Value = value;
				}
				m_Initialized = true;
				m_TimeOfLastUpdate = DateTime.Now;
			}
		}
	}

	public abstract bool TryRefreshValue(out T value);
}
