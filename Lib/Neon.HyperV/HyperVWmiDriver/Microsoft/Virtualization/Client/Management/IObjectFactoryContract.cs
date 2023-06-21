using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IObjectFactoryContract : IObjectFactory
{
    T IObjectFactory.GetVirtualizationManagementObject<T>(ObjectKey key, ICimInstance cimInstance, WmiOperationOptions options)
    {
        return default(T);
    }

    IList<T> IObjectFactory.GetVirtualizationManagementObjects<T>(Server server, string wmiNamespace, string wmiClassName, WmiOperationOptions options)
    {
        return null;
    }

    IList<T> IObjectFactory.QueryVirtualizationManagementObjects<T>(Server server, string wmiNamespace, string wqlQuery, WmiOperationOptions options)
    {
        return null;
    }

    bool IObjectFactory.TryGetVirtualizationManagementObject<T>(ObjectKey key, out T virtManObj)
    {
        virtManObj = default(T);
        return false;
    }
}
