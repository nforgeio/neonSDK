using System;

namespace Microsoft.Virtualization.Client.Common;

internal class INewVirtualHardDiskWizardTypeAttribute : TypeAttribute
{
    internal INewVirtualHardDiskWizardTypeAttribute(Type implementingType)
        : base(implementingType)
    {
    }
}
