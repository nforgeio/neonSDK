using System;

namespace Microsoft.Virtualization.Client.Common;

internal class IServerNodeTypeAttribute : TypeAttribute
{
    internal IServerNodeTypeAttribute(Type implementingType)
        : base(implementingType)
    {
    }
}
