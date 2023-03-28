using System;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class DataUpdater<T> : DataUpdaterBase<T> where T : class, IVirtualizationManagementObject
{
	public override bool IsTemplate => false;

	internal DataUpdater(T dataItem)
		: base(dataItem)
	{
	}

	protected override void UpdateAssociators(TimeSpan threshold)
	{
		if (m_Value != null && !base.IsDeleted)
		{
			m_Value.UpdateAssociationCache(threshold);
		}
	}

	protected override void UpdateProperties(TimeSpan threshold)
	{
		if (m_Value != null && !base.IsDeleted)
		{
			m_Value.UpdatePropertyCache(threshold);
		}
	}
}
