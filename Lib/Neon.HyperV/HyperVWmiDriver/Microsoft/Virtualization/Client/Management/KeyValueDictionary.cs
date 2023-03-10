using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class KeyValueDictionary : SortedDictionary<string, object>, IReadOnlyDictionary<string, object>, IReadOnlyCollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
{
	public new IEnumerable<string> Keys => base.Keys.AsEnumerable();

	public new IEnumerable<object> Values => base.Values.AsEnumerable();

	public KeyValueDictionary()
		: base((IComparer<string>)StringComparer.OrdinalIgnoreCase)
	{
	}

	public KeyValueDictionary(IDictionary<string, object> other)
		: base(other, (IComparer<string>)StringComparer.OrdinalIgnoreCase)
	{
	}

	public KeyValueDictionary(IEnumerable<KeyValuePair<string, object>> other)
		: base((IComparer<string>)StringComparer.OrdinalIgnoreCase)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		foreach (KeyValuePair<string, object> item in other)
		{
			Add(item.Key, item.Value);
		}
	}
}
