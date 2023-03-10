using System;

namespace Microsoft.Virtualization.Client.Common;

internal class RemoveReplicationFormTypeAttribute : TypeAttribute
{
	internal RemoveReplicationFormTypeAttribute(Type implementingType)
		: base(implementingType)
	{
	}
}
