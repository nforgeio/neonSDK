using System;

namespace Microsoft.Virtualization.Client.Common;

internal class ImportReplicationFormTypeAttribute : TypeAttribute
{
    internal ImportReplicationFormTypeAttribute(Type implementingType)
        : base(implementingType)
    {
    }
}
