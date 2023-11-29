using System;

namespace Microsoft.Virtualization.Client.Common;

internal class SettingIdTypeAttribute : TypeAttribute
{
    internal SettingIdTypeAttribute(Type implementingType)
        : base(implementingType)
    {
    }
}
