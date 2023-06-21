using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMTaskContract : IVMTask, IVirtualizationManagementObject, IDisposable
{
    public string InstanceId => null;

    public DateTime? StartTime => null;

    public DateTime ScheduledStartTime => default(DateTime);

    public TimeSpan ElapsedTime => default(TimeSpan);

    public int PercentComplete => 0;

    public bool IsCompleted => false;

    public long ErrorCode => 0L;

    public string Name => null;

    public string ErrorDetailsDescription => null;

    public string ErrorSummaryDescription => null;

    public VMTaskStatus Status => VMTaskStatus.Running;

    public bool CompletedWithWarnings => false;

    public bool Cancelable => false;

    public int JobType => 0;

    public bool IsDeleted => false;

    public IDictionary<string, object> PutProperties
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public string ClientSideFailedMessage
    {
        get
        {
            return null;
        }
        set
        {
        }
    }

    public IEnumerable<IVirtualizationManagementObject> AffectedElements => null;

    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Completed;

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

    public void Cancel()
    {
    }

    public bool WaitForCompletion()
    {
        return false;
    }

    public bool WaitForCompletion(TimeSpan timeout)
    {
        return false;
    }

    public List<MsvmError> GetErrors()
    {
        return null;
    }

    public abstract void InvalidatePropertyCache();

    public abstract void UpdatePropertyCache();

    public abstract void UpdatePropertyCache(TimeSpan threshold);

    public abstract void RegisterForInstanceModificationEvents(InstanceModificationEventStrategy strategy);

    public abstract void UnregisterForInstanceModificationEvents();

    public abstract void InvalidateAssociationCache();

    public abstract void UpdateAssociationCache();

    public abstract void UpdateAssociationCache(TimeSpan threshold);

    public abstract string GetEmbeddedInstance();

    public abstract void DiscardPendingPropertyChanges();

    public abstract void Dispose();
}
