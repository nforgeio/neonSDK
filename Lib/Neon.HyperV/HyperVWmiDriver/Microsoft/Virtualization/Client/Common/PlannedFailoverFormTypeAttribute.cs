using System;

namespace Microsoft.Virtualization.Client.Common;

internal class PlannedFailoverFormTypeAttribute : TypeAttribute
{
	internal PlannedFailoverFormTypeAttribute(Type implementingType)
		: base(implementingType)
	{
	}
}
