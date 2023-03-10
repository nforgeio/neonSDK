using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Options;

namespace Microsoft.Virtualization.Client.Management;

internal class CimSessionWrapper : ICimSession, IDisposable
{
	private readonly CimSession session;

	public string ComputerName => session.ComputerName;

	public Guid InstanceId => session.InstanceId;

	public CimSession Session => session;

	internal CimSessionWrapper(CimSession session)
	{
		this.session = session;
	}

	public void Close()
	{
		session.Close();
	}

	public CimAsyncStatus CloseAsync()
	{
		return session.CloseAsync();
	}

	public ICimInstance GetInstance(string namespaceName, ICimInstance instanceId)
	{
		return session.GetInstance(namespaceName, instanceId.Instance).ToICimInstance();
	}

	public ICimInstance GetInstance(string namespaceName, ICimInstance instanceId, CimOperationOptions options)
	{
		return session.GetInstance(namespaceName, instanceId.Instance, options).ToICimInstance();
	}

	public ICimInstance ModifyInstance(ICimInstance instance)
	{
		return session.ModifyInstance(instance.Instance).ToICimInstance();
	}

	public ICimInstance ModifyInstance(string namespaceName, ICimInstance instance)
	{
		return session.ModifyInstance(namespaceName, instance.Instance).ToICimInstance();
	}

	public ICimInstance ModifyInstance(string namespaceName, ICimInstance instance, CimOperationOptions options)
	{
		return session.ModifyInstance(namespaceName, instance.Instance, options).ToICimInstance();
	}

	public ICimInstance CreateInstance(string namespaceName, ICimInstance instance)
	{
		return session.CreateInstance(namespaceName, instance.Instance).ToICimInstance();
	}

	public ICimInstance CreateInstance(string namespaceName, ICimInstance instance, CimOperationOptions options)
	{
		return session.CreateInstance(namespaceName, instance.Instance, options).ToICimInstance();
	}

	public CimAsyncResult<CimInstance> CreateInstanceAsync(string namespaceName, ICimInstance instance)
	{
		return session.CreateInstanceAsync(namespaceName, instance.Instance);
	}

	public CimAsyncResult<CimInstance> CreateInstanceAsync(string namespaceName, ICimInstance instance, CimOperationOptions options)
	{
		return session.CreateInstanceAsync(namespaceName, instance.Instance, options);
	}

	public void DeleteInstance(ICimInstance instance)
	{
		session.DeleteInstance(instance.Instance);
	}

	public void DeleteInstance(string namespaceName, ICimInstance instance)
	{
		session.DeleteInstance(namespaceName, instance.Instance);
	}

	public void DeleteInstance(string namespaceName, ICimInstance instance, CimOperationOptions options)
	{
		session.DeleteInstance(namespaceName, instance.Instance, options);
	}

	public CimAsyncStatus DeleteInstanceAsync(ICimInstance instance)
	{
		return session.DeleteInstanceAsync(instance.Instance);
	}

	public CimAsyncStatus DeleteInstanceAsync(string namespaceName, ICimInstance instance)
	{
		return session.DeleteInstanceAsync(namespaceName, instance.Instance);
	}

	public CimAsyncStatus DeleteInstanceAsync(string namespaceName, ICimInstance instance, CimOperationOptions options)
	{
		return session.DeleteInstanceAsync(namespaceName, instance.Instance, options);
	}

	public IEnumerable<CimSubscriptionResult> Subscribe(string namespaceName, string queryDialect, string queryExpression)
	{
		return session.Subscribe(namespaceName, queryDialect, queryExpression);
	}

	public IEnumerable<CimSubscriptionResult> Subscribe(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions operationOptions)
	{
		return session.Subscribe(namespaceName, queryDialect, queryExpression, operationOptions);
	}

	public IEnumerable<CimSubscriptionResult> Subscribe(string namespaceName, string queryDialect, string queryExpression, CimSubscriptionDeliveryOptions options)
	{
		return session.Subscribe(namespaceName, queryDialect, queryExpression, options);
	}

	public IEnumerable<CimSubscriptionResult> Subscribe(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions operationOptions, CimSubscriptionDeliveryOptions options)
	{
		return session.Subscribe(namespaceName, queryDialect, queryExpression, operationOptions, options);
	}

	public CimAsyncMultipleResults<CimSubscriptionResult> SubscribeAsync(string namespaceName, string queryDialect, string queryExpression)
	{
		return session.SubscribeAsync(namespaceName, queryDialect, queryExpression);
	}

	public CimAsyncMultipleResults<CimSubscriptionResult> SubscribeAsync(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions operationOptions)
	{
		return session.SubscribeAsync(namespaceName, queryDialect, queryExpression, operationOptions);
	}

	public CimAsyncMultipleResults<CimSubscriptionResult> SubscribeAsync(string namespaceName, string queryDialect, string queryExpression, CimSubscriptionDeliveryOptions options)
	{
		return session.SubscribeAsync(namespaceName, queryDialect, queryExpression, options);
	}

	public CimAsyncMultipleResults<CimSubscriptionResult> SubscribeAsync(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions operationOptions, CimSubscriptionDeliveryOptions options)
	{
		return session.SubscribeAsync(namespaceName, queryDialect, queryExpression, operationOptions, options);
	}

	public IEnumerable<ICimInstance> EnumerateInstances(string namespaceName, string className)
	{
		return session.EnumerateInstances(namespaceName, className).ToICimInstances();
	}

	public IEnumerable<ICimInstance> EnumerateInstances(string namespaceName, string className, CimOperationOptions options)
	{
		return session.EnumerateInstances(namespaceName, className, options).ToICimInstances();
	}

	public CimAsyncMultipleResults<CimInstance> EnumerateInstancesAsync(string namespaceName, string className)
	{
		return session.EnumerateInstancesAsync(namespaceName, className);
	}

	public CimAsyncMultipleResults<CimInstance> EnumerateInstancesAsync(string namespaceName, string className, CimOperationOptions options)
	{
		return session.EnumerateInstancesAsync(namespaceName, className, options);
	}

	public IEnumerable<ICimInstance> QueryInstances(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions options)
	{
		return session.QueryInstances(namespaceName, queryDialect, queryExpression, options).ToICimInstances();
	}

	public CimAsyncMultipleResults<CimInstance> QueryInstancesAsync(string namespaceName, string queryDialect, string queryExpression)
	{
		return session.QueryInstancesAsync(namespaceName, queryDialect, queryExpression);
	}

	public CimAsyncMultipleResults<CimInstance> QueryInstancesAsync(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions options)
	{
		return session.QueryInstancesAsync(namespaceName, queryDialect, queryExpression, options);
	}

	public IEnumerable<ICimInstance> EnumerateAssociatedInstances(string namespaceName, ICimInstance sourceInstance, string associationClassName, string resultClassName, string sourceRole, string resultRole)
	{
		return session.EnumerateAssociatedInstances(namespaceName, sourceInstance.Instance, associationClassName, resultClassName, sourceRole, resultRole).ToICimInstances();
	}

	public IEnumerable<ICimInstance> EnumerateAssociatedInstances(string namespaceName, ICimInstance sourceInstance, string associationClassName, string resultClassName, string sourceRole, string resultRole, CimOperationOptions options)
	{
		return session.EnumerateAssociatedInstances(namespaceName, sourceInstance.Instance, associationClassName, resultClassName, sourceRole, resultRole, options).ToICimInstances();
	}

	public CimAsyncMultipleResults<CimInstance> EnumerateAssociatedInstancesAsync(string namespaceName, ICimInstance sourceInstance, string associationClassName, string resultClassName, string sourceRole, string resultRole)
	{
		return session.EnumerateAssociatedInstancesAsync(namespaceName, sourceInstance.Instance, associationClassName, resultClassName, sourceRole, resultRole);
	}

	public CimAsyncMultipleResults<CimInstance> EnumerateAssociatedInstancesAsync(string namespaceName, ICimInstance sourceInstance, string associationClassName, string resultClassName, string sourceRole, string resultRole, CimOperationOptions options)
	{
		return session.EnumerateAssociatedInstancesAsync(namespaceName, sourceInstance.Instance, associationClassName, resultClassName, sourceRole, resultRole, options);
	}

	public IEnumerable<ICimInstance> EnumerateReferencingInstances(string namespaceName, ICimInstance sourceInstance, string associationClassName, string sourceRole)
	{
		return session.EnumerateReferencingInstances(namespaceName, sourceInstance.Instance, associationClassName, sourceRole).ToICimInstances();
	}

	public IEnumerable<ICimInstance> EnumerateReferencingInstances(string namespaceName, ICimInstance sourceInstance, string associationClassName, string sourceRole, CimOperationOptions options)
	{
		return session.EnumerateReferencingInstances(namespaceName, sourceInstance.Instance, associationClassName, sourceRole, options).ToICimInstances();
	}

	public CimAsyncMultipleResults<CimInstance> EnumerateReferencingInstancesAsync(string namespaceName, ICimInstance sourceInstance, string associationClassName, string sourceRole)
	{
		return session.EnumerateReferencingInstancesAsync(namespaceName, sourceInstance.Instance, associationClassName, sourceRole);
	}

	public CimAsyncMultipleResults<CimInstance> EnumerateReferencingInstancesAsync(string namespaceName, ICimInstance sourceInstance, string associationClassName, string sourceRole, CimOperationOptions options)
	{
		return session.EnumerateReferencingInstancesAsync(namespaceName, sourceInstance.Instance, associationClassName, sourceRole, options);
	}

	public CimMethodResult InvokeMethod(ICimInstance instance, string methodName, CimMethodParametersCollection methodParameters)
	{
		return session.InvokeMethod(instance.Instance, methodName, methodParameters);
	}

	public CimMethodResult InvokeMethod(string namespaceName, ICimInstance instance, string methodName, CimMethodParametersCollection methodParameters)
	{
		return session.InvokeMethod(namespaceName, instance.Instance, methodName, methodParameters);
	}

	public CimMethodResult InvokeMethod(string namespaceName, ICimInstance instance, string methodName, CimMethodParametersCollection methodParameters, CimOperationOptions options)
	{
		return session.InvokeMethod(namespaceName, instance.Instance, methodName, methodParameters, options);
	}

	public CimAsyncResult<CimMethodResult> InvokeMethodAsync(ICimInstance instance, string methodName, CimMethodParametersCollection methodParameters)
	{
		return session.InvokeMethodAsync(instance.Instance, methodName, methodParameters);
	}

	public CimAsyncResult<CimMethodResult> InvokeMethodAsync(string namespaceName, ICimInstance instance, string methodName, CimMethodParametersCollection methodParameters)
	{
		return session.InvokeMethodAsync(namespaceName, instance.Instance, methodName, methodParameters);
	}

	public CimAsyncMultipleResults<CimMethodResultBase> InvokeMethodAsync(string namespaceName, ICimInstance instance, string methodName, CimMethodParametersCollection methodParameters, CimOperationOptions options)
	{
		return session.InvokeMethodAsync(namespaceName, instance.Instance, methodName, methodParameters, options);
	}

	public CimMethodResult InvokeMethod(string namespaceName, string className, string methodName, CimMethodParametersCollection methodParameters)
	{
		return session.InvokeMethod(namespaceName, className, methodName, methodParameters);
	}

	public CimMethodResult InvokeMethod(string namespaceName, string className, string methodName, CimMethodParametersCollection methodParameters, CimOperationOptions options)
	{
		return session.InvokeMethod(namespaceName, className, methodName, methodParameters, options);
	}

	public CimAsyncResult<CimMethodResult> InvokeMethodAsync(string namespaceName, string className, string methodName, CimMethodParametersCollection methodParameters)
	{
		return session.InvokeMethodAsync(namespaceName, className, methodName, methodParameters);
	}

	public CimAsyncMultipleResults<CimMethodResultBase> InvokeMethodAsync(string namespaceName, string className, string methodName, CimMethodParametersCollection methodParameters, CimOperationOptions options)
	{
		return session.InvokeMethodAsync(namespaceName, className, methodName, methodParameters, options);
	}

	public ICimClass GetClass(string namespaceName, string className)
	{
		return session.GetClass(namespaceName, className).ToICimClass();
	}

	public ICimClass GetClass(string namespaceName, string className, CimOperationOptions options)
	{
		return session.GetClass(namespaceName, className, options).ToICimClass();
	}

	public CimAsyncResult<CimClass> GetClassAsync(string namespaceName, string className)
	{
		return session.GetClassAsync(namespaceName, className);
	}

	public CimAsyncResult<CimClass> GetClassAsync(string namespaceName, string className, CimOperationOptions options)
	{
		return session.GetClassAsync(namespaceName, className, options);
	}

	public IEnumerable<ICimClass> EnumerateClasses(string namespaceName)
	{
		return session.EnumerateClasses(namespaceName).ToICimClasses();
	}

	public IEnumerable<ICimClass> EnumerateClasses(string namespaceName, string className)
	{
		return session.EnumerateClasses(namespaceName, className).ToICimClasses();
	}

	public IEnumerable<ICimClass> EnumerateClasses(string namespaceName, string className, CimOperationOptions options)
	{
		return session.EnumerateClasses(namespaceName, className, options).ToICimClasses();
	}

	public CimAsyncMultipleResults<CimClass> EnumerateClassesAsync(string namespaceName)
	{
		return session.EnumerateClassesAsync(namespaceName);
	}

	public CimAsyncMultipleResults<CimClass> EnumerateClassesAsync(string namespaceName, string className)
	{
		return session.EnumerateClassesAsync(namespaceName, className);
	}

	public CimAsyncMultipleResults<CimClass> EnumerateClassesAsync(string namespaceName, string className, CimOperationOptions options)
	{
		return session.EnumerateClassesAsync(namespaceName, className, options);
	}

	public bool TestConnection()
	{
		return session.TestConnection();
	}

	public bool TestConnection(out ICimInstance instance, out CimException exception)
	{
		CimInstance instance2;
		bool result = session.TestConnection(out instance2, out exception);
		instance = instance2.ToICimInstance();
		return result;
	}

	public CimAsyncResult<CimInstance> TestConnectionAsync()
	{
		return session.TestConnectionAsync();
	}

	public void Dispose()
	{
		session.Dispose();
	}

	public IEnumerable<ICimInstance> QueryInstances(string namespaceName, string queryDialect, string queryExpression)
	{
		return session.QueryInstances(namespaceName, queryDialect, queryExpression).ToICimInstances();
	}

	public CimAsyncResult<CimInstance> GetInstanceAsync(string namespaceName, ICimInstance instanceId)
	{
		return session.GetInstanceAsync(namespaceName, instanceId.Instance);
	}

	public CimAsyncResult<CimInstance> GetInstanceAsync(string namespaceName, ICimInstance instanceId, CimOperationOptions options)
	{
		return session.GetInstanceAsync(namespaceName, instanceId.Instance, options);
	}

	public CimAsyncResult<CimInstance> ModifyInstanceAsync(ICimInstance instance)
	{
		return session.ModifyInstanceAsync(instance.Instance);
	}

	public CimAsyncResult<CimInstance> ModifyInstanceAsync(string namespaceName, ICimInstance instance)
	{
		return session.ModifyInstanceAsync(namespaceName, instance.Instance);
	}

	public CimAsyncResult<CimInstance> ModifyInstanceAsync(string namespaceName, ICimInstance instance, CimOperationOptions options)
	{
		return session.ModifyInstanceAsync(namespaceName, instance.Instance, options);
	}

	public override string ToString()
	{
		return session.ToString();
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
	private void ObjectInvariant()
	{
	}
}
