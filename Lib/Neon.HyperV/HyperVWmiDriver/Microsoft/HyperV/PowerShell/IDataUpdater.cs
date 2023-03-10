using System;

namespace Microsoft.HyperV.PowerShell;

internal interface IDataUpdater<out T>
{
	bool IsDeleted { get; }

	bool IsTemplate { get; }

	event EventHandler Deleted;

	T GetData(UpdatePolicy policy);
}
