using System;

namespace Microsoft.Virtualization.Client.Common;

internal class IVmSettingsDialogTypeAttribute : TypeAttribute
{
	internal IVmSettingsDialogTypeAttribute(Type implementingType)
		: base(implementingType)
	{
	}
}
