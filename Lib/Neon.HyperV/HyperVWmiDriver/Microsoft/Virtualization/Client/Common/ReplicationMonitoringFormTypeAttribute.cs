using System;

namespace Microsoft.Virtualization.Client.Common;

internal class ReplicationMonitoringFormTypeAttribute : TypeAttribute
{
    internal ReplicationMonitoringFormTypeAttribute(Type implementingType)
        : base(implementingType)
    {
    }
}
