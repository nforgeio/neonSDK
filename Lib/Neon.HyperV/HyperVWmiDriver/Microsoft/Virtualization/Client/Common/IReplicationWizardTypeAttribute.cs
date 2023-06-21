using System;

namespace Microsoft.Virtualization.Client.Common;

internal class IReplicationWizardTypeAttribute : TypeAttribute
{
    internal IReplicationWizardTypeAttribute(Type implementingType)
        : base(implementingType)
    {
    }
}
