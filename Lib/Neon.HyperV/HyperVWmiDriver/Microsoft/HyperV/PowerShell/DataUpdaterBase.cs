using System;
using System.Globalization;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal abstract class DataUpdaterBase<T> : IDataUpdater<T> where T : IVirtualizationManagementObject
{
	protected T m_Value;

	private readonly object m_SyncObject = new object();

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

	public abstract bool IsTemplate { get; }

	public event EventHandler Deleted;

	internal DataUpdaterBase(T dataItem)
	{
		m_Value = dataItem;
	}

	public T GetData(UpdatePolicy policy)
	{
		switch (policy)
		{
		case UpdatePolicy.EnsureUpdated:
		case UpdatePolicy.EnsureAssociatorsUpdated:
			EnsureUpdated(Constants.UpdateThreshold, policy);
			break;
		default:
			throw new ArgumentOutOfRangeException("policy", string.Format(CultureInfo.CurrentCulture, ErrorMessages.ArgumentOutOfRange_InvalidEnumValue, policy.ToString(), typeof(UpdatePolicy).Name));
		case UpdatePolicy.None:
			break;
		}
		return m_Value;
	}

	public TChildType GetDataAs<TChildType>(UpdatePolicy policy) where TChildType : T
	{
		if (m_Value is TChildType)
		{
			return (TChildType)(object)GetData(policy);
		}
		return default(TChildType);
	}

	private void EnsureUpdated(TimeSpan threshold, UpdatePolicy policy)
	{
		lock (m_SyncObject)
		{
			try
			{
				if (policy == UpdatePolicy.EnsureUpdated)
				{
					UpdateProperties(threshold);
				}
				else
				{
					UpdateAssociators(threshold);
				}
			}
			catch (ServerObjectDeletedException)
			{
				IsDeleted = true;
				this.Deleted?.Invoke(this, EventArgs.Empty);
			}
		}
	}

	protected abstract void UpdateProperties(TimeSpan threshold);

	protected abstract void UpdateAssociators(TimeSpan threshold);
}
