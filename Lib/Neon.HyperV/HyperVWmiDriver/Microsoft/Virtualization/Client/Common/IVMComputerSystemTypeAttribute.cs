using System;

namespace Microsoft.Virtualization.Client.Common;

internal class IVMComputerSystemTypeAttribute : TypeAttribute
{
    internal IVMComputerSystemTypeAttribute(Type implementingType)
        : base(implementingType)
    {
    }
}
