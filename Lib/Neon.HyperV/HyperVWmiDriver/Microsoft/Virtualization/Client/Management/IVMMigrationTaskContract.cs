using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMMigrationTaskContract : IVMMigrationTask, IVMTask, IVirtualizationManagementObject, IDisposable
{
    public string DestinationHost => null;

    public string VmComputerSystemInstanceId => null;

    public abstract string InstanceId { get; }

    public abstract DateTime? StartTime { get; }

    public abstract DateTime ScheduledStartTime { get; }

    public abstract TimeSpan ElapsedTime { get; }

    public abstract int PercentComplete { get; }

    public abstract bool IsCompleted { get; }

    public abstract long ErrorCode { get; }

    public abstract string Name { get; }

    public abstract string ErrorDetailsDescription { get; }

    public abstract string ErrorSummaryDescription { get; }

    public abstract VMTaskStatus Status { get; }

    public abstract bool CompletedWithWarnings { get; }

    public abstract bool Cancelable { get; }

    public abstract int JobType { get; }

    public abstract bool IsDeleted { get; }

    public abstract IDictionary<string, object> PutProperties { get; set; }

    public abstract string ClientSideFailedMessage { get; set; }

    public abstract IEnumerable<IVirtualizationManagementObject> AffectedElements { get; }

    public abstract Server Server { get; }

    public abstract WmiObjectPath ManagementPath { get; }

    public event EventHandler Completed;

    public event EventHandler Deleted;

    public event EventHandler CacheUpdated;

    public abstract void Cancel();

    public abstract bool WaitForCompletion();

    public abstract bool WaitForCompletion(TimeSpan timeout);

    public abstract List<MsvmError> GetErrors();

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
