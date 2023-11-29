using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class CacheBase
{
    protected DateTime m_LastWmiUpdateTime;

    protected readonly ObjectKey m_Key;

    protected bool m_Initialized;

    protected readonly object m_ObjectLock = new object();

    protected CacheBase(ObjectKey key)
    {
        if (key == null)
        {
            throw new ArgumentNullException("key");
        }
        m_Key = key;
    }

    public bool NeedsUpdate(TimeSpan refreshTime)
    {
        lock (m_ObjectLock)
        {
            TimeSpan timeSpan = DateTime.Now - m_LastWmiUpdateTime;
            if (!m_Initialized || timeSpan >= refreshTime || timeSpan < TimeSpan.Zero || m_LastWmiUpdateTime < m_Key.Server.LastCacheFlushTime)
            {
                return true;
            }
        }
        return false;
    }

    public void InvalidateCache()
    {
        lock (m_ObjectLock)
        {
            m_LastWmiUpdateTime = DateTime.MinValue;
        }
    }

    public bool Update(TimeSpan refreshTime)
    {
        return Update(refreshTime, null);
    }

    public bool Update(TimeSpan refreshTime, WmiOperationOptions options)
    {
        bool flag = NeedsUpdate(refreshTime);
        if (flag)
        {
            flag = PerformCacheUpdate(options);
        }
        return flag;
    }

    protected abstract bool PerformCacheUpdate(WmiOperationOptions options);
}
