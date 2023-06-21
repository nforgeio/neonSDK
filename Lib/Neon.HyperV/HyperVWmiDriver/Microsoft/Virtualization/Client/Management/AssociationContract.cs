using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class AssociationContract : Association
{
    protected override IEnumerable<ICimInstance> GetRelatedObjectsSelf(Server server, WmiObjectPath wmiObjectPath, WmiOperationOptions options)
    {
        return null;
    }
}
