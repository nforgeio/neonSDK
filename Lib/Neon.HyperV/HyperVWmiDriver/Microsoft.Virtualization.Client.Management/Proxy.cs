#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Management.Infrastructure;

namespace Microsoft.Virtualization.Client.Management;

internal class Proxy : IProxy
{
	private readonly ObjectKey m_ObjectKey;

	private readonly PropertyCacheManager m_PropertyCacheManager;

	private readonly AssociationCacheManager m_AssociationCacheManager;

	private EventHandler m_DeletedHandler;

	private string[] m_EventWatchAdditionalConditions;

	private EventObjectKey m_DeletionObjectKey;

	private readonly object m_SyncLock = new object();

	private EventObjectKey DeletionObjectKey => m_DeletionObjectKey ?? (m_DeletionObjectKey = new EventObjectKey(Key, InstanceEventType.InstanceDeletionEvent, EventWatchAdditionalConditions));

	public ObjectKey Key => m_ObjectKey;

	public WmiObjectPath Path => m_ObjectKey.ManagementPath;

	public ICimClass CimClass => m_PropertyCacheManager.CimClass;

	public string[] EventWatchAdditionalConditions
	{
		get
		{
			return m_EventWatchAdditionalConditions;
		}
		set
		{
			m_EventWatchAdditionalConditions = value;
		}
	}

	public event EventHandler PropertyCacheUpdated;

	public event EventHandler Deleted
	{
		add
		{
			bool flag = false;
			lock (m_SyncLock)
			{
				bool num = m_DeletedHandler == null;
				m_DeletedHandler = (EventHandler)Delegate.Combine(m_DeletedHandler, value);
				bool flag2 = m_DeletedHandler == null;
				flag = num && !flag2;
			}
			if (flag)
			{
				InstanceEventManager.GetInstanceEventMonitor(DeletionObjectKey)?.RegisterObject(DeletionObjectKey, DeletedEventArrived);
			}
		}
		remove
		{
			bool flag = false;
			lock (m_SyncLock)
			{
				if (m_DeletedHandler != null && value != null)
				{
					m_DeletedHandler = (EventHandler)Delegate.Remove(m_DeletedHandler, value);
					flag = m_DeletedHandler == null;
				}
			}
			if (flag)
			{
				InstanceEventManager.GetInstanceEventMonitor(DeletionObjectKey)?.UnregisterObject(DeletionObjectKey, DeletedEventArrived);
			}
		}
	}

	public Proxy(ObjectKey key, bool delayInitializePropertyCache, ICimInstance cimInstanceToProxy)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key", "Can not construct a Proxy with null key.");
		}
		m_ObjectKey = key;
		if (cimInstanceToProxy == null && !delayInitializePropertyCache)
		{
			try
			{
				VMTrace.TraceWmiObjectGet(m_ObjectKey.ManagementPath);
				ICimInstance instance = m_ObjectKey.Server.GetInstance(m_ObjectKey.ManagementPath);
				if (!string.Equals(m_ObjectKey.ManagementPath.ClassName, instance.CimSystemProperties.ClassName, StringComparison.OrdinalIgnoreCase))
				{
					WmiObjectPath wmiObjectPath = new WmiObjectPath(key.Server, key.NamespaceName, instance);
					VMTrace.TraceWarning(string.Format(CultureInfo.InvariantCulture, "Got object with different path: {0}", wmiObjectPath.ToString()));
					m_ObjectKey = new ObjectKey(m_ObjectKey.Server, wmiObjectPath);
				}
				cimInstanceToProxy = instance;
			}
			catch (CimException ex)
			{
				if (ex.NativeErrorCode == NativeErrorCode.NotFound)
				{
					throw ThrowHelper.CreateRemoteObjectNotFoundException(m_ObjectKey.Server, m_ObjectKey, ex);
				}
				throw ThrowHelper.CreateServerException(m_ObjectKey.Server, ex);
			}
			catch (Exception serverException)
			{
				throw ThrowHelper.CreateServerException(m_ObjectKey.Server, serverException);
			}
		}
		PropertyCache cache = new PropertyCache(m_ObjectKey, cimInstanceToProxy);
		m_PropertyCacheManager = new PropertyCacheManager(cache, this);
		m_AssociationCacheManager = new AssociationCacheManager();
	}

	private void DeletedEventArrived(object sender, InstanceEventArrivedArgs e)
	{
		VMTrace.TraceWmiEvent("IDE object deleted: " + m_ObjectKey.ManagementPath.ToString());
		if (m_DeletedHandler != null)
		{
			m_DeletedHandler(this, EventArgs.Empty);
		}
	}

	public void NotifyPropertyCacheUpdated()
	{
		if (this.PropertyCacheUpdated != null)
		{
			this.PropertyCacheUpdated(this, EventArgs.Empty);
		}
	}

	public object GetProperty(string propertyName)
	{
		object value = null;
		if (!m_PropertyCacheManager.TryGetValue(propertyName, out value))
		{
			string className = m_ObjectKey.ClassName;
			throw ThrowHelper.CreateClassDefinitionMismatchException(ClassDefinitionMismatchReason.Property, className, propertyName, null);
		}
		return value;
	}

	public IEnumerable<string> GetPropertyNames()
	{
		return m_PropertyCacheManager.GetPropertyNames();
	}

	public bool DoesPropertyExist(string propertyName)
	{
		object value;
		return m_PropertyCacheManager.TryGetValue(propertyName, out value);
	}

	public void PutProperties(IDictionary<string, object> propertyDictionary)
	{
		try
		{
			m_ObjectKey.Server.ModifyInstance(m_ObjectKey.ManagementPath, propertyDictionary);
		}
		catch (Exception serverException)
		{
			throw ThrowHelper.CreateServerException(Key.Server, serverException);
		}
	}

	public bool NeedsUpdate(TimeSpan refreshTime)
	{
		return m_PropertyCacheManager.NeedsUpdate(refreshTime);
	}

	public void InvalidatePropertyCache()
	{
		m_PropertyCacheManager.InvalidateCache();
	}

	public void InvalidateAssociationCache()
	{
		m_AssociationCacheManager.InvalidateAssociations();
	}

	public void UpdatePropertyCache(IDictionary<string, object> propertyDictionary)
	{
		m_PropertyCacheManager.UpdateCache(propertyDictionary);
	}

	public void UpdatePropertyCache(TimeSpan threshold)
	{
		UpdatePropertyCache(threshold, null);
	}

	public void UpdatePropertyCache(TimeSpan threshold, WmiOperationOptions options)
	{
		m_PropertyCacheManager.UpdateCache(threshold, options);
	}

	public void UpdatePropertyCache(ICimInstance cimInstance, WmiOperationOptions options)
	{
		m_PropertyCacheManager.UpdateCache(cimInstance, options);
	}

	public void RegisterForInstanceModificationEvents(InstanceModificationEventStrategy strategy)
	{
		m_PropertyCacheManager.RegisterForInstanceModificationEvents(strategy);
	}

	public void UnregisterForInstanceModificationEvents()
	{
		m_PropertyCacheManager.UnregisterForInstanceModificationEvents();
	}

	public void UpdateAssociationCache(TimeSpan threshold, IReadOnlyList<Association> associationsToExclude)
	{
		m_AssociationCacheManager.UpdateCache(threshold, associationsToExclude);
	}

	public void UpdateOneCachedAssociation(Association association, TimeSpan threshold)
	{
		m_AssociationCacheManager.UpdateCache(association, threshold);
	}

	public TReturnType InvokeMethodWithReturn<TReturnType>(string methodName, object[] args)
	{
		object obj = InvokeMethodInternal(methodName, args);
		TReturnType result = default(TReturnType);
		try
		{
			if (obj != null)
			{
				return (TReturnType)obj;
			}
			return result;
		}
		catch (InvalidCastException inner)
		{
			throw ThrowHelper.CreateInvalidMethodReturnValueException(methodName, typeof(TReturnType), obj, inner);
		}
	}

	public void InvokeMethodWithoutReturn(string methodName, object[] args)
	{
		InvokeMethodInternal(methodName, args);
	}

	public uint InvokeMethod(string methodName, object[] args)
	{
		return InvokeMethodWithReturn<uint>(methodName, args);
	}

	public IEnumerable<ObjectKey> GetRelatedObjectKeys(Association association, WmiOperationOptions options)
	{
		if (!m_AssociationCacheManager.TryGetRelatedObjectKeys(association, out var relatedObjectKeys))
		{
			AssociationCache associationCache = new AssociationCache(m_ObjectKey, association);
			associationCache.Update(TimeSpan.Zero, options);
			m_AssociationCacheManager.AddCache(association, associationCache);
			return associationCache.RelatedObjectKeys;
		}
		return relatedObjectKeys;
	}

	private object InvokeMethodInternal(string methodName, object[] args)
	{
		object obj = null;
		try
		{
			obj = m_ObjectKey.Server.InvokeMethod(m_ObjectKey.ManagementPath, methodName, args);
			return obj;
		}
		catch (CimException ex)
		{
			if (ex.NativeErrorCode == NativeErrorCode.MethodNotFound || ex.NativeErrorCode == NativeErrorCode.MethodNotAvailable || ex.NativeErrorCode == NativeErrorCode.InvalidParameter)
			{
				string className = m_ObjectKey.ClassName;
				throw ThrowHelper.CreateClassDefinitionMismatchException(ClassDefinitionMismatchReason.Method, className, methodName, ex);
			}
			throw ThrowHelper.CreateServerException(m_ObjectKey.Server, ex);
		}
		catch (NullReferenceException inner)
		{
			throw ThrowHelper.CreateInvalidMethodReturnValueException(methodName, typeof(uint), obj, inner);
		}
		catch (Exception serverException)
		{
			throw ThrowHelper.CreateServerException(m_ObjectKey.Server, serverException);
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
	private void ObjectInvariant()
	{
	}
}
