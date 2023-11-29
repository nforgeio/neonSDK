using System;

namespace Microsoft.Virtualization.Client.Common;

internal class ObjectLocatorTypeAttribute : TypeAttribute
{
    internal ObjectLocatorTypeAttribute(Type implementingType)
        : base(implementingType)
    {
    }
}
