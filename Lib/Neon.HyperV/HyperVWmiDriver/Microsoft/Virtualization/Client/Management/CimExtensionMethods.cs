using System.Collections.Generic;
using System.Linq;
using Microsoft.Management.Infrastructure;

namespace Microsoft.Virtualization.Client.Management;

internal static class CimExtensionMethods
{
	internal static ICimSession ToICimSession(this CimSession session)
	{
		if (session != null)
		{
			return new CimSessionWrapper(session);
		}
		return null;
	}

	internal static ICimInstance ToICimInstance(this CimInstance instance)
	{
		if (instance != null)
		{
			return new CimInstanceWrapper(instance);
		}
		return null;
	}

	internal static IEnumerable<ICimInstance> ToICimInstances(this IEnumerable<CimInstance> instances)
	{
		return instances?.Select((CimInstance i) => new CimInstanceWrapper(i));
	}

	internal static ICimClass ToICimClass(this CimClass klass)
	{
		if (klass != null)
		{
			return new CimClassWrapper(klass);
		}
		return null;
	}

	internal static IEnumerable<ICimClass> ToICimClasses(this IEnumerable<CimClass> classes)
	{
		return classes?.Select((CimClass c) => new CimClassWrapper(c));
	}
}
