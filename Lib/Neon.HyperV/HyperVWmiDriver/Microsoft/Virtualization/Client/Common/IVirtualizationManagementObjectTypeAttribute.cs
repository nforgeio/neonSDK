using System;

namespace Microsoft.Virtualization.Client.Common;

internal class IVirtualizationManagementObjectTypeAttribute : TypeAttribute
{
	internal IVirtualizationManagementObjectTypeAttribute(Type implementingType)
		: base(implementingType)
	{
	}
}
