using System;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.Common;

namespace Microsoft.HyperV.PowerShell.Commands;

internal sealed class VmJob : Job, IOperationWatcher
{
	private readonly VirtualizationCmdletBase _executingCmdlet;

	private readonly object m_SyncObject = new object();

	private WatchableTask m_CurrentTask;

	private ProgressRecord m_CurrentProgress;

	private static VMTaskStatusEnumResourceConverter gm_StatusTypeConverter = new VMTaskStatusEnumResourceConverter();

	public override string StatusMessage
	{
		get
		{
			TaskStatus status = m_CurrentTask.Status;
			return gm_StatusTypeConverter.ConvertToString(null, CultureInfo.CurrentUICulture, status);
		}
	}

	public override bool HasMoreData => true;

	public override string Location
	{
		get
		{
			if (m_CurrentTask != null)
			{
				return m_CurrentTask.ComputerName;
			}
			return string.Empty;
		}
	}

	public VmJob(string command, VirtualizationCmdletBase executingCmdlet)
		: base(command)
	{
		_executingCmdlet = executingCmdlet;
		SetJobState(JobState.NotStarted);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && m_CurrentTask != null)
		{
			m_CurrentTask.Dispose();
		}
		base.Dispose(disposing);
	}

	void IOperationWatcher.WriteError(ErrorRecord error)
	{
		if (base.Error.IsOpen)
		{
			base.Error.Add(error);
		}
		if (m_CurrentProgress == null)
		{
			m_CurrentProgress = new ProgressRecord(base.Progress.Count, CmdletResources.VMJob_DefaultCaption, error.Exception.ToString());
			if (base.Progress.IsOpen)
			{
				base.Progress.Add(m_CurrentProgress);
			}
		}
		SetCurrentProgress(100, EnumValues.TaskStatus_Exception);
	}

	void IOperationWatcher.WriteObject(object output)
	{
		if (base.Output.IsOpen)
		{
			base.Output.Add(new PSObject(output));
		}
	}

	void IOperationWatcher.WriteVerbose(string message)
	{
		if (base.Verbose.IsOpen)
		{
			base.Verbose.Add(new VerboseRecord(message));
		}
	}

	void IOperationWatcher.WriteWarning(string message)
	{
		if (base.Warning.IsOpen)
		{
			base.Warning.Add(new WarningRecord(message));
		}
	}

	bool IOperationWatcher.ShouldProcess(string description)
	{
		return true;
	}

	bool IOperationWatcher.ShouldContinue(string description)
	{
		return true;
	}

	void IOperationWatcher.Watch(WatchableTask task)
	{
		SetCurrentTask(task);
		SetJobState(JobState.Running);
		task.WaitForCompletion();
		ClearCurrentTask();
	}

	public override void StopJob()
	{
		SetJobState(JobState.Stopping);
		WatchableTask currentTask = m_CurrentTask;
		if (currentTask != null && !currentTask.IsCompleted && currentTask.IsCancelable)
		{
			currentTask.Cancel();
			return;
		}
		((IOperationWatcher)this).WriteVerbose(CmdletResources.TaskNotFound);
		SetJobState(JobState.Stopped);
	}

	internal void Complete()
	{
		SetJobState(JobState.Completed);
	}

	private void SetCurrentTask(WatchableTask task)
	{
		lock (m_SyncObject)
		{
			task.Completed += OnTaskUpdatedOrCompleted;
			task.Updated += OnTaskUpdatedOrCompleted;
			m_CurrentTask = task;
		}
	}

	private void ClearCurrentTask()
	{
		lock (m_SyncObject)
		{
			if (m_CurrentTask != null)
			{
				WatchableTask currentTask = m_CurrentTask;
				m_CurrentTask = null;
				currentTask.Updated -= OnTaskUpdatedOrCompleted;
				currentTask.Completed -= OnTaskUpdatedOrCompleted;
				currentTask.Dispose();
			}
		}
	}

	internal void SetCurrentProgress(int percentComplete, string statusDescription)
	{
		if (m_CurrentProgress != null)
		{
			m_CurrentProgress.PercentComplete = percentComplete;
			m_CurrentProgress.StatusDescription = statusDescription;
			m_CurrentProgress.RecordType = ((percentComplete == 100) ? ProgressRecordType.Completed : ProgressRecordType.Processing);
		}
	}

	private void OnTaskUpdatedOrCompleted(object sender, EventArgs ea)
	{
		WatchableTask watchableTask = sender as WatchableTask;
		if (m_CurrentProgress == null)
		{
			m_CurrentProgress = new ProgressRecord(base.Progress.Count, watchableTask.Caption, StatusMessage);
			if (base.Progress.IsOpen)
			{
				base.Progress.Add(m_CurrentProgress);
			}
		}
		SetCurrentProgress(watchableTask.PercentComplete, StatusMessage);
		if (watchableTask.IsCompleted)
		{
			m_CurrentProgress = null;
		}
	}
}
