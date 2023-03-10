using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class CollectionDataUpdater<T> : CustomDataUpdater<ICollection<T>>
{
	private readonly Func<ICollection<T>> m_RefreshMethod;

	public CollectionDataUpdater(Server server, ICollection<T> initialValue, Func<ICollection<T>> refreshMethod)
		: base(server, initialValue)
	{
		m_RefreshMethod = refreshMethod;
	}

	public override bool TryRefreshValue(out ICollection<T> collection)
	{
		collection = m_RefreshMethod();
		if (collection != null)
		{
			return !collection.Any();
		}
		return false;
	}
}
