using System;

namespace Microsoft.Virtualization.Client.Common;

internal class INewVirtualMachineWizardTypeAttribute : TypeAttribute
{
    internal INewVirtualMachineWizardTypeAttribute(Type implementingType)
        : base(implementingType)
    {
    }
}
