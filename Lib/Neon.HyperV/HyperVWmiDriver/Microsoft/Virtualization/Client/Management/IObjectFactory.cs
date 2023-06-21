using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal interface IObjectFactory
{
    T GetVirtualizationManagementObject<T>(ObjectKey key, ICimInstance cimInstance = null, WmiOperationOptions options = null) where T : IVirtualizationManagementObject;

    IList<T> GetVirtualizationManagementObjects<T>(Server server, string wmiNamespace, string wmiClassName, WmiOperationOptions options = null) where T : IVirtualizationManagementObject;

    IList<T> QueryVirtualizationManagementObjects<T>(Server server, string wmiNamespace, string wqlQuery, WmiOperationOptions options = null) where T : IVirtualizationManagementObject;

    bool TryGetVirtualizationManagementObject<T>(ObjectKey key, out T virtManObj) where T : IVirtualizationManagementObject;
}
