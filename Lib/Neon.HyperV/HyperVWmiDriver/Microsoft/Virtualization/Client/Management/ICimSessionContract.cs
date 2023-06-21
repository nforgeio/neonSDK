using System;
using System.Collections.Generic;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Options;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class ICimSessionContract : ICimSession, IDisposable
{
    public string ComputerName => null;

    public Guid InstanceId => default(Guid);

    public CimSession Session => null;

    public void Close()
    {
    }

    public CimAsyncStatus CloseAsync()
    {
        return null;
    }

    public ICimInstance GetInstance(string namespaceName, ICimInstance instanceId)
    {
        return null;
    }

    public ICimInstance GetInstance(string namespaceName, ICimInstance instanceId, CimOperationOptions options)
    {
        return null;
    }

    public CimAsyncResult<CimInstance> GetInstanceAsync(string namespaceName, ICimInstance instanceId)
    {
        return null;
    }

    public CimAsyncResult<CimInstance> GetInstanceAsync(string namespaceName, ICimInstance instanceId, CimOperationOptions options)
    {
        return null;
    }

    public ICimInstance ModifyInstance(ICimInstance instance)
    {
        return null;
    }

    public ICimInstance ModifyInstance(string namespaceName, ICimInstance instance)
    {
        return null;
    }

    public ICimInstance ModifyInstance(string namespaceName, ICimInstance instance, CimOperationOptions options)
    {
        return null;
    }

    public CimAsyncResult<CimInstance> ModifyInstanceAsync(ICimInstance instance)
    {
        return null;
    }

    public CimAsyncResult<CimInstance> ModifyInstanceAsync(string namespaceName, ICimInstance instance)
    {
        return null;
    }

    public CimAsyncResult<CimInstance> ModifyInstanceAsync(string namespaceName, ICimInstance instance, CimOperationOptions options)
    {
        return null;
    }

    public ICimInstance CreateInstance(string namespaceName, ICimInstance instance)
    {
        return null;
    }

    public ICimInstance CreateInstance(string namespaceName, ICimInstance instance, CimOperationOptions options)
    {
        return null;
    }

    public CimAsyncResult<CimInstance> CreateInstanceAsync(string namespaceName, ICimInstance instance)
    {
        return null;
    }

    public CimAsyncResult<CimInstance> CreateInstanceAsync(string namespaceName, ICimInstance instance, CimOperationOptions options)
    {
        return null;
    }

    public void DeleteInstance(ICimInstance instance)
    {
    }

    public void DeleteInstance(string namespaceName, ICimInstance instance)
    {
    }

    public void DeleteInstance(string namespaceName, ICimInstance instance, CimOperationOptions options)
    {
    }

    public CimAsyncStatus DeleteInstanceAsync(ICimInstance instance)
    {
        return null;
    }

    public CimAsyncStatus DeleteInstanceAsync(string namespaceName, ICimInstance instance)
    {
        return null;
    }

    public CimAsyncStatus DeleteInstanceAsync(string namespaceName, ICimInstance instance, CimOperationOptions options)
    {
        return null;
    }

    public IEnumerable<CimSubscriptionResult> Subscribe(string namespaceName, string queryDialect, string queryExpression)
    {
        return null;
    }

    public IEnumerable<CimSubscriptionResult> Subscribe(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions operationOptions)
    {
        return null;
    }

    public IEnumerable<CimSubscriptionResult> Subscribe(string namespaceName, string queryDialect, string queryExpression, CimSubscriptionDeliveryOptions options)
    {
        return null;
    }

    public IEnumerable<CimSubscriptionResult> Subscribe(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions operationOptions, CimSubscriptionDeliveryOptions options)
    {
        return null;
    }

    public CimAsyncMultipleResults<CimSubscriptionResult> SubscribeAsync(string namespaceName, string queryDialect, string queryExpression)
    {
        return null;
    }

    public CimAsyncMultipleResults<CimSubscriptionResult> SubscribeAsync(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions operationOptions)
    {
        return null;
    }

    public CimAsyncMultipleResults<CimSubscriptionResult> SubscribeAsync(string namespaceName, string queryDialect, string queryExpression, CimSubscriptionDeliveryOptions options)
    {
        return null;
    }

    public CimAsyncMultipleResults<CimSubscriptionResult> SubscribeAsync(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions operationOptions, CimSubscriptionDeliveryOptions options)
    {
        return null;
    }

    public IEnumerable<ICimInstance> EnumerateInstances(string namespaceName, string className)
    {
        return null;
    }

    public IEnumerable<ICimInstance> EnumerateInstances(string namespaceName, string className, CimOperationOptions options)
    {
        return null;
    }

    public CimAsyncMultipleResults<CimInstance> EnumerateInstancesAsync(string namespaceName, string className)
    {
        return null;
    }

    public CimAsyncMultipleResults<CimInstance> EnumerateInstancesAsync(string namespaceName, string className, CimOperationOptions options)
    {
        return null;
    }

    public IEnumerable<ICimInstance> QueryInstances(string namespaceName, string queryDialect, string queryExpression)
    {
        return null;
    }

    public IEnumerable<ICimInstance> QueryInstances(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions options)
    {
        return null;
    }

    public CimAsyncMultipleResults<CimInstance> QueryInstancesAsync(string namespaceName, string queryDialect, string queryExpression)
    {
        return null;
    }

    public CimAsyncMultipleResults<CimInstance> QueryInstancesAsync(string namespaceName, string queryDialect, string queryExpression, CimOperationOptions options)
    {
        return null;
    }

    public IEnumerable<ICimInstance> EnumerateAssociatedInstances(string namespaceName, ICimInstance sourceInstance, string associationClassName, string resultClassName, string sourceRole, string resultRole)
    {
        return null;
    }

    public IEnumerable<ICimInstance> EnumerateAssociatedInstances(string namespaceName, ICimInstance sourceInstance, string associationClassName, string resultClassName, string sourceRole, string resultRole, CimOperationOptions options)
    {
        return null;
    }

    public CimAsyncMultipleResults<CimInstance> EnumerateAssociatedInstancesAsync(string namespaceName, ICimInstance sourceInstance, string associationClassName, string resultClassName, string sourceRole, string resultRole)
    {
        return null;
    }

    public CimAsyncMultipleResults<CimInstance> EnumerateAssociatedInstancesAsync(string namespaceName, ICimInstance sourceInstance, string associationClassName, string resultClassName, string sourceRole, string resultRole, CimOperationOptions options)
    {
        return null;
    }

    public IEnumerable<ICimInstance> EnumerateReferencingInstances(string namespaceName, ICimInstance sourceInstance, string associationClassName, string sourceRole)
    {
        return null;
    }

    public IEnumerable<ICimInstance> EnumerateReferencingInstances(string namespaceName, ICimInstance sourceInstance, string associationClassName, string sourceRole, CimOperationOptions options)
    {
        return null;
    }

    public CimAsyncMultipleResults<CimInstance> EnumerateReferencingInstancesAsync(string namespaceName, ICimInstance sourceInstance, string associationClassName, string sourceRole)
    {
        return null;
    }

    public CimAsyncMultipleResults<CimInstance> EnumerateReferencingInstancesAsync(string namespaceName, ICimInstance sourceInstance, string associationClassName, string sourceRole, CimOperationOptions options)
    {
        return null;
    }

    public CimMethodResult InvokeMethod(ICimInstance instance, string methodName, CimMethodParametersCollection methodParameters)
    {
        return null;
    }

    public CimMethodResult InvokeMethod(string namespaceName, ICimInstance instance, string methodName, CimMethodParametersCollection methodParameters)
    {
        return null;
    }

    public CimMethodResult InvokeMethod(string namespaceName, ICimInstance instance, string methodName, CimMethodParametersCollection methodParameters, CimOperationOptions options)
    {
        return null;
    }

    public CimAsyncResult<CimMethodResult> InvokeMethodAsync(ICimInstance instance, string methodName, CimMethodParametersCollection methodParameters)
    {
        return null;
    }

    public CimAsyncResult<CimMethodResult> InvokeMethodAsync(string namespaceName, ICimInstance instance, string methodName, CimMethodParametersCollection methodParameters)
    {
        return null;
    }

    public CimAsyncMultipleResults<CimMethodResultBase> InvokeMethodAsync(string namespaceName, ICimInstance instance, string methodName, CimMethodParametersCollection methodParameters, CimOperationOptions options)
    {
        return null;
    }

    public CimMethodResult InvokeMethod(string namespaceName, string className, string methodName, CimMethodParametersCollection methodParameters)
    {
        return null;
    }

    public CimMethodResult InvokeMethod(string namespaceName, string className, string methodName, CimMethodParametersCollection methodParameters, CimOperationOptions options)
    {
        return null;
    }

    public CimAsyncResult<CimMethodResult> InvokeMethodAsync(string namespaceName, string className, string methodName, CimMethodParametersCollection methodParameters)
    {
        return null;
    }

    public CimAsyncMultipleResults<CimMethodResultBase> InvokeMethodAsync(string namespaceName, string className, string methodName, CimMethodParametersCollection methodParameters, CimOperationOptions options)
    {
        return null;
    }

    public ICimClass GetClass(string namespaceName, string className)
    {
        return null;
    }

    public ICimClass GetClass(string namespaceName, string className, CimOperationOptions options)
    {
        return null;
    }

    public CimAsyncResult<CimClass> GetClassAsync(string namespaceName, string className)
    {
        return null;
    }

    public CimAsyncResult<CimClass> GetClassAsync(string namespaceName, string className, CimOperationOptions options)
    {
        return null;
    }

    public IEnumerable<ICimClass> EnumerateClasses(string namespaceName)
    {
        return null;
    }

    public IEnumerable<ICimClass> EnumerateClasses(string namespaceName, string className)
    {
        return null;
    }

    public IEnumerable<ICimClass> EnumerateClasses(string namespaceName, string className, CimOperationOptions options)
    {
        return null;
    }

    public CimAsyncMultipleResults<CimClass> EnumerateClassesAsync(string namespaceName)
    {
        return null;
    }

    public CimAsyncMultipleResults<CimClass> EnumerateClassesAsync(string namespaceName, string className)
    {
        return null;
    }

    public CimAsyncMultipleResults<CimClass> EnumerateClassesAsync(string namespaceName, string className, CimOperationOptions options)
    {
        return null;
    }

    public bool TestConnection()
    {
        return false;
    }

    public bool TestConnection(out ICimInstance instance, out CimException exception)
    {
        instance = null;
        exception = null;
        return false;
    }

    public CimAsyncResult<CimInstance> TestConnectionAsync()
    {
        return null;
    }

    public abstract void Dispose();
}
