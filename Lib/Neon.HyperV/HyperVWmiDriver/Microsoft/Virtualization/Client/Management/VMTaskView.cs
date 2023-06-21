#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using Microsoft.Management.Infrastructure;

namespace Microsoft.Virtualization.Client.Management;

internal class VMTaskView : View, IVMTask, IVirtualizationManagementObject, IDisposable
{
    internal static class WmiMemberNames
    {
        public const string InstanceId = "InstanceID";

        public const string StartTime = "StartTime";

        public const string ScheduledStartTime = "ScheduledStartTime";

        public const string ElapsedTime = "ElapsedTime";

        public const string PercentComplete = "PercentComplete";

        public const string ErrorCode = "ErrorCode";

        public const string ErrorDetailsDescription = "ErrorDescription";

        public const string ErrorSummaryDescription = "ErrorSummaryDescription";

        public const string JobState = "JobState";

        public const string Name = "ElementName";

        public const string Cancelable = "Cancellable";

        public const string JobType = "JobType";

        public const string Cancel = "RequestStateChange";

        public const string GetErrorEx = "GetErrorEx";
    }

    private enum JobState
    {
        New = 2,
        Starting = 3,
        Running = 4,
        Suspended = 5,
        ShuttingDown = 6,
        Completed = 7,
        Terminated = 8,
        Killed = 9,
        Exception = 10,
        CompletedWithWarnings = 32768
    }

    private enum RequestedJobState
    {
        Start = 2,
        Suspend,
        Terminate,
        Kill
    }

    private const RequestedJobState gm_RequestCancelState = RequestedJobState.Terminate;

    private static readonly DateTime gm_MinFileTime = DateTime.FromFileTime(0L);

    private bool m_StopWaitingForCompletedChanged;

    private bool m_Disposed;

    private readonly object m_SyncObject = new object();

    private IDictionary<string, object> m_PutProperties;

    private string m_ClientSideFailedMsg;

    private string m_ErrorDetailsDescription;

    private string m_ErrorSummaryDescription;

    private string m_Name;

    private JobState? m_CachedJobState;

    private ushort m_CachedServerErrorCode;

    protected ManualResetEvent m_WaitHandle;

    protected bool m_IsDeleted;

    protected long m_ClientSetErrorCode;

    private readonly WaitCallback m_UnregisterInstanceModificationEventsCallback;

    public string InstanceId => GetProperty<string>("InstanceID");

    public DateTime? StartTime
    {
        get
        {
            DateTime? dateTime = GetProperty<DateTime>("StartTime");
            if (dateTime == gm_MinFileTime)
            {
                dateTime = null;
            }
            return dateTime;
        }
    }

    public DateTime ScheduledStartTime => GetProperty<DateTime>("ScheduledStartTime");

    public TimeSpan ElapsedTime => GetProperty<TimeSpan>("ElapsedTime");

    public int PercentComplete
    {
        get
        {
            if (!m_IsDeleted && m_ClientSetErrorCode == 0L)
            {
                return NumberConverter.UInt16ToInt32((ushort)(m_CachedJobState.HasValue ? 100 : GetProperty<ushort>("PercentComplete")));
            }
            return 100;
        }
    }

    public bool IsCompleted
    {
        get
        {
            if (!m_IsDeleted && m_ClientSetErrorCode == 0L)
            {
                JobState jobState = (m_CachedJobState.HasValue ? m_CachedJobState.Value : ((JobState)GetProperty<ushort>("JobState")));
                if (jobState != JobState.Completed && jobState != JobState.CompletedWithWarnings && jobState != JobState.Terminated && jobState != JobState.Killed)
                {
                    return jobState == JobState.Exception;
                }
                return true;
            }
            return true;
        }
    }

    public long ErrorCode
    {
        get
        {
            ushort num = (m_CachedJobState.HasValue ? m_CachedServerErrorCode : ServerErrorCode);
            if ((m_IsDeleted || m_ClientSetErrorCode != 0L) && (num == View.ErrorCodeSuccess || num == View.ErrorCodeJob))
            {
                return m_IsDeleted ? (-3) : m_ClientSetErrorCode;
            }
            return NumberConverter.UInt16ToInt64(num);
        }
    }

    public string Name => m_Name;

    public string ErrorDetailsDescription => m_ErrorDetailsDescription;

    public string ErrorSummaryDescription => m_ErrorSummaryDescription;

    public VMTaskStatus Status
    {
        get
        {
            JobState jobState = (m_CachedJobState.HasValue ? m_CachedJobState.Value : ((JobState)GetProperty<ushort>("JobState")));
            VMTaskStatus vMTaskStatus = VMTaskStatus.Running;
            switch (jobState)
            {
            case JobState.New:
            case JobState.Starting:
            case JobState.Running:
            case JobState.Suspended:
            case JobState.ShuttingDown:
                if (!m_IsDeleted && m_ClientSetErrorCode == 0L)
                {
                    return VMTaskStatus.Running;
                }
                return VMTaskStatus.CompletedWithErrors;
            case JobState.Completed:
            case JobState.CompletedWithWarnings:
                return VMTaskStatus.CompletedSuccessfully;
            case JobState.Terminated:
            case JobState.Killed:
                return VMTaskStatus.Canceled;
            case JobState.Exception:
                return VMTaskStatus.CompletedWithErrors;
            default:
                throw ThrowHelper.CreateInvalidPropertyValueException("JobState", typeof(JobState), jobState, null);
            }
        }
    }

    public bool CompletedWithWarnings => (m_CachedJobState.HasValue ? ((int)m_CachedJobState.Value) : ((int)GetProperty<ushort>("JobState"))) == 32768;

    public bool Cancelable => GetProperty<bool>("Cancellable");

    public int JobType
    {
        get
        {
            try
            {
                return NumberConverter.UInt16ToInt32(GetProperty<ushort>("JobType"));
            }
            catch (ClassDefinitionMismatchException)
            {
                return 0;
            }
        }
    }

    public bool IsDeleted => m_IsDeleted;

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

    private ushort ServerErrorCode
    {
        get
        {
            ushort result = 0;
            object property = GetProperty<object>("ErrorCode");
            if (property != null)
            {
                try
                {
                    result = (ushort)property;
                    return result;
                }
                catch (InvalidCastException)
                {
                    return result;
                }
            }
            return result;
        }
    }

    public IEnumerable<IVirtualizationManagementObject> AffectedElements => GetRelatedObjects<IVirtualizationManagementObject>(base.Associations.AffectedJobElement);

    public event EventHandler Completed;

    protected VMTaskView()
    {
        m_UnregisterInstanceModificationEventsCallback = UnregisterForInstanceModificationEventsCallback;
    }

    ~VMTaskView()
    {
        Dispose(disposing: false);
    }

    [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "Invokes DisposeInternal on a background thread which disposes of the remaining parameters")]
    public virtual void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (m_Disposed)
        {
            return;
        }
        lock (m_SyncObject)
        {
            if (!m_StopWaitingForCompletedChanged)
            {
                StopWaitingForCompletedChanged(disposing);
            }
            if (m_WaitHandle != null)
            {
                m_WaitHandle.Dispose();
            }
            m_WaitHandle = null;
            m_Disposed = true;
        }
    }

    private void EmptyHandler(object sender, EventArgs ea)
    {
    }

    protected override void OnCacheUpdated(object sender, EventArgs ea)
    {
        JobState property = (JobState)GetProperty<ushort>("JobState");
        ushort serverErrorCode = ServerErrorCode;
        bool isCompleted = IsCompleted;
        bool hasValue = m_CachedJobState.HasValue;
        bool flag = isCompleted && !hasValue;
        VMTrace.TraceInformation(string.Format(CultureInfo.InvariantCulture, "Updating task '{0}'. Name: {1}, Status: {2}, IsCompleted: {3}, Percent Complete: {4}, ErrorCode: {5}, ErrorSummary: {6}", InstanceId, Name, Status.ToString(), isCompleted.ToString(), PercentComplete.ToString(CultureInfo.InvariantCulture), serverErrorCode.ToString(CultureInfo.InvariantCulture), ErrorSummaryDescription));
        if (flag)
        {
            lock (m_SyncObject)
            {
                if (m_CachedJobState.HasValue)
                {
                    flag = false;
                }
                else
                {
                    m_CachedJobState = property;
                    m_CachedServerErrorCode = serverErrorCode;
                }
            }
            if (flag)
            {
                if ((serverErrorCode != View.ErrorCodeSuccess && serverErrorCode != View.ErrorCodeJob) || property == JobState.CompletedWithWarnings)
                {
                    LoadClientLocalizedStrings(needsName: false);
                }
                if (m_WaitHandle != null)
                {
                    m_WaitHandle.Set();
                }
            }
        }
        lock (m_SyncObject)
        {
            if (flag && !m_StopWaitingForCompletedChanged)
            {
                StopWaitingForCompletedChanged();
            }
        }
        base.OnCacheUpdated(sender, ea);
        if (flag)
        {
            RaiseCompleted();
        }
    }

    protected override void OnProxyDeleted(object sender, EventArgs ea)
    {
        bool flag = false;
        lock (m_SyncObject)
        {
            if (!m_StopWaitingForCompletedChanged)
            {
                m_IsDeleted = true;
                if (m_WaitHandle != null)
                {
                    m_WaitHandle.Set();
                }
                StopWaitingForCompletedChanged();
                flag = true;
            }
        }
        if (flag)
        {
            base.OnProxyDeleted(sender, ea);
            base.OnCacheUpdated(sender, EventArgs.Empty);
            RaiseCompleted();
        }
        else
        {
            base.OnProxyDeleted(sender, ea);
        }
    }

    internal override void Initialize(IProxy proxy, ObjectKey key)
    {
        InitializeInternal(proxy, key, registerWithTaskConnectionTester: true);
    }

    public virtual void InformServerDisconnected(string disconnectedErrorMsg)
    {
        if (IsCompleted)
        {
            return;
        }
        m_ClientSideFailedMsg = disconnectedErrorMsg;
        m_ClientSetErrorCode = -1L;
        try
        {
            if (m_WaitHandle != null)
            {
                m_WaitHandle.Set();
            }
            RaiseCacheUpdated(EventArgs.Empty);
            RaiseCompleted();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    internal void InitializeInternal(IProxy proxy, ObjectKey key, bool registerWithTaskConnectionTester)
    {
        base.Initialize(proxy, key);
        m_WaitHandle = new ManualResetEvent(initialState: false);
        LoadClientLocalizedStrings(needsName: true);
        if (!IsCompleted)
        {
            try
            {
                CacheUpdated += EmptyHandler;
                RegisterForInstanceModificationEvents(InstanceModificationEventStrategy.BulkInstanceModificationEvent);
                Deleted += EmptyHandler;
                UpdatePropertyCache();
            }
            catch (ServerObjectDeletedException)
            {
                VMTrace.TraceWmiEvent(string.Format(CultureInfo.InvariantCulture, "IDE task '{0}' deleted.", InstanceId));
                OnProxyDeleted(proxy, EventArgs.Empty);
            }
            if (registerWithTaskConnectionTester)
            {
                TaskConnectionTester.RegisterTask(base.Server, this);
            }
        }
        else
        {
            m_WaitHandle.Set();
        }
    }

    public void Cancel()
    {
        VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Canceling task '{0}'.", base.ManagementPath));
        uint num = InvokeMethod("RequestStateChange", (ushort)4, null);
        if (num == View.ErrorCodeJob)
        {
            WaitForCompletion();
            if (Status != VMTaskStatus.Canceled)
            {
                throw ThrowHelper.CreateCancelTaskFailedException(ErrorCode, null);
            }
        }
        else if (num != View.ErrorCodeSuccess)
        {
            throw ThrowHelper.CreateCancelTaskFailedException(num, null);
        }
        VMTrace.TraceUserActionCompleted("Cancel task completed successfully.");
    }

    public virtual bool WaitForCompletion()
    {
        return m_WaitHandle.WaitOne();
    }

    public virtual bool WaitForCompletion(TimeSpan timeout)
    {
        return m_WaitHandle.WaitOne(timeout);
    }

    public List<MsvmError> GetErrors()
    {
        List<MsvmError> list = new List<MsvmError>();
        VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Getting Errors from Concrete Job {0}.", base.ManagementPath.ToString()));
        object[] array = new object[1];
        uint num = InvokeMethod("GetErrorEx", array);
        if (num == View.ErrorCodeSuccess)
        {
            if ((string[])array[0] != null)
            {
                string[] array2 = (string[])array[0];
                foreach (string embeddedInstance in array2)
                {
                    try
                    {
                        MsvmError item = EmbeddedInstance.ConvertTo<MsvmError>(base.Server, embeddedInstance);
                        list.Add(item);
                    }
                    catch (Exception ex)
                    {
                        VMTrace.TraceError("Failed to parse Msvm_Error embedded instance.", ex);
                    }
                }
            }
            VMTrace.TraceUserActionCompleted(string.Format(CultureInfo.InvariantCulture, "Succeeded in getting errors from concrete job."));
        }
        else
        {
            VMTrace.TraceError(string.Format(CultureInfo.InvariantCulture, "Getting errors from concrete job failed with code {0}", num));
        }
        return list;
    }

    private void LoadClientLocalizedStrings(bool needsName)
    {
        try
        {
            using ICimInstance cimInstance = base.Server.GetInstance(base.ManagementPath);
            if (needsName)
            {
                m_Name = (string)cimInstance.CimInstanceProperties["ElementName"].Value;
            }
            m_ErrorDetailsDescription = (string)cimInstance.CimInstanceProperties["ErrorDescription"].Value;
            m_ErrorSummaryDescription = (string)cimInstance.CimInstanceProperties["ErrorSummaryDescription"].Value;
        }
        catch (Exception ex)
        {
            if (!(ex is CimException) || ((CimException)ex).NativeErrorCode != NativeErrorCode.NotFound)
            {
                VMTrace.TraceError(string.Format(CultureInfo.InvariantCulture, "Error getting localized properties for the task '{0}' ('{1}').", InstanceId, Name), ex);
            }
            if (needsName)
            {
                m_Name = GetProperty<string>("ElementName");
            }
            m_ErrorDetailsDescription = GetProperty<string>("ErrorDescription");
            m_ErrorSummaryDescription = GetProperty<string>("ErrorSummaryDescription");
        }
    }

    private void StopWaitingForCompletedChanged(bool unregisterAsync = true)
    {
        m_StopWaitingForCompletedChanged = true;
        CacheUpdated -= EmptyHandler;
        Deleted -= EmptyHandler;
        if (!IsCompleted)
        {
            TaskConnectionTester.UnregisterTask(base.Server, this);
        }
        if (!unregisterAsync)
        {
            UnregisterForInstanceModificationEvents();
        }
        else
        {
            ThreadPool.QueueUserWorkItem(m_UnregisterInstanceModificationEventsCallback);
        }
    }

    private void UnregisterForInstanceModificationEventsCallback(object ignored)
    {
        UnregisterForInstanceModificationEvents();
    }

    protected void RaiseCompleted()
    {
        if (this.Completed != null)
        {
            this.Completed(this, EventArgs.Empty);
        }
    }

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
    private void ObjectInvariant()
    {
    }
}
