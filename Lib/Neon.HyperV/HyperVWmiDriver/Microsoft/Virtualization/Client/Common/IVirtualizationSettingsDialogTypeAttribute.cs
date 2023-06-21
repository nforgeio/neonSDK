using System;

namespace Microsoft.Virtualization.Client.Common;

internal class IVirtualizationSettingsDialogTypeAttribute : TypeAttribute
{
    internal IVirtualizationSettingsDialogTypeAttribute(Type implementingType)
        : base(implementingType)
    {
    }
}
