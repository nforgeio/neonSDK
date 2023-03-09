using System;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class WatchableTask : VirtualizationObject, IDisposable
{
	private readonly DataUpdater<IVMTask> m_Task;

	private readonly string m_DefaultCaption;

	internal string Caption
	{
		get
		{
			string name = m_Task.GetData(UpdatePolicy.EnsureUpdated).Name;
			if (!string.IsNullOrEmpty(name))
			{
				return name;
			}
			return m_DefaultCaption;
		}
	}

	internal int PercentComplete => m_Task.GetData(UpdatePolicy.EnsureUpdated).PercentComplete;

	internal bool IsCompleted => m_Task.GetData(UpdatePolicy.EnsureUpdated).IsCompleted;

	internal bool IsCancelable => m_Task.GetData(UpdatePolicy.EnsureUpdated).Cancelable;

	internal TaskStatus Status => (TaskStatus)m_Task.GetData(UpdatePolicy.EnsureUpdated).Status;

	internal VirtualizationObject TaskOwner { get; set; }

	internal event EventHandler Completed;

	internal event EventHandler Updated;

	internal WatchableTask(IVMTask task, string defaultCaption, VirtualizationObject taskOwner)
		: base(task)
	{
		m_DefaultCaption = defaultCaption;
		task.Completed += OnTaskCompleted;
		task.CacheUpdated += OnTaskUpdated;
		m_Task = InitializePrimaryDataUpdater(task);
		TaskOwner = taskOwner;
	}

	~WatchableTask()
	{
		Dispose(disposing: false);
	}

	internal void Cancel()
	{
		try
		{
			m_Task.GetData(UpdatePolicy.EnsureUpdated).Cancel();
		}
		catch (Exception exception)
		{
			throw ExceptionHelper.ConvertToVirtualizationException(exception, TaskOwner);
		}
	}

	internal bool WaitForCompletion()
	{
		try
		{
			return m_Task.GetData(UpdatePolicy.EnsureUpdated).WaitForCompletion();
		}
		catch (Exception exception)
		{
			throw ExceptionHelper.ConvertToVirtualizationException(exception, TaskOwner);
		}
	}

	internal bool WaitForCompletion(TimeSpan timeout)
	{
		try
		{
			return m_Task.GetData(UpdatePolicy.EnsureUpdated).WaitForCompletion(timeout);
		}
		catch (Exception exception)
		{
			throw ExceptionHelper.ConvertToVirtualizationException(exception, TaskOwner);
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (disposing && m_Task != null)
		{
			m_Task.GetData(UpdatePolicy.None)?.Dispose();
		}
	}

	private void OnTaskCompleted(object sender, EventArgs ea)
	{
		this.Completed?.Invoke(this, ea);
	}

	private void OnTaskUpdated(object sender, EventArgs ea)
	{
		this.Updated?.Invoke(this, ea);
	}

	internal static void MonitorTask(IVMTask task, string taskDescription, IOperationWatcher operationWatcher, VirtualizationObject targetObject)
	{
		using WatchableTask task2 = new WatchableTask(task, taskDescription, targetObject);
		operationWatcher.Watch(task2);
	}
}
