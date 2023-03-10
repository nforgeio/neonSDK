using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class AssociationCacheManager
{
	private readonly Dictionary<Association, AssociationCache> m_AssociationCacheList = new Dictionary<Association, AssociationCache>();

	public bool TryGetRelatedObjectKeys(Association association, out IEnumerable<ObjectKey> relatedObjectKeys)
	{
		if (association == null)
		{
			throw new ArgumentNullException("association");
		}
		return GetUpdatedCacheData(association, out relatedObjectKeys);
	}

	public void AddCache(Association association, AssociationCache cache)
	{
		lock (m_AssociationCacheList)
		{
			if (!m_AssociationCacheList.ContainsKey(association))
			{
				m_AssociationCacheList.Add(association, cache);
			}
		}
	}

	public void InvalidateAssociations()
	{
		lock (m_AssociationCacheList)
		{
			foreach (KeyValuePair<Association, AssociationCache> associationCache in m_AssociationCacheList)
			{
				if (!associationCache.Key.DoNotUpdate)
				{
					associationCache.Value.InvalidateCache();
				}
			}
		}
	}

	public void UpdateCache(TimeSpan threshold, IReadOnlyList<Association> associationsToExclude)
	{
		List<AssociationCache> list;
		lock (m_AssociationCacheList)
		{
			list = new List<AssociationCache>(m_AssociationCacheList.Count);
			foreach (KeyValuePair<Association, AssociationCache> associationCache in m_AssociationCacheList)
			{
				if (!associationCache.Key.DoNotUpdate && !associationsToExclude.Contains(associationCache.Key))
				{
					list.Add(associationCache.Value);
				}
			}
		}
		foreach (AssociationCache item in list)
		{
			item.Update(threshold);
		}
	}

	public void UpdateCache(Association association, TimeSpan threshold)
	{
		if (!association.DoNotUpdate)
		{
			bool flag;
			AssociationCache value;
			lock (m_AssociationCacheList)
			{
				flag = m_AssociationCacheList.TryGetValue(association, out value);
			}
			if (flag)
			{
				value.Update(threshold);
			}
		}
	}

	private bool GetUpdatedCacheData(Association association, out IEnumerable<ObjectKey> relatedObjects)
	{
		bool flag = false;
		relatedObjects = null;
		AssociationCache value = null;
		lock (m_AssociationCacheList)
		{
			flag = m_AssociationCacheList.TryGetValue(association, out value);
		}
		if (flag)
		{
			relatedObjects = value.RelatedObjectKeys;
		}
		return flag;
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
	private void ObjectInvariant()
	{
	}
}
