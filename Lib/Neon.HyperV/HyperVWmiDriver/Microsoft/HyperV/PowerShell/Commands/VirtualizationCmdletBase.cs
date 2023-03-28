#define TRACE
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using System.Threading;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client;

namespace Microsoft.HyperV.PowerShell.Commands;

internal abstract class VirtualizationCmdletBase : PSCmdlet, IServerParameters, IParameterSet, IOperationWatcher
{
	protected bool m_ShouldContinueYesToAll;

	protected bool m_ShouldContinueNoToAll;

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter]
	[ValidateNotNullOrEmpty]
	public virtual CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter]
	[ValidateNotNullOrEmpty]
	public virtual string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public virtual PSCredential[] Credential { get; set; }

	public virtual VirtualMachineParameterType VirtualMachineParameterType
	{
		get
		{
			VirtualMachineParameterType result = VirtualMachineParameterType.None;
			if (this is IVirtualMachineCmdlet)
			{
				if (this is IVmByNameCmdlet && (CurrentParameterSetIs("Name") || IsParameterSpecified("Name")))
				{
					result = VirtualMachineParameterType.Name;
				}
				else if (this is IVmByObjectCmdlet && (CurrentParameterSetIs("VMObject") || IsParameterSpecified("VM")))
				{
					result = VirtualMachineParameterType.VMObject;
				}
				else if (this is IVmBySingularObjectCmdlet && (CurrentParameterSetIs("VM") || IsParameterSpecified("VM")))
				{
					result = VirtualMachineParameterType.SingularVMObject;
				}
				else if (this is IVmByVMNameCmdlet && (CurrentParameterSetIs("VMName") || IsParameterSpecified("VMName")))
				{
					result = VirtualMachineParameterType.VMName;
				}
				else if (this is IVmBySingularVMNameCmdlet && (CurrentParameterSetIs("VMName") || IsParameterSpecified("VMName")))
				{
					result = VirtualMachineParameterType.SingularVMName;
				}
				else if (this is IVmByVMIdCmdlet && (CurrentParameterSetIs("VMId") || IsParameterSpecified("VMId")))
				{
					result = VirtualMachineParameterType.VMId;
				}
			}
			return result;
		}
	}

	private bool AsJobPresent
	{
		get
		{
			if (this is ISupportsAsJob supportsAsJob)
			{
				return supportsAsJob.AsJob.IsPresent;
			}
			return false;
		}
	}

	private bool ForcePresent
	{
		get
		{
			if (this is ISupportsForce supportsForce)
			{
				return supportsForce.Force.IsPresent;
			}
			return false;
		}
	}

	private bool PassthroughPresent
	{
		get
		{
			if (this is ISupportsPassthrough supportsPassthrough)
			{
				return supportsPassthrough.Passthru.IsPresent;
			}
			return false;
		}
	}

    //protected string CurrentFileSystemLocation => base.SessionState.Path.CurrentFileSystemLocation.Path;
    protected string CurrentFileSystemLocation => Environment.CurrentDirectory;

    protected virtual void NormalizeParameters()
	{
	}

	protected virtual void ValidateParameters()
	{
		if (AsJobPresent && PassthroughPresent)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.AsJobAndPassthruBothPresent);
		}
		ParameterValidator.ValidateServerParameters(this);
	}

	private void PerformOperationForJob(object job)
	{
		VmJob vmJob = (VmJob)job;
		PerformOperationWithLogging(vmJob);
		vmJob.Complete();
	}

	private void PerformOperationWithLogging(IOperationWatcher operationWatcher)
	{
		try
		{
			PerformOperation(operationWatcher);
		}
		catch (Exception e)
		{
			ExceptionHelper.DisplayErrorOnException(e, operationWatcher);
		}
	}

	internal abstract void PerformOperation(IOperationWatcher operationWatcher);

	protected override void ProcessRecord()
	{
		VMTrace.TraceUserActionInitiatedWithProperties(string.Format(CultureInfo.InvariantCulture, "Start executing command {0}", base.MyInvocation.InvocationName), base.MyInvocation.BoundParameters);
		base.ProcessRecord();
		Func<Action, bool> func = delegate(Action action)
		{
			try
			{
				action();
			}
			catch (Exception e)
			{
				ExceptionHelper.DisplayErrorOnException(e, this);
				return false;
			}
			return true;
		};
		if (func(NormalizeParameters) && func(ValidateParameters))
		{
			if (!AsJobPresent)
			{
				PerformOperationWithLogging(this);
			}
			else
			{
				VirtualizationCmdletBase virtualizationCmdletBase = (VirtualizationCmdletBase)MemberwiseClone();
				VmJob vmJob = new VmJob(base.MyInvocation.Line, virtualizationCmdletBase);
				base.JobRepository.Add(vmJob);
				WriteObject(vmJob);
				ThreadPool.QueueUserWorkItem(virtualizationCmdletBase.PerformOperationForJob, vmJob);
			}
		}
		VMTrace.TraceUserActionCompleted(string.Format(CultureInfo.InvariantCulture, "Completed executing command {0}", base.MyInvocation.InvocationName));
	}

	protected void ValidateMutuallyExclusiveParameters(string firstParameter, string secondParameter)
	{
		if (IsParameterSpecified(firstParameter) && IsParameterSpecified(secondParameter))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_ParametersAreMutuallyExclusive, firstParameter, secondParameter));
		}
	}

	protected bool CurrentParameterSetIs(string parameterSetName)
	{
		return string.Equals(base.ParameterSetName, parameterSetName, StringComparison.OrdinalIgnoreCase);
	}

	bool IParameterSet.CurrentParameterSetIs(string parameterSetName)
	{
		return CurrentParameterSetIs(parameterSetName);
	}

	protected bool IsParameterSpecified(string parameterName)
	{
		return base.MyInvocation.BoundParameters.ContainsKey(parameterName);
	}

	void IOperationWatcher.Watch(WatchableTask task)
	{
		do
		{
			WriteProgress(GetTaskProgress(task));
		}
		while (!task.WaitForCompletion(Constants.TaskUpdateProgressThreshold));
		WriteProgress(GetTaskProgress(task));
	}

	void IOperationWatcher.WriteVerbose(string message)
	{
		VMTrace.TraceInformation(message);
		WriteVerbose(message);
	}

	void IOperationWatcher.WriteWarning(string message)
	{
		VMTrace.TraceWarning(message);
		WriteWarning(message);
	}

	void IOperationWatcher.WriteError(ErrorRecord record)
	{
		string information = ((record.ErrorDetails == null || string.IsNullOrEmpty(record.ErrorDetails.Message)) ? string.Format(CultureInfo.InvariantCulture, "The error '{0}' was written to the pipeline", record.FullyQualifiedErrorId) : record.ErrorDetails.Message);
		VMTrace.TraceError(information, record.Exception);
		WriteError(record);
	}

	bool IOperationWatcher.ShouldProcess(string description)
	{
		return ShouldProcess(description, string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcessWarning, description), CmdletResources.ConfirmCaption);
	}

	bool IOperationWatcher.ShouldContinue(string description)
	{
		if (m_ShouldContinueNoToAll)
		{
			return false;
		}
		if (ForcePresent || AsJobPresent || m_ShouldContinueYesToAll)
		{
			return true;
		}
		return ShouldContinue(description, CmdletResources.ConfirmCaption, ref m_ShouldContinueYesToAll, ref m_ShouldContinueNoToAll);
	}

	internal static ProgressRecord GetTaskProgress(WatchableTask task)
	{
		string caption = task.Caption;
		int percentComplete = task.PercentComplete;
		bool isCompleted = task.IsCompleted;
		return new ProgressRecord(0, caption, string.Format(CultureInfo.CurrentCulture, TaskDescriptions.PercentCompleteTemplate, percentComplete))
		{
			SecondsRemaining = -1,
			PercentComplete = percentComplete,
			RecordType = (isCompleted ? ProgressRecordType.Completed : ProgressRecordType.Processing)
		};
	}

	void IOperationWatcher.WriteObject(object output)
	{
		WriteObject(output);
	}
}
