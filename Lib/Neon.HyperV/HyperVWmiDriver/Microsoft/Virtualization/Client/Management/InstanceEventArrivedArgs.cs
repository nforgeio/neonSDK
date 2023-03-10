using System;
using Microsoft.Management.Infrastructure;

namespace Microsoft.Virtualization.Client.Management;

internal class InstanceEventArrivedArgs : EventArgs, IDisposable
{
	internal static class WmiPropertyNames
	{
		public const string TargetInstance = "TargetInstance";
	}

	public CimInstance InstanceEvent { get; private set; }

	public ICimInstance TargetInstance => ((CimInstance)InstanceEvent.CimInstanceProperties["TargetInstance"].Value).ToICimInstance();

	public string MachineId { get; private set; }

	internal InstanceEventArrivedArgs(CimSubscriptionResult subscriptionResult)
	{
		InstanceEvent = new CimInstance(subscriptionResult.Instance);
		MachineId = subscriptionResult.MachineId;
	}

	~InstanceEventArrivedArgs()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected void Dispose(bool disposing)
	{
		if (InstanceEvent != null)
		{
			if (disposing)
			{
				InstanceEvent.Dispose();
			}
			InstanceEvent = null;
		}
		MachineId = null;
	}
}
