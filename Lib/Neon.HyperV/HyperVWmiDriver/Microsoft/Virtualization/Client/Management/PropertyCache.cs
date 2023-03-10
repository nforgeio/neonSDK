#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal class PropertyCache : CacheBase
{
	private readonly Dictionary<string, object> m_Properties;

	internal ICimClass CimClass { get; private set; }

	public PropertyCache(ObjectKey key, ICimInstance cimInstance)
		: base(key)
	{
		m_Properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		if (cimInstance != null)
		{
			Update(cimInstance);
			m_Initialized = true;
			m_LastWmiUpdateTime = DateTime.Now;
		}
	}

	public bool TryGetValue(string propertyName, out object value)
	{
		value = null;
		bool flag = false;
		bool flag2 = false;
		if (propertyName.StartsWith("__", StringComparison.OrdinalIgnoreCase))
		{
			flag = true;
			switch (propertyName)
			{
			case "__CLASS":
				value = m_Key.ClassName;
				break;
			case "__SERVER":
				value = m_Key.Server;
				break;
			case "__Namespace":
				value = m_Key.NamespaceName;
				break;
			case "__PATH":
				value = m_Key.ManagementPath;
				break;
			default:
				flag = false;
				break;
			}
		}
		lock (m_ObjectLock)
		{
			if (!flag)
			{
				flag = m_Properties.TryGetValue(propertyName, out value);
			}
			if (!flag && !m_Initialized)
			{
				flag2 = true;
			}
		}
		if (flag2)
		{
			PerformCacheUpdate(null);
			lock (m_ObjectLock)
			{
				return m_Properties.TryGetValue(propertyName, out value);
			}
		}
		return flag;
	}

	public IEnumerable<string> GetPropertyNames()
	{
		return m_Properties.Keys;
	}

	internal bool Update(ICimInstance cimInstance, WmiOperationOptions options)
	{
		if (options == null || !options.PartialObject)
		{
			return Update(cimInstance);
		}
		if (CimClass == null)
		{
			lock (m_ObjectLock)
			{
				if (CimClass == null)
				{
					CimClass = cimInstance.CimClass;
				}
			}
		}
		KeyValueDictionary keyValueDictionary = new KeyValueDictionary();
		if (options.KeysOnly)
		{
			foreach (CimPropertyDeclaration item in cimInstance.CimClass.CimClassProperties.Where((CimPropertyDeclaration p) => p.Flags.HasFlag(CimFlags.Key)))
			{
				keyValueDictionary.Add(item.Name, cimInstance.CimInstanceProperties[item.Name].Value);
			}
		}
		else
		{
			string[] partialObjectProperties = options.PartialObjectProperties;
			foreach (string text in partialObjectProperties)
			{
				CimProperty cimProperty = cimInstance.CimInstanceProperties[text];
				if (cimProperty != null)
				{
					keyValueDictionary.Add(text, cimProperty.Value);
				}
			}
		}
		return Update(keyValueDictionary);
	}

	public bool Update(ICimInstance cimInstance)
	{
		CimKeyedCollection<CimProperty> cimInstanceProperties = cimInstance.CimInstanceProperties;
		lock (m_ObjectLock)
		{
			if (CimClass == null)
			{
				CimClass = cimInstance.CimClass;
			}
			bool flag = false;
			foreach (CimProperty item in cimInstanceProperties)
			{
				flag |= UpdateSingleProperty(item.Name, item.Value);
			}
			return flag;
		}
	}

	public bool Update(IDictionary<string, object> propertyDictionary)
	{
		lock (m_ObjectLock)
		{
			bool flag = false;
			foreach (KeyValuePair<string, object> item in propertyDictionary)
			{
				flag |= UpdateSingleProperty(item.Key, item.Value);
			}
			return flag;
		}
	}

	protected override bool PerformCacheUpdate(WmiOperationOptions options)
	{
		ICimInstance cimInstance = null;
		try
		{
			cimInstance = m_Key.Server.GetInstance(m_Key.ManagementPath, options?.CimOperationOptions);
		}
		catch (Exception serverException)
		{
			throw ThrowHelper.CreateServerException(m_Key.Server, serverException);
		}
		using (cimInstance)
		{
			VMTrace.TraceWmiGetProperties(cimInstance.CimSystemProperties.ClassName, cimInstance.CimInstanceProperties);
			lock (m_ObjectLock)
			{
				m_LastWmiUpdateTime = DateTime.Now;
				m_Initialized = true;
				return Update(cimInstance, options);
			}
		}
	}

	private bool UpdateSingleProperty(string propertyName, object propertyValue)
	{
		bool flag = false;
		if (!m_Properties.ContainsKey(propertyName))
		{
			flag = true;
		}
		else if (propertyValue != null)
		{
			object obj = m_Properties[propertyName];
			flag = ((obj == null || !(propertyValue is Array)) ? (!propertyValue.Equals(obj)) : IsArrayChanged((Array)propertyValue, (Array)obj));
		}
		else
		{
			flag = m_Properties[propertyName] != null;
		}
		if (flag)
		{
			m_Properties[propertyName] = propertyValue;
		}
		return flag;
	}

	private bool IsArrayChanged(Array newPropertyArray, Array cachedPropertyArray)
	{
		bool flag = newPropertyArray.Length != cachedPropertyArray.Length;
		if (!flag)
		{
			for (int i = 0; i < newPropertyArray.Length; i++)
			{
				object value = newPropertyArray.GetValue(i);
				object value2 = cachedPropertyArray.GetValue(i);
				flag = ((value == null) ? (value2 != null) : (!value.Equals(value2)));
				if (flag)
				{
					break;
				}
			}
		}
		return flag;
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
	private void ObjectInvariant()
	{
	}
}
