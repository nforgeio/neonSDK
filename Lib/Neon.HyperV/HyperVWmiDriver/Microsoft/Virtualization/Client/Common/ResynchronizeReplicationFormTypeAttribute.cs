using System;

namespace Microsoft.Virtualization.Client.Common;

internal class ResynchronizeReplicationFormTypeAttribute : TypeAttribute
{
    internal ResynchronizeReplicationFormTypeAttribute(Type implementingType)
        : base(implementingType)
    {
    }
}
