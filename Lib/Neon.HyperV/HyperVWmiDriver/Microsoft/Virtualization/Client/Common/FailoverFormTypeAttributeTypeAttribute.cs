using System;

namespace Microsoft.Virtualization.Client.Common;

internal class FailoverFormTypeAttributeTypeAttribute : TypeAttribute
{
    internal FailoverFormTypeAttributeTypeAttribute(Type implementingType)
        : base(implementingType)
    {
    }
}
