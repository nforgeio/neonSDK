using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class ProxyRepository : IDisposable
{
	private class ProxyEntry
	{
		private IProxy m_Proxy;

		private WeakReference m_WeakProxy;

		private DateTime m_LastAccess;

		public DateTime LastAccess => m_LastAccess;

		public bool IsStrongReference => m_Proxy != null;

		public bool GarbageCollected
		{
			get
			{
				if (m_WeakProxy != null)
				{
					return m_WeakProxy.Target == null;
				}
				return false;
			}
		}

		public ProxyEntry(IProxy proxy)
		{
			if (proxy == null)
			{
				throw new ArgumentNullException("proxy");
			}
			m_Proxy = proxy;
			m_LastAccess = DateTime.Now;
		}

		public bool AccessProxy(out IProxy proxy)
		{
			m_LastAccess = DateTime.Now;
			IProxy proxy2 = m_Proxy;
			if (proxy2 == null)
			{
				proxy2 = m_WeakProxy.Target as IProxy;
				if (proxy2 != null)
				{
					m_Proxy = proxy2;
					m_WeakProxy = null;
				}
			}
			proxy = proxy2;
			return proxy2 != null;
		}

		public void ConvertToWeakReference()
		{
			if (m_Proxy == null)
			{
				throw new InvalidOperationException("Can not convert to weak reference unless this is a strong reference.");
			}
			m_WeakProxy = new WeakReference(m_Proxy);
			m_Proxy = null;
		}

		public void UpdateGarbageWithNewInstance(IProxy proxy)
		{
			if (!GarbageCollected)
			{
				throw new InvalidOperationException();
			}
			if (proxy == null)
			{
				throw new ArgumentNullException("proxy");
			}
			m_Proxy = proxy;
			m_WeakProxy = null;
			m_LastAccess = DateTime.Now;
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
		private void ObjectInvariant()
		{
		}
	}

	private static readonly TimeSpan gm_LastAccessThreshold = TimeSpan.FromMinutes(15.0);

	private static readonly TimeSpan gm_CleanupThreadFrequency = TimeSpan.FromMinutes(10.0);

	private readonly Dictionary<ObjectKey, ProxyEntry> m_Repository = new Dictionary<ObjectKey, ProxyEntry>(100);

	private readonly Timer m_CleanupTimer;

	public ProxyRepository()
	{
		m_CleanupTimer = new Timer(Cleanup, null, gm_LastAccessThreshold, gm_CleanupThreadFrequency);
	}

	public void Dispose()
	{
		m_CleanupTimer.Dispose();
	}

	public bool RegisterProxy(ref IProxy proxy)
	{
		if (proxy == null)
		{
			throw new ArgumentNullException("proxy");
		}
		IProxy proxy2 = null;
		lock (((ICollection)m_Repository).SyncRoot)
		{
			ProxyEntry value = null;
			if (!m_Repository.TryGetValue(proxy.Key, out value))
			{
				m_Repository.Add(proxy.Key, new ProxyEntry(proxy));
			}
			else if (value.AccessProxy(out proxy2))
			{
				proxy = proxy2;
			}
			else
			{
				value.UpdateGarbageWithNewInstance(proxy);
			}
		}
		return proxy2 == null;
	}

	public bool UnregisterProxy(IProxy proxy)
	{
		if (proxy == null)
		{
			throw new ArgumentNullException("proxy");
		}
		lock (((ICollection)m_Repository).SyncRoot)
		{
			return m_Repository.Remove(proxy.Key);
		}
	}

	internal void Clear()
	{
		lock (((ICollection)m_Repository).SyncRoot)
		{
			m_Repository.Clear();
		}
	}

	public IList<IProxy> GetProxies(Server server, string className)
	{
		lock (((ICollection)m_Repository).SyncRoot)
		{
			List<IProxy> list = new List<IProxy>();
			foreach (ObjectKey key in m_Repository.Keys)
			{
				if (key.Server.Equals(server) && string.Equals(key.ClassName, className, StringComparison.OrdinalIgnoreCase))
				{
					ProxyEntry proxyEntry = m_Repository[key];
					IProxy proxy = null;
					if (proxyEntry.AccessProxy(out proxy))
					{
						list.Add(proxy);
					}
				}
			}
			return list;
		}
	}

	public bool TryGetProxy(ObjectKey key, out IProxy proxy)
	{
		proxy = null;
		lock (((ICollection)m_Repository).SyncRoot)
		{
			ProxyEntry value;
			return m_Repository.TryGetValue(key, out value) && value.AccessProxy(out proxy);
		}
	}

	private void Cleanup(object state)
	{
		List<ObjectKey> list = new List<ObjectKey>();
		lock (((ICollection)m_Repository).SyncRoot)
		{
			DateTime now = DateTime.Now;
			foreach (KeyValuePair<ObjectKey, ProxyEntry> item in m_Repository)
			{
				ProxyEntry value = item.Value;
				if (value.IsStrongReference)
				{
					if (now - value.LastAccess > gm_LastAccessThreshold)
					{
						value.ConvertToWeakReference();
					}
				}
				else if (value.GarbageCollected)
				{
					list.Add(item.Key);
				}
			}
			foreach (ObjectKey item2 in list)
			{
				m_Repository.Remove(item2);
			}
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
	private void ObjectInvariant()
	{
	}
}
