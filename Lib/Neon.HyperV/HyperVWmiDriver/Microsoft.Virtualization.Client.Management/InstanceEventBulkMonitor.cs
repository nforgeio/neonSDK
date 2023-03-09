#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal class InstanceEventBulkMonitor : IObserver<CimSubscriptionResult>
{
	private readonly object m_SyncRoot = new object();

	private readonly Dictionary<WmiObjectPath, List<InstanceEventArrivedHandler>> m_LookupTable = new Dictionary<WmiObjectPath, List<InstanceEventArrivedHandler>>();

	private string m_EventQuery;

	private CimAsyncMultipleResults<CimSubscriptionResult> m_Subscription;

	private IDisposable m_SubscriptionLifeTime;

	private TimeSpan m_Interval = TimeSpan.FromSeconds(2.0);

	private readonly EventObjectKey m_Key;

	private Dictionary<WmiObjectPath, InstanceEventArrivedArgs> m_MissedEvents = new Dictionary<WmiObjectPath, InstanceEventArrivedArgs>();

	private Dictionary<WmiObjectPath, InstanceEventArrivedArgs> m_OldMissedEvents = new Dictionary<WmiObjectPath, InstanceEventArrivedArgs>();

	private DateTime m_MissedEventsLastCleanup = DateTime.Now;

	private static readonly TimeSpan MISSED_EVENT_CLEANUP_INTERVAL = TimeSpan.FromSeconds(5.0);

	[SuppressMessage("Microsoft.Performance", "CA1811")]
	public TimeSpan TimeInterval
	{
		get
		{
			return m_Interval;
		}
		set
		{
			m_Interval = value;
		}
	}

	internal InstanceEventBulkMonitor(EventObjectKey key)
	{
		m_Key = key;
	}

	public void OnCompleted()
	{
		VMTrace.TraceInformation(string.Format(CultureInfo.InvariantCulture, "{0}::OnCompleted called for event notification query '{1}'.", GetType().Name, m_EventQuery));
	}

	public void OnError(Exception error)
	{
		VMTrace.TraceError(string.Format(CultureInfo.InvariantCulture, "{0}::OnError called for event notification '{1}' with exception {2}.", GetType().Name, m_EventQuery, error.GetType().Name), error);
	}

	public void OnNext(CimSubscriptionResult subscriptionResult)
	{
		try
		{
			CimInstance cimInstance = (CimInstance)subscriptionResult.Instance.CimInstanceProperties["TargetInstance"].Value;
			WmiObjectPath key;
			if (!m_Key.ManagementPath.IsInstance)
			{
				key = m_Key.ManagementPath;
			}
			else
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
				foreach (string key2 in m_Key.ManagementPath.KeyValues.Keys)
				{
					object value = cimInstance.CimInstanceProperties[key2].Value;
					dictionary.Add(key2, value);
				}
				key = new WmiObjectPath(m_Key.Server, m_Key.NamespaceName, m_Key.ClassName, dictionary);
			}
			List<InstanceEventArrivedHandler> value2;
			lock (m_SyncRoot)
			{
				CleanupOldMissedEvents();
				if (m_LookupTable.TryGetValue(key, out value2))
				{
					if (value2 != null)
					{
						value2 = new List<InstanceEventArrivedHandler>(value2);
					}
				}
				else
				{
					m_MissedEvents[key] = new InstanceEventArrivedArgs(subscriptionResult);
				}
			}
			if (value2 == null)
			{
				return;
			}
			using InstanceEventArrivedArgs eventArgs = new InstanceEventArrivedArgs(subscriptionResult);
			foreach (InstanceEventArrivedHandler item in value2)
			{
				try
				{
					item(this, eventArgs);
				}
				catch (Exception ex)
				{
					VMTrace.TraceWarning("Exception calling WMI arrived event handler.", ex);
				}
			}
		}
		catch (Exception ex2)
		{
			VMTrace.TraceError("Error handling WMI event arrived!", ex2);
		}
	}

	internal void RegisterObject(EventObjectKey key, InstanceEventArrivedHandler handler)
	{
		CimAsyncMultipleResults<CimSubscriptionResult> cimAsyncMultipleResults = null;
		lock (m_SyncRoot)
		{
			if (m_LookupTable.TryGetValue(key.ManagementPath, out var value))
			{
				if (!value.Contains(handler))
				{
					value.Add(handler);
				}
			}
			else
			{
				value = new List<InstanceEventArrivedHandler>();
				value.Add(handler);
				m_LookupTable.Add(key.ManagementPath, value);
				if (m_Subscription == null)
				{
					m_EventQuery = key.GetInstanceEventQuery(m_Interval, classQuery: true);
					m_Subscription = key.Server.SubscribeAsync(key.NamespaceName, m_EventQuery);
					cimAsyncMultipleResults = m_Subscription;
				}
			}
		}
		if (cimAsyncMultipleResults != null)
		{
			try
			{
				VMTrace.TraceWmiWatcher(string.Format(CultureInfo.InvariantCulture, "Starting WMI event subscription with query '{0}'.", m_EventQuery));
				IDisposable subscriptionLifeTime = cimAsyncMultipleResults.Subscribe(this);
				VMTrace.TraceWmiWatcher("WMI event subscription started.");
				lock (m_SyncRoot)
				{
					m_SubscriptionLifeTime = subscriptionLifeTime;
				}
			}
			catch (Exception serverException)
			{
				lock (m_SyncRoot)
				{
					m_Subscription = null;
				}
				throw ThrowHelper.CreateServerException(key.Server, callback: true, serverException);
			}
		}
		HandleMissedEvents(key, handler);
	}

	internal void UnregisterObject(EventObjectKey key, InstanceEventArrivedHandler handler)
	{
		IDisposable disposable = null;
		lock (m_SyncRoot)
		{
			List<InstanceEventArrivedHandler> value = null;
			if (m_LookupTable.TryGetValue(key.ManagementPath, out value))
			{
				value.Remove(handler);
				if (value.Count == 0)
				{
					m_LookupTable.Remove(key.ManagementPath);
				}
				if (m_LookupTable.Count == 0)
				{
					disposable = m_SubscriptionLifeTime;
					m_SubscriptionLifeTime = null;
					m_Subscription = null;
				}
			}
		}
		if (disposable != null)
		{
			VMTrace.TraceWmiWatcher(string.Format(CultureInfo.InvariantCulture, "Stopping WMI event subscription with query '{0}'.", m_EventQuery));
			disposable.Dispose();
			VMTrace.TraceWmiWatcher("WMI event subscription stopped.");
		}
	}

	public void TearDown()
	{
		IDisposable disposable = null;
		lock (m_SyncRoot)
		{
			disposable = m_SubscriptionLifeTime;
			m_SubscriptionLifeTime = null;
			m_Subscription = null;
		}
		disposable?.Dispose();
	}

	private void HandleMissedEvents(EventObjectKey key, InstanceEventArrivedHandler handler)
	{
		InstanceEventArrivedArgs value;
		lock (m_SyncRoot)
		{
			if (m_MissedEvents.TryGetValue(key.ManagementPath, out value))
			{
				m_MissedEvents.Remove(key.ManagementPath);
				m_OldMissedEvents.Remove(key.ManagementPath);
			}
			else if (m_OldMissedEvents.TryGetValue(key.ManagementPath, out value))
			{
				m_OldMissedEvents.Remove(key.ManagementPath);
			}
		}
		try
		{
			if (value != null)
			{
				using (value)
				{
					handler(this, value);
					return;
				}
			}
		}
		catch (Exception ex)
		{
			VMTrace.TraceWarning("Exception calling WMI arrived event handler for missed event.", ex);
		}
	}

	private void CleanupOldMissedEvents()
	{
		if (DateTime.Now - m_MissedEventsLastCleanup > MISSED_EVENT_CLEANUP_INTERVAL)
		{
			m_MissedEventsLastCleanup = DateTime.Now;
			if (m_MissedEvents.Count > 0 || m_OldMissedEvents.Count > 0)
			{
				m_OldMissedEvents = m_MissedEvents;
				m_MissedEvents = new Dictionary<WmiObjectPath, InstanceEventArrivedArgs>();
			}
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
	private void ObjectInvariant()
	{
	}
}
