#define TRACE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class View : IVirtualizationManagementObject
{
	internal static class WmiCommonMemberNames
	{
		public const string Name = "Name";

		public const string InstanceId = "InstanceID";

		public const string State = "State";

		public const string FriendlyName = "ElementName";

		public const string Antecedent = "Antecedent";

		public const string Dependent = "Dependent";

		public const string PartComponent = "PartComponent";
	}

	private const uint m_ErrorCodeSuccess = 0u;

	private const uint m_ErrorCodeJob = 4096u;

	protected static TimeSpan gm_NewObjectUpdateTime = TimeSpan.FromSeconds(1.0);

	private static readonly IReadOnlyList<Association> gm_AssociationsNotToUpdate = new Association[0];

	private IProxy m_Proxy;

	private ObjectKey m_Key;

	private readonly Dictionary<string, object> m_PropertyStore = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

	private EventHandler m_Deleted;

	private EventHandler m_PropertyCacheUpdated;

	internal static uint ErrorCodeSuccess => 0u;

	internal static uint ErrorCodeJob => 4096u;

	public Server Server => m_Key.Server;

	protected ObjectKey Key => m_Key;

	public WmiObjectPath ManagementPath => m_Proxy.Path;

	internal IProxy Proxy => m_Proxy;

	internal IProxyFactory ProxyFactory => Microsoft.Virtualization.Client.Management.ProxyFactory.Instance;

	protected Associations Associations => Associations.GetAssociations(Server);

	protected virtual IReadOnlyList<Association> AssociationsNotToUpdate => gm_AssociationsNotToUpdate;

	public virtual event EventHandler Deleted
	{
		add
		{
			if (m_Deleted == null && value != null)
			{
				m_Proxy.Deleted += OnProxyDeleted;
			}
			m_Deleted = (EventHandler)Delegate.Combine(m_Deleted, value);
		}
		remove
		{
			if (m_Deleted != null && value != null)
			{
				m_Deleted = (EventHandler)Delegate.Remove(m_Deleted, value);
				if (m_Deleted == null)
				{
					m_Proxy.Deleted -= OnProxyDeleted;
				}
			}
		}
	}

	public virtual event EventHandler CacheUpdated
	{
		add
		{
			if (m_PropertyCacheUpdated == null && value != null)
			{
				m_Proxy.PropertyCacheUpdated += OnCacheUpdated;
			}
			m_PropertyCacheUpdated = (EventHandler)Delegate.Combine(m_PropertyCacheUpdated, value);
		}
		remove
		{
			if (m_PropertyCacheUpdated != null && value != null)
			{
				m_PropertyCacheUpdated = (EventHandler)Delegate.Remove(m_PropertyCacheUpdated, value);
				if (m_PropertyCacheUpdated == null)
				{
					m_Proxy.PropertyCacheUpdated -= OnCacheUpdated;
				}
			}
		}
	}

	internal virtual void Initialize(IProxy proxy, ObjectKey key)
	{
		if (proxy == null)
		{
			throw new ArgumentNullException("proxy");
		}
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		m_Proxy = proxy;
		m_Key = key;
	}

	protected virtual void OnCacheUpdated(object sender, EventArgs ea)
	{
		RaiseCacheUpdated(EventArgs.Empty);
	}

	protected void RaiseCacheUpdated(EventArgs ea)
	{
		if (m_PropertyCacheUpdated != null)
		{
			m_PropertyCacheUpdated(this, ea);
		}
	}

	protected virtual void OnProxyDeleted(object sender, EventArgs ea)
	{
		RaiseDeleted(EventArgs.Empty);
	}

	protected void RaiseDeleted(EventArgs ea)
	{
		if (m_Deleted != null)
		{
			m_Deleted(this, ea);
		}
	}

	public void InvalidatePropertyCache()
	{
		m_Proxy.InvalidatePropertyCache();
	}

	public bool NeedsUpdate(TimeSpan refreshTime)
	{
		return m_Proxy.NeedsUpdate(refreshTime);
	}

	public void UpdatePropertyCache()
	{
		UpdatePropertyCache(TimeSpan.Zero);
	}

	public virtual void UpdatePropertyCache(TimeSpan threshold)
	{
		m_Proxy.UpdatePropertyCache(threshold);
	}

	public virtual void RegisterForInstanceModificationEvents(InstanceModificationEventStrategy strategy)
	{
		m_Proxy.RegisterForInstanceModificationEvents(strategy);
	}

	public virtual void UnregisterForInstanceModificationEvents()
	{
		m_Proxy.UnregisterForInstanceModificationEvents();
	}

	public void InvalidateAssociationCache()
	{
		m_Proxy.InvalidateAssociationCache();
	}

	public void UpdateAssociationCache()
	{
		UpdateAssociationCache(TimeSpan.Zero);
	}

	public virtual void UpdateAssociationCache(TimeSpan threshold)
	{
		m_Proxy.UpdateAssociationCache(threshold, AssociationsNotToUpdate);
	}

	public void DiscardPendingPropertyChanges()
	{
		m_PropertyStore.Clear();
	}

	public void Put()
	{
		using IVMTask iVMTask = BeginPut();
		iVMTask.WaitForCompletion();
		EndPut(iVMTask);
	}

	protected IDictionary<string, object> GetChangedProperties()
	{
		if (!m_PropertyStore.Any())
		{
			return null;
		}
		return new Dictionary<string, object>(m_PropertyStore, StringComparer.OrdinalIgnoreCase);
	}

	public IVMTask BeginPut()
	{
		IDictionary<string, object> changedProperties = GetChangedProperties();
		if (changedProperties == null)
		{
			return new CompletedTask(Server);
		}
		return BeginPutInternal(changedProperties);
	}

	public void EndPut(IVMTask putTask)
	{
		if (putTask == null)
		{
			throw new ArgumentNullException("putTask", "PutTask can not be null.");
		}
		if (!putTask.IsCompleted)
		{
			throw new ArgumentException("Can only end a method with a completed task.");
		}
		if ((uint)putTask.ErrorCode == ErrorCodeSuccess)
		{
			IDictionary<string, object> putProperties = putTask.PutProperties;
			if (putProperties != null)
			{
				m_Proxy.UpdatePropertyCache(putProperties);
				foreach (KeyValuePair<string, object> item in putProperties)
				{
					if (m_PropertyStore[item.Key] == item.Value)
					{
						m_PropertyStore.Remove(item.Key);
					}
				}
			}
			VMTrace.TraceUserActionCompleted("Put operation completed successfully. Object's properties have been modified.");
			return;
		}
		Exception ex = null;
		if (putTask is CompletedTask completedTask)
		{
			ex = completedTask.WrappedException;
		}
		bool operationCanceled = putTask.Status == VMTaskStatus.Canceled;
		GetErrorInformationFromTask(putTask, out var errorSummaryDescription, out var errorDetailsDescription, out var errorCode, out var errorCodeMapper);
		if (ex != null && IgnorePutInnerException(errorCode, ex))
		{
			ex = null;
		}
		throw ThrowHelper.CreateVirtualizationOperationFailedException(errorSummaryDescription, errorDetailsDescription, VirtualizationOperation.Put, errorCode, operationCanceled, errorCodeMapper, ex);
	}

	protected virtual bool IgnorePutInnerException(long errorCode, Exception putEx)
	{
		bool result = false;
		if (errorCode == 32770)
		{
			result = putEx is ServerCallFailedException ex && ex.FailureReason == ServerCallFailedReason.NotSupported;
		}
		return result;
	}

	protected virtual IVMTask BeginPutInternal(IDictionary<string, object> properties)
	{
		try
		{
			VMTrace.TraceUserActionInitiatedWithProperties(string.Format(CultureInfo.InvariantCulture, "Beginning Put operation to modify object '{0}'.", ManagementPath.ToString()), properties);
			m_Proxy.PutProperties(properties);
			return new CompletedTask(Server)
			{
				PutProperties = properties
			};
		}
		catch (Exception wrappedException)
		{
			return new CompletedTask(Server, wrappedException)
			{
				PutProperties = properties
			};
		}
	}

	protected TPropertyType GetProperty<TPropertyType>(string wmiPropertyName)
	{
		if (!m_PropertyStore.TryGetValue(wmiPropertyName, out var value))
		{
			value = m_Proxy.GetProperty(wmiPropertyName);
		}
		if (value == null)
		{
			return default(TPropertyType);
		}
		try
		{
			return (TPropertyType)value;
		}
		catch (InvalidCastException inner)
		{
			throw ThrowHelper.CreateInvalidPropertyValueException(wmiPropertyName, typeof(TPropertyType), value, inner);
		}
	}

	protected IEnumerable<string> GetPropertyNames()
	{
		return m_PropertyStore.Keys.Union(m_Proxy.GetPropertyNames());
	}

	protected TPropertyType GetPropertyOrDefault<TPropertyType>(string wmiPropertyName, TPropertyType defaultValue = default(TPropertyType))
	{
		if (!DoesPropertyExist(wmiPropertyName))
		{
			return defaultValue;
		}
		return GetProperty<TPropertyType>(wmiPropertyName);
	}

	protected TClientType GetProperty<TClientType>(string wmiPropertyName, WmiTypeConverter<TClientType> converter)
	{
		object property = GetProperty<object>(wmiPropertyName);
		try
		{
			return converter.ConvertFromWmiType(property);
		}
		catch (InvalidCastException inner)
		{
			throw ThrowHelper.CreateInvalidPropertyValueException(wmiPropertyName, typeof(TClientType), property, inner);
		}
		catch (NullReferenceException inner2)
		{
			throw ThrowHelper.CreateInvalidPropertyValueException(wmiPropertyName, typeof(TClientType), property, inner2);
		}
	}

	protected void SetProperty(string wmiPropertyName, object value)
	{
		m_PropertyStore[wmiPropertyName] = value;
	}

	protected bool DoesPropertyExist(string wmiPropertyName)
	{
		return m_Proxy.DoesPropertyExist(wmiPropertyName);
	}

	protected uint InvokeMethod(string name, params object[] args)
	{
		return m_Proxy.InvokeMethod(name, args);
	}

	protected void InvokeMethodWithoutReturn(string name, params object[] args)
	{
		m_Proxy.InvokeMethodWithoutReturn(name, args);
	}

	protected TReturnType InvokeMethodWithReturn<TReturnType>(string name, params object[] args)
	{
		return m_Proxy.InvokeMethodWithReturn<TReturnType>(name, args);
	}

	protected IVMTask BeginMethodTaskReturn(uint result, object affectedElementPath, object taskPath)
	{
		if (result == ErrorCodeSuccess)
		{
			if (affectedElementPath == null)
			{
				return new CompletedTask(Server);
			}
			if (!(affectedElementPath is IList list))
			{
				IVirtualizationManagementObject viewFromPath = GetViewFromPath((WmiObjectPath)affectedElementPath);
				return new CompletedTask(Server, viewFromPath);
			}
			IVirtualizationManagementObject[] array = new IVirtualizationManagementObject[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				array[i] = GetViewFromPath((WmiObjectPath)list[i]);
			}
			return new CompletedTask(Server, array);
		}
		if (result == ErrorCodeJob)
		{
			if (taskPath == null)
			{
				throw new ArgumentNullException("taskPath", "Cannot be null when result is job.");
			}
			return GetTaskFromPath((WmiObjectPath)taskPath);
		}
		return new CompletedTask(Server, result);
	}

	protected void EndMethod(IVMTask task, VirtualizationOperation operation)
	{
		EndMethodReturnInternal(task, operation, affectedElementExpected: false);
	}

	protected AffectedElementType EndMethodReturn<AffectedElementType>(IVMTask task, VirtualizationOperation operation) where AffectedElementType : class, IVirtualizationManagementObject
	{
		return EndMethodReturn<AffectedElementType>(task, operation, throwIfAffectedElementNotFound: true);
	}

	protected AffectedElementType EndMethodReturn<AffectedElementType>(IVMTask task, VirtualizationOperation operation, bool throwIfAffectedElementNotFound) where AffectedElementType : class, IVirtualizationManagementObject
	{
		AffectedElementType val = null;
		foreach (IVirtualizationManagementObject item in EndMethodReturnInternal(task, operation, affectedElementExpected: true))
		{
			val = item as AffectedElementType;
			if (val != null)
			{
				break;
			}
		}
		if (val == null && throwIfAffectedElementNotFound)
		{
			long errorCode = -2L;
			ErrorCodeMapper errorCodeMapper = GetErrorCodeMapper();
			throw ThrowHelper.CreateVirtualizationOperationFailedException(task.ClientSideFailedMessage, operation, errorCode, errorCodeMapper, null);
		}
		return val;
	}

	protected IEnumerable<AffectedElementType> EndMethodReturnEnumeration<AffectedElementType>(IVMTask task, VirtualizationOperation operation) where AffectedElementType : class, IVirtualizationManagementObject
	{
		IEnumerable<IVirtualizationManagementObject> enumerable = EndMethodReturnInternal(task, operation, affectedElementExpected: true);
		List<AffectedElementType> list = new List<AffectedElementType>();
		foreach (IVirtualizationManagementObject item2 in enumerable)
		{
			if (item2 is AffectedElementType item)
			{
				list.Add(item);
			}
		}
		return list;
	}

	private IEnumerable<IVirtualizationManagementObject> EndMethodReturnInternal(IVMTask task, VirtualizationOperation operation, bool affectedElementExpected)
	{
		if (task == null)
		{
			throw new ArgumentNullException("task");
		}
		IEnumerable<IVirtualizationManagementObject> result = null;
		if (task.Status != VMTaskStatus.CompletedSuccessfully)
		{
			bool operationCanceled = task.Status == VMTaskStatus.Canceled;
			GetErrorInformationFromTask(task, out var errorSummaryDescription, out var errorDetailsDescription, out var errorCode, out var errorCodeMapper);
			throw ThrowHelper.CreateVirtualizationOperationFailedException(errorSummaryDescription, errorDetailsDescription, operation, errorCode, operationCanceled, errorCodeMapper, null);
		}
		if (affectedElementExpected)
		{
			task.UpdateAssociationCache();
			result = task.AffectedElements;
		}
		return result;
	}

	protected void GetErrorInformationFromTask(IVMTask task, out string errorSummaryDescription, out string errorDetailsDescription, out long errorCode, out ErrorCodeMapper errorCodeMapper)
	{
		if (task == null)
		{
			throw new ArgumentNullException("task");
		}
		if (task.Status == VMTaskStatus.CompletedSuccessfully)
		{
			throw new ArgumentException(null, "task");
		}
		errorSummaryDescription = task.ErrorSummaryDescription;
		errorDetailsDescription = task.ErrorDetailsDescription;
		errorCode = task.ErrorCode;
		if (string.IsNullOrEmpty(errorSummaryDescription))
		{
			errorSummaryDescription = errorDetailsDescription;
			errorDetailsDescription = null;
		}
		if (string.IsNullOrEmpty(errorSummaryDescription))
		{
			errorSummaryDescription = task.ClientSideFailedMessage;
			errorDetailsDescription = null;
			errorCodeMapper = GetErrorCodeMapper();
		}
		else
		{
			errorCodeMapper = new ServerProvidedMessageErrorCodeMapper();
		}
	}

	protected virtual ErrorCodeMapper GetErrorCodeMapper()
	{
		return new ErrorCodeMapper();
	}

	protected IVMTask BeginNetworkMethodTaskReturn(uint result, object affectedElementPath, object taskPath)
	{
		try
		{
			IVMTask iVMTask = BeginMethodTaskReturn(result, affectedElementPath, taskPath);
			if (!iVMTask.IsCompleted)
			{
				return new VMNetworkTaskWait(Server, iVMTask);
			}
			return iVMTask;
		}
		catch (ServerConnectionException ex)
		{
			if (ex.Issue == ServerConnectionIssue.RpcServerUnavailable)
			{
				return HandleNetworkTaskConnectionError(taskPath as string);
			}
			throw;
		}
		catch (ServerCallFailedException ex2)
		{
			if (ex2.FailureReason == ServerCallFailedReason.RpcCallFailed)
			{
				return HandleNetworkTaskConnectionError(taskPath as string);
			}
			throw;
		}
	}

	private IVMTask HandleNetworkTaskConnectionError(string networkJobPath)
	{
		try
		{
			return new VMNetworkTaskWait(Server, WmiObjectPath.FromRelativePath(Server, ManagementPath.NamespaceName, networkJobPath));
		}
		catch (ArgumentException inner)
		{
			IVMTask iVMTask = null;
			throw ThrowHelper.CreateServerCallFailedException(Server, ServerCallFailedReason.UnknownProviderError, inner);
		}
	}

	protected IEnumerable<T> GetRelatedObjects<T>(Association association, WmiOperationOptions options = null) where T : IVirtualizationManagementObject
	{
		IEnumerable<ObjectKey> relatedObjectKeys = m_Proxy.GetRelatedObjectKeys(association, options);
		foreach (ObjectKey item in relatedObjectKeys)
		{
			T val = default(T);
			try
			{
				IVirtualizationManagementObject virtualizationManagementObject = ObjectFactory.Instance.GetVirtualizationManagementObject<IVirtualizationManagementObject>(item);
				if (virtualizationManagementObject is T)
				{
					val = (T)virtualizationManagementObject;
				}
			}
			catch (VirtualizationManagementException ex)
			{
				VMTrace.TraceError("Error getting the related object! (This can happen if the related object is not in the cache and we need to retrieve it and either there is a network problem or the object no longer exists.)", ex);
			}
			if (val != null)
			{
				yield return val;
			}
		}
	}

	protected T GetRelatedObject<T>(Association association, bool throwIfNotFound = true, WmiOperationOptions options = null) where T : IVirtualizationManagementObject
	{
		IEnumerator<T> enumerator = null;
		try
		{
			enumerator = GetRelatedObjects<T>(association, options).GetEnumerator();
			T result = default(T);
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
			if (throwIfNotFound)
			{
				throw ThrowHelper.CreateRelatedObjectNotFoundException(Server, typeof(T));
			}
			return result;
		}
		finally
		{
			enumerator?.Dispose();
		}
	}

	protected IVMTask GetTaskFromPath(WmiObjectPath taskPath)
	{
		return (IVMTask)GetViewFromPath(taskPath);
	}

	protected WmiObjectPath GetWmiObjectPathFromPath(string path)
	{
		return WmiObjectPath.FromRelativePath(Server, ManagementPath.NamespaceName, path);
	}

	protected IVirtualizationManagementObject GetViewFromPath(string path)
	{
		return GetViewFromPath(GetWmiObjectPathFromPath(path));
	}

	protected IVirtualizationManagementObject GetViewFromPath(WmiObjectPath path)
	{
		ObjectKey objectKey = null;
		try
		{
			objectKey = new ObjectKey(Server, path);
		}
		catch (ArgumentException inner)
		{
			throw ThrowHelper.CreateServerCallFailedException(Server, ServerCallFailedReason.UnknownProviderError, inner);
		}
		catch (NoWmiMappingException inner2)
		{
			throw ThrowHelper.CreateServerCallFailedException(Server, ServerCallFailedReason.UnknownProviderError, inner2);
		}
		return ObjectFactory.Instance.GetVirtualizationManagementObject<IVirtualizationManagementObject>(objectKey);
	}

	protected IProxy GetServiceProxy()
	{
		return ProxyFactory.GetProxy(ObjectKeyCreator.CreateVMServiceObjectKey(Server), delayInitializePropertyCache: true);
	}

	protected IProxy GetFailoverReplicationServiceProxy()
	{
		return ProxyFactory.GetProxy(ObjectKeyCreator.CreateFailoverReplicationServiceObjectKey(Server), delayInitializePropertyCache: true);
	}

	protected IProxy GetSnapshotServiceProxy()
	{
		return ProxyFactory.GetProxy(ObjectKeyCreator.CreateSnapshotServiceObjectKey(Server), delayInitializePropertyCache: true);
	}

	protected IProxy GetSwitchManagementServiceProxy()
	{
		return ProxyFactory.GetProxy(ObjectKeyCreator.CreateSwitchManagementServiceObjectKey(Server), delayInitializePropertyCache: true);
	}

	protected IProxy GetResourcePoolConfigurationServiceProxy()
	{
		return ProxyFactory.GetProxy(ObjectKeyCreator.CreateResourcePoolConfigServiceObjectKey(Server), delayInitializePropertyCache: true);
	}

	protected IProxy GetCollectionManagementServiceProxy()
	{
		return ProxyFactory.GetProxy(ObjectKeyCreator.CreateCollectionManagementServiceObjectKey(Server), delayInitializePropertyCache: true);
	}

	protected string GetEmbeddedInstance(IDictionary<string, object> properties)
	{
		string text = null;
		try
		{
			return Server.GetEmbeddedInstance(ManagementPath, properties);
		}
		catch (Exception serverException)
		{
			throw ThrowHelper.CreateServerException(Server, serverException);
		}
	}

	public string GetEmbeddedInstance()
	{
		return GetEmbeddedInstance(m_PropertyStore);
	}

	protected void RegisterForInstanceCreationEvent(EventObjectKey creationEventKey, InstanceEventArrivedHandler eventHandler)
	{
		InstanceEventManager.GetInstanceEventMonitor(creationEventKey)?.RegisterObject(creationEventKey, eventHandler);
	}

	protected void UnregisterForInstanceCreationEvent(EventObjectKey creationEventKey, InstanceEventArrivedHandler eventHandler)
	{
		InstanceEventManager.GetInstanceEventMonitor(creationEventKey)?.UnregisterObject(creationEventKey, eventHandler);
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
	private void ObjectInvariant()
	{
	}
}
