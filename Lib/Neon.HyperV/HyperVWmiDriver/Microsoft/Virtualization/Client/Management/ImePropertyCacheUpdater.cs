#define TRACE
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal class ImePropertyCacheUpdater : IObserver<CimSubscriptionResult>
{
    private static readonly TimeSpan gm_ImeEventInterval = TimeSpan.FromSeconds(2.0);

    private readonly PropertyCacheManager m_PropertyCacheManager;

    private readonly Proxy m_Proxy;

    private string m_EventQuery;

    private CimAsyncMultipleResults<CimSubscriptionResult> m_Subscription;

    private IDisposable m_SubscriptionLifeTime;

    private InstanceModificationEventStrategy m_Strategy;

    private EventObjectKey m_ModificationEventObjectKey;

    private int m_ImeRegistrationCount;

    private EventObjectKey ModificationEventObjectKey => m_ModificationEventObjectKey ?? (m_ModificationEventObjectKey = new EventObjectKey(m_Proxy.Key, InstanceEventType.InstanceModificationEvent, m_Proxy.EventWatchAdditionalConditions));

    public ImePropertyCacheUpdater(Proxy proxy, PropertyCacheManager propertyCacheManager)
    {
        if (proxy == null)
        {
            throw new ArgumentNullException("proxy");
        }
        if (propertyCacheManager == null)
        {
            throw new ArgumentNullException("propertyCacheManager");
        }
        m_Proxy = proxy;
        m_PropertyCacheManager = propertyCacheManager;
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
            CimInstance instance = (CimInstance)subscriptionResult.Instance.CimInstanceProperties["TargetInstance"].Value;
            UpdateCache(instance.ToICimInstance());
        }
        catch (Exception ex)
        {
            VMTrace.TraceError("Error handling WMI event arrived!", ex);
        }
    }

    private void ImeEventArrived(object sender, InstanceEventArrivedArgs e)
    {
        CimInstance instance = (CimInstance)e.InstanceEvent.CimInstanceProperties["TargetInstance"].Value;
        UpdateCache(instance.ToICimInstance());
    }

    private void UpdateCache(ICimInstance updatedInstance)
    {
        VMTrace.TraceWmiEventModifiedProperties("IME object modified: " + m_Proxy.Key.ManagementPath.ToString(), updatedInstance.CimInstanceProperties);
        m_PropertyCacheManager.UpdateCache(updatedInstance);
    }

    public void RegisterForInstanceModificationEvents(InstanceModificationEventStrategy strategy)
    {
        if (strategy != InstanceModificationEventStrategy.InstanceModificationEvent && strategy != InstanceModificationEventStrategy.BulkInstanceModificationEvent)
        {
            throw new ArgumentOutOfRangeException("strategy");
        }
        CimAsyncMultipleResults<CimSubscriptionResult> cimAsyncMultipleResults = null;
        EventObjectKey modificationEventObjectKey = ModificationEventObjectKey;
        int num;
        lock (this)
        {
            num = ++m_ImeRegistrationCount;
            if (num == 1)
            {
                m_Strategy = strategy;
                if (strategy == InstanceModificationEventStrategy.InstanceModificationEvent)
                {
                    m_EventQuery = modificationEventObjectKey.GetInstanceEventQuery(gm_ImeEventInterval, classQuery: false);
                    m_Subscription = modificationEventObjectKey.Server.SubscribeAsync(modificationEventObjectKey.NamespaceName, m_EventQuery);
                    cimAsyncMultipleResults = m_Subscription;
                }
            }
        }
        if (m_Strategy == InstanceModificationEventStrategy.BulkInstanceModificationEvent)
        {
            InstanceEventBulkMonitor instanceEventMonitor = InstanceEventManager.GetInstanceEventMonitor(modificationEventObjectKey);
            if (instanceEventMonitor != null)
            {
                instanceEventMonitor.TimeInterval = gm_ImeEventInterval;
                instanceEventMonitor.RegisterObject(modificationEventObjectKey, ImeEventArrived);
            }
        }
        else if (num == 1)
        {
            VMTrace.TraceWmiWatcher(string.Format(CultureInfo.InvariantCulture, "Starting WMI event subscription with query '{0}'.", m_EventQuery));
            IDisposable subscriptionLifeTime = cimAsyncMultipleResults.Subscribe(this);
            VMTrace.TraceWmiWatcher("WMI event subscription started.");
            lock (this)
            {
                m_SubscriptionLifeTime = subscriptionLifeTime;
            }
        }
    }

    public void UnregisterForInstanceModificationEvents()
    {
        IDisposable disposable = null;
        bool flag = false;
        lock (this)
        {
            if (m_ImeRegistrationCount > 0 && --m_ImeRegistrationCount == 0)
            {
                if (m_Strategy == InstanceModificationEventStrategy.InstanceModificationEvent)
                {
                    disposable = m_SubscriptionLifeTime;
                    m_SubscriptionLifeTime = null;
                    m_Subscription = null;
                }
                else
                {
                    flag = true;
                }
                m_Strategy = InstanceModificationEventStrategy.None;
            }
        }
        if (disposable != null)
        {
            VMTrace.TraceWmiWatcher(string.Format(CultureInfo.InvariantCulture, "Stopping WMI event subscription with query '{0}'.", m_EventQuery));
            disposable.Dispose();
            VMTrace.TraceWmiWatcher("WMI event subscription stopped.");
        }
        else if (flag)
        {
            InstanceEventManager.GetInstanceEventMonitor(ModificationEventObjectKey)?.UnregisterObject(ModificationEventObjectKey, ImeEventArrived);
        }
    }

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
    private void ObjectInvariant()
    {
    }
}
