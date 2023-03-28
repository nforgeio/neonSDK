using System;
using System.Collections.Generic;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Options;

namespace Microsoft.Virtualization.Client.Management;

internal interface ICimSession : IDisposable
{
	string ComputerName { get; }

	Guid InstanceId { get; }

	CimSession Session { get; }

	void Close();

	CimAsyncStatus CloseAsync();

	ICimInstance GetInstance(string namespaceName, ICimInstance instanceId);

	ICimInstance GetInstance(string namespaceName, ICimInstance instanceId, CimOperationOptions options);

	CimAsyncResult<CimInstance> GetInstanceAsync(string namespaceName, ICimInstance instanceId);

	CimAsyncResult<CimInstance> GetInstanceAsync(string namespaceName, ICimInstance instanceId, CimOperationOptions options);

	ICimInstance ModifyInstance(ICimInstance instance);

	ICimInstance ModifyInstance(string namespaceName, ICimInstance instance);

	ICimInstance ModifyInstance(string namespaceName, ICimInstance instance, CimOperationOptions options);

	CimAsyncResult<CimInstance> ModifyInstanceAsync(ICimInstance instance);

	CimAsyncResult<CimInstance> ModifyInstanceAsync(string namespaceName, ICimInstance instance);

	CimAsyncResult<CimInstance> ModifyInstanceAsync(string namespaceName, ICimInstance instance, CimOperationOptions options);

	ICimInstance CreateInstance(string namespaceName, ICimInstance instance);

	ICimInstance CreateInstance(string namespaceName, ICimInstance instance, CimOperationOptions options);

	CimAsyncResult<CimInstance> CreateInstanceAsync(string namespaceName, ICimInstance instance);

	CimAsyncResult<CimInstance> CreateInstanceAsync(string namespaceName, ICimInstance instance, CimOperationOptions options);

	void DeleteInstance(ICimInstance instance);

	void DeleteInstance(string namespaceName, ICimInstance instance);

	void DeleteInstance(string namespaceName, ICimInstance instance, CimOperationOptions options);

	CimAsyncStatus DeleteInstanceAsync(ICimInstance instance);

	CimAsyncStatus DeleteInstanceAsync(string namespaceName, ICimInstance instance);

	CimAsyncStatus DeleteInstanceAsync(string namespaceName, ICimInstance instance, CimOperationOptions options);

	IEnumerable<CimSubscriptionResult> Subscribe(string namespaceName, string queryDialect, string queryExpression);

	IEnumerable<CimSubscriptionResult> Subscribe(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions operationOptions);

	IEnumerable<CimSubscriptionResult> Subscribe(string namespaceName, string queryDialect, string queryExpression, CimSubscriptionDeliveryOptions options);

	IEnumerable<CimSubscriptionResult> Subscribe(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions operationOptions, CimSubscriptionDeliveryOptions options);

	CimAsyncMultipleResults<CimSubscriptionResult> SubscribeAsync(string namespaceName, string queryDialect, string queryExpression);

	CimAsyncMultipleResults<CimSubscriptionResult> SubscribeAsync(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions operationOptions);

	CimAsyncMultipleResults<CimSubscriptionResult> SubscribeAsync(string namespaceName, string queryDialect, string queryExpression, CimSubscriptionDeliveryOptions options);

	CimAsyncMultipleResults<CimSubscriptionResult> SubscribeAsync(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions operationOptions, CimSubscriptionDeliveryOptions options);

	IEnumerable<ICimInstance> EnumerateInstances(string namespaceName, string className);

	IEnumerable<ICimInstance> EnumerateInstances(string namespaceName, string className, CimOperationOptions options);

	CimAsyncMultipleResults<CimInstance> EnumerateInstancesAsync(string namespaceName, string className);

	CimAsyncMultipleResults<CimInstance> EnumerateInstancesAsync(string namespaceName, string className, CimOperationOptions options);

	IEnumerable<ICimInstance> QueryInstances(string namespaceName, string queryDialect, string queryExpression);

	IEnumerable<ICimInstance> QueryInstances(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions options);

	CimAsyncMultipleResults<CimInstance> QueryInstancesAsync(string namespaceName, string queryDialect, string queryExpression);

	CimAsyncMultipleResults<CimInstance> QueryInstancesAsync(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions options);

	IEnumerable<ICimInstance> EnumerateAssociatedInstances(string namespaceName, ICimInstance sourceInstance, string associationClassName, string resultClassName, string sourceRole, string resultRole);

	IEnumerable<ICimInstance> EnumerateAssociatedInstances(string namespaceName, ICimInstance sourceInstance, string associationClassName, string resultClassName, string sourceRole, string resultRole, CimOperationOptions options);

	CimAsyncMultipleResults<CimInstance> EnumerateAssociatedInstancesAsync(string namespaceName, ICimInstance sourceInstance, string associationClassName, string resultClassName, string sourceRole, string resultRole);

	CimAsyncMultipleResults<CimInstance> EnumerateAssociatedInstancesAsync(string namespaceName, ICimInstance sourceInstance, string associationClassName, string resultClassName, string sourceRole, string resultRole, CimOperationOptions options);

	IEnumerable<ICimInstance> EnumerateReferencingInstances(string namespaceName, ICimInstance sourceInstance, string associationClassName, string sourceRole);

	IEnumerable<ICimInstance> EnumerateReferencingInstances(string namespaceName, ICimInstance sourceInstance, string associationClassName, string sourceRole, CimOperationOptions options);

	CimAsyncMultipleResults<CimInstance> EnumerateReferencingInstancesAsync(string namespaceName, ICimInstance sourceInstance, string associationClassName, string sourceRole);

	CimAsyncMultipleResults<CimInstance> EnumerateReferencingInstancesAsync(string namespaceName, ICimInstance sourceInstance, string associationClassName, string sourceRole, CimOperationOptions options);

	CimMethodResult InvokeMethod(ICimInstance instance, string methodName, CimMethodParametersCollection methodParameters);

	CimMethodResult InvokeMethod(string namespaceName, ICimInstance instance, string methodName, CimMethodParametersCollection methodParameters);

	CimMethodResult InvokeMethod(string namespaceName, ICimInstance instance, string methodName, CimMethodParametersCollection methodParameters, CimOperationOptions options);

	CimAsyncResult<CimMethodResult> InvokeMethodAsync(ICimInstance instance, string methodName, CimMethodParametersCollection methodParameters);

	CimAsyncResult<CimMethodResult> InvokeMethodAsync(string namespaceName, ICimInstance instance, string methodName, CimMethodParametersCollection methodParameters);

	CimAsyncMultipleResults<CimMethodResultBase> InvokeMethodAsync(string namespaceName, ICimInstance instance, string methodName, CimMethodParametersCollection methodParameters, CimOperationOptions options);

	CimMethodResult InvokeMethod(string namespaceName, string className, string methodName, CimMethodParametersCollection methodParameters);

	CimMethodResult InvokeMethod(string namespaceName, string className, string methodName, CimMethodParametersCollection methodParameters, CimOperationOptions options);

	CimAsyncResult<CimMethodResult> InvokeMethodAsync(string namespaceName, string className, string methodName, CimMethodParametersCollection methodParameters);

	CimAsyncMultipleResults<CimMethodResultBase> InvokeMethodAsync(string namespaceName, string className, string methodName, CimMethodParametersCollection methodParameters, CimOperationOptions options);

	ICimClass GetClass(string namespaceName, string className);

	ICimClass GetClass(string namespaceName, string className, CimOperationOptions options);

	CimAsyncResult<CimClass> GetClassAsync(string namespaceName, string className);

	CimAsyncResult<CimClass> GetClassAsync(string namespaceName, string className, CimOperationOptions options);

	IEnumerable<ICimClass> EnumerateClasses(string namespaceName);

	IEnumerable<ICimClass> EnumerateClasses(string namespaceName, string className);

	IEnumerable<ICimClass> EnumerateClasses(string namespaceName, string className, CimOperationOptions options);

	CimAsyncMultipleResults<CimClass> EnumerateClassesAsync(string namespaceName);

	CimAsyncMultipleResults<CimClass> EnumerateClassesAsync(string namespaceName, string className);

	CimAsyncMultipleResults<CimClass> EnumerateClassesAsync(string namespaceName, string className, CimOperationOptions options);

	bool TestConnection();

	bool TestConnection(out ICimInstance instance, out CimException exception);

	CimAsyncResult<CimInstance> TestConnectionAsync();
}
