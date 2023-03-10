#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class Association
{
	private bool m_DoNotUpdate;

	public bool DoNotUpdate
	{
		get
		{
			return m_DoNotUpdate;
		}
		set
		{
			m_DoNotUpdate = value;
		}
	}

	public List<ObjectKey> GetRelatedObjects(Server server, WmiObjectPath wmiObjectPath, WmiOperationOptions options)
	{
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		if (wmiObjectPath == null)
		{
			throw new ArgumentNullException("wmiObjectPath");
		}
		List<ObjectKey> list = new List<ObjectKey>();
		try
		{
			foreach (ICimInstance item in GetRelatedObjectsSelf(server, wmiObjectPath, options))
			{
				using (item)
				{
					try
					{
						ObjectKey objectKey = new ObjectKey(server, new WmiObjectPath(server, item.CimSystemProperties.Namespace, item));
						ProxyFactory.Instance.GetProxy(objectKey, delayInitializePropertyCache: true, item, options);
						list.Add(objectKey);
					}
					catch (NoWmiMappingException)
					{
						VMTrace.TraceWarning(string.Format(CultureInfo.InvariantCulture, "The WMI object's name '{0}' can not be mapped to a VirtMan type so ignore it.", item.CimSystemProperties.ClassName));
					}
				}
			}
			return list;
		}
		catch (Exception serverException)
		{
			throw ThrowHelper.CreateServerException(server, serverException);
		}
		finally
		{
			VMTrace.TraceWmiQueryComplete("Association query completed.");
		}
	}

	protected abstract IEnumerable<ICimInstance> GetRelatedObjectsSelf(Server server, WmiObjectPath wmiObjectPath, WmiOperationOptions options);
}
