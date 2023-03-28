using System;

namespace Microsoft.Virtualization.Client.Common;

internal class IRecoveryConfigurationDialogTypeAttribute : TypeAttribute
{
	internal IRecoveryConfigurationDialogTypeAttribute(Type implementingType)
		: base(implementingType)
	{
	}
}
