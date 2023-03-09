using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("CIM_ConcreteJob")]
internal interface IVMTask : IVirtualizationManagementObject, IDisposable
{
	[Key]
	string InstanceId { get; }

	DateTime? StartTime { get; }

	DateTime ScheduledStartTime { get; }

	[FrequentlyChanging]
	TimeSpan ElapsedTime { get; }

	[FrequentlyChanging]
	int PercentComplete { get; }

	bool IsCompleted { get; }

	long ErrorCode { get; }

	string Name { get; }

	string ErrorDetailsDescription { get; }

	string ErrorSummaryDescription { get; }

	VMTaskStatus Status { get; }

	bool CompletedWithWarnings { get; }

	bool Cancelable { get; }

	int JobType { get; }

	bool IsDeleted { get; }

	IDictionary<string, object> PutProperties { get; set; }

	string ClientSideFailedMessage { get; set; }

	IEnumerable<IVirtualizationManagementObject> AffectedElements { get; }

	event EventHandler Completed;

	void Cancel();

	bool WaitForCompletion();

	bool WaitForCompletion(TimeSpan timeout);

	List<MsvmError> GetErrors();
}
