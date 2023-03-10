using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class VirtualizationManagementObjectEqualityComparer<T> : IEqualityComparer<T> where T : IVirtualizationManagementObject
{
	public bool Equals(T first, T second)
	{
		if ((object)first == (object)second)
		{
			return true;
		}
		if (first == null || second == null || first.GetType() != second.GetType())
		{
			return false;
		}
		return object.Equals(first.ManagementPath, second.ManagementPath);
	}

	public int GetHashCode(T obj)
	{
		if (!(obj.ManagementPath != null))
		{
			return 0;
		}
		return obj.ManagementPath.GetHashCode();
	}
}
