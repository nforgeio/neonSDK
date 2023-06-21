using System;

namespace Microsoft.Virtualization.Client.Common;

internal class InitializeReplicationFormTypeAttribute : TypeAttribute
{
    internal InitializeReplicationFormTypeAttribute(Type implementingType)
        : base(implementingType)
    {
    }
}
