#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Microsoft.Virtualization.Client.Management;

internal class VMNetworkTaskWait : IVMNetworkTask, IVMTask, IVirtualizationManagementObject, IDisposable
{
    private readonly Server m_Server;

    private readonly WmiObjectPath m_NetworkJobPath;

    private IVMTask m_ConnectedJob;

    private readonly Timer m_Timer;

    private readonly DateTime m_StartTime = DateTime.Now;

    private bool m_Timedout;

    private bool m_IsDeleted;

    private bool m_UnexpectedTestConnectednessError;

    private string m_ClientSideFailedMessage;

    private readonly ManualResetEvent m_WaitHandle;

    private IDictionary<string, object> m_PutProperties;

    private static readonly TimeSpan gm_TimerInterval = TimeSpan.FromSeconds(5.0);

    private static readonly TimeSpan gm_CommunicationRestoredMaxTimeout = TimeSpan.FromMinutes(15.0);

    public DateTime? StartTime => m_StartTime;

    public DateTime ScheduledStartTime => m_StartTime;

    public TimeSpan ElapsedTime => DateTime.Now - m_StartTime;

    public int PercentComplete
    {
        get
        {
            bool isCompleted = IsCompleted;
            int result = 0;
            if (isCompleted)
            {
                result = 100;
            }
            else if (m_ConnectedJob != null)
            {
                result = m_ConnectedJob.PercentComplete;
            }
            return result;
        }
    }

    public bool IsCompleted
    {
        get
        {
            bool flag = m_IsDeleted || m_Timedout || m_UnexpectedTestConnectednessError;
            if (!flag && m_ConnectedJob != null)
            {
                flag = m_ConnectedJob.IsCompleted;
            }
            return flag;
        }
    }

    public long ErrorCode
    {
        get
        {
            long result = 0L;
            if (m_Timedout)
            {
                result = -4L;
            }
            else if (m_IsDeleted)
            {
                result = -3L;
            }
            else if (m_UnexpectedTestConnectednessError)
            {
                result = -1L;
            }
            else if (m_ConnectedJob != null)
            {
                result = m_ConnectedJob.ErrorCode;
            }
            return result;
        }
    }

    public string Name
    {
        get
        {
            if (m_ConnectedJob == null)
            {
                return string.Empty;
            }
            return m_ConnectedJob.Name;
        }
    }

    public string ErrorDetailsDescription
    {
        get
        {
            if (m_ConnectedJob == null)
            {
                return null;
            }
            return m_ConnectedJob.ErrorDetailsDescription;
        }
    }

    public string ErrorSummaryDescription
    {
        get
        {
            if (m_ConnectedJob == null)
            {
                return null;
            }
            return m_ConnectedJob.ErrorSummaryDescription;
        }
    }

    public VMTaskStatus Status
    {
        get
        {
            VMTaskStatus vMTaskStatus = ((m_Timedout || m_IsDeleted || m_UnexpectedTestConnectednessError) ? VMTaskStatus.CompletedWithErrors : VMTaskStatus.Running);
            if (vMTaskStatus == VMTaskStatus.Running && m_ConnectedJob != null)
            {
                vMTaskStatus = m_ConnectedJob.Status;
            }
            return vMTaskStatus;
        }
    }

    public bool CompletedWithWarnings
    {
        get
        {
            if (m_ConnectedJob == null)
            {
                return false;
            }
            return m_ConnectedJob.CompletedWithWarnings;
        }
    }

    public bool Cancelable
    {
        get
        {
            if (m_ConnectedJob == null)
            {
                return false;
            }
            return m_ConnectedJob.Cancelable;
        }
    }

    public int JobType => 0;

    public bool IsDeleted
    {
        get
        {
            bool isDeleted = m_IsDeleted;
            if (!isDeleted && m_ConnectedJob != null)
            {
                isDeleted = m_ConnectedJob.IsDeleted;
            }
            return isDeleted;
        }
    }

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
            return m_ClientSideFailedMessage;
        }
        set
        {
            m_ClientSideFailedMessage = value;
        }
    }

    public string InstanceId => (string)m_NetworkJobPath.KeyValues["InstanceID"];

    public IEnumerable<IVirtualizationManagementObject> AffectedElements
    {
        get
        {
            if (m_ConnectedJob == null)
            {
                return new List<IVirtualizationManagementObject>();
            }
            return m_ConnectedJob.AffectedElements;
        }
    }

    public Server Server => m_Server;

    public WmiObjectPath ManagementPath => m_NetworkJobPath;

    public event EventHandler Completed;

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

    public VMNetworkTaskWait(Server server, WmiObjectPath networkJobPath)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        if (networkJobPath == null)
        {
            throw new ArgumentNullException("networkJobPath");
        }
        m_Server = server;
        m_NetworkJobPath = networkJobPath;
        m_WaitHandle = new ManualResetEvent(initialState: false);
        m_Timer = new Timer(TestConnectedness, null, gm_TimerInterval, TimeSpan.FromMilliseconds(-1.0));
    }

    public VMNetworkTaskWait(Server server, IVMTask networkTask)
        : this(server, networkTask.ManagementPath)
    {
        if (networkTask == null)
        {
            throw new ArgumentNullException("networkTask");
        }
        m_ConnectedJob = networkTask;
        m_ConnectedJob.CacheUpdated += OnCacheUpdated;
        m_ConnectedJob.Deleted += OnDeleted;
        m_ConnectedJob.Completed += OnCompleted;
    }

    private void OnCacheUpdated(object sender, EventArgs ea)
    {
        if (this.CacheUpdated != null)
        {
            this.CacheUpdated(this, ea);
        }
    }

    private void OnCompleted(object sender, EventArgs ea)
    {
        SignalWaitHandle();
        if (this.Completed != null)
        {
            this.Completed(this, ea);
        }
    }

    private void OnDeleted(object sender, EventArgs ea)
    {
        SignalWaitHandle();
        if (this.Deleted != null)
        {
            this.Deleted(this, ea);
        }
    }

    public void Cancel()
    {
        if (m_ConnectedJob != null)
        {
            m_ConnectedJob.Cancel();
            return;
        }
        throw ThrowHelper.CreateCancelTaskFailedException(-1L, null);
    }

    public bool WaitForCompletion(TimeSpan timeout)
    {
        return WaitForCompletionInternal(useTimeout: true, timeout);
    }

    public bool WaitForCompletion()
    {
        return WaitForCompletionInternal(useTimeout: false, TimeSpan.Zero);
    }

    public List<MsvmError> GetErrors()
    {
        if (m_ConnectedJob == null)
        {
            return new List<MsvmError>(0);
        }
        return m_ConnectedJob.GetErrors();
    }

    public void InvalidatePropertyCache()
    {
        if (m_ConnectedJob != null)
        {
            m_ConnectedJob.InvalidatePropertyCache();
        }
    }

    public void UpdatePropertyCache()
    {
        if (m_ConnectedJob != null)
        {
            m_ConnectedJob.UpdatePropertyCache();
        }
    }

    public void UpdatePropertyCache(TimeSpan threshold)
    {
        if (m_ConnectedJob != null)
        {
            m_ConnectedJob.UpdatePropertyCache(threshold);
        }
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
        if (m_ConnectedJob != null)
        {
            m_ConnectedJob.InvalidateAssociationCache();
        }
    }

    public void UpdateAssociationCache()
    {
        if (m_ConnectedJob != null)
        {
            m_ConnectedJob.UpdateAssociationCache();
        }
    }

    public void UpdateAssociationCache(TimeSpan threshold)
    {
        if (m_ConnectedJob != null)
        {
            m_ConnectedJob.UpdateAssociationCache(threshold);
        }
    }

    public string GetEmbeddedInstance()
    {
        throw new NotSupportedException();
    }

    public void DiscardPendingPropertyChanges()
    {
    }

    public void Dispose()
    {
        m_Timer.Dispose();
        m_WaitHandle.Dispose();
        if (m_ConnectedJob != null)
        {
            m_ConnectedJob.Dispose();
        }
        GC.SuppressFinalize(this);
    }

    private void TestConnectedness(object state)
    {
        try
        {
            if (m_ConnectedJob == null)
            {
                ObjectKey key = new ObjectKey(m_Server, m_NetworkJobPath);
                IProxy proxy = ProxyFactory.Instance.GetProxy(key);
                VMNetworkTaskView vMNetworkTaskView = new VMNetworkTaskView();
                vMNetworkTaskView.Initialize(proxy, key);
                m_ConnectedJob = vMNetworkTaskView;
                m_ConnectedJob.CacheUpdated += OnCacheUpdated;
                m_ConnectedJob.Deleted += OnDeleted;
                m_ConnectedJob.Completed += OnCompleted;
            }
            m_ConnectedJob.UpdatePropertyCache(gm_TimerInterval);
        }
        catch (ServerConnectionException ex)
        {
            if (ex.Issue == ServerConnectionIssue.RpcServerUnavailable)
            {
                VMTrace.TraceInformation("Communication not restored. Keep polling.");
                if (DateTime.Now - m_StartTime > gm_CommunicationRestoredMaxTimeout)
                {
                    VMTrace.TraceWarning("Timeout waiting to restore communication. Fail the task.");
                    m_Timedout = true;
                }
            }
            else
            {
                VMTrace.TraceError("Unexpected error while polling server for network task!", ex);
                m_UnexpectedTestConnectednessError = true;
            }
        }
        catch (ServerCallFailedException ex2)
        {
            if (ex2.FailureReason == ServerCallFailedReason.RpcCallFailed)
            {
                VMTrace.TraceInformation("Communication not restored. Keep polling.");
                if (DateTime.Now - m_StartTime > gm_CommunicationRestoredMaxTimeout)
                {
                    VMTrace.TraceWarning("Timeout waiting to restore communication. Fail the task.");
                    m_Timedout = true;
                }
            }
            else
            {
                VMTrace.TraceError("Unexpected error while polling server for network task!", ex2);
                m_UnexpectedTestConnectednessError = true;
            }
        }
        catch (ObjectNotFoundException)
        {
            VMTrace.TraceWarning("Communication restored to server, but network job object is not found, or no longer exists.");
            m_IsDeleted = true;
        }
        catch (Exception ex4)
        {
            VMTrace.TraceError("Unexpected error while polling server for network task!", ex4);
            m_UnexpectedTestConnectednessError = true;
        }
        if (IsCompleted)
        {
            SignalWaitHandle();
            if (this.CacheUpdated != null)
            {
                this.CacheUpdated(this, EventArgs.Empty);
            }
            if (this.Completed != null)
            {
                this.Completed(this, EventArgs.Empty);
            }
            if (IsDeleted && this.Deleted != null)
            {
                this.Deleted(this, EventArgs.Empty);
            }
            return;
        }
        try
        {
            m_Timer.Change(gm_TimerInterval, TimeSpan.FromMilliseconds(-1.0));
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private bool WaitForCompletionInternal(bool useTimeout, TimeSpan timeout)
    {
        if (!useTimeout)
        {
            return m_WaitHandle.WaitOne();
        }
        return m_WaitHandle.WaitOne(timeout);
    }

    private void SignalWaitHandle()
    {
        try
        {
            m_Timer.Dispose();
            m_WaitHandle.Set();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
    private void ObjectInvariant()
    {
    }
}
