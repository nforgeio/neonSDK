using System;

namespace Microsoft.Virtualization.Client.Common;

internal class VmComputerSystemStateTypeAttribute : TypeAttribute
{
    internal VmComputerSystemStateTypeAttribute(Type implementingType)
        : base(implementingType)
    {
    }
}
