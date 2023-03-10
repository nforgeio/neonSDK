using System;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class TemplateObjectDataUpdater<T> : DataUpdaterBase<T> where T : class, IVirtualizationManagementObject
{
	public override bool IsTemplate => true;

	internal TemplateObjectDataUpdater(T dataItem)
		: base(dataItem)
	{
	}

	protected override void UpdateAssociators(TimeSpan threshold)
	{
	}

	protected override void UpdateProperties(TimeSpan threshold)
	{
	}
}
