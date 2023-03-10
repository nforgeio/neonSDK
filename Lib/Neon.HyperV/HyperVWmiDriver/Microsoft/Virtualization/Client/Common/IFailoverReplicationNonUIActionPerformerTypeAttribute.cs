using System;

namespace Microsoft.Virtualization.Client.Common;

internal class IFailoverReplicationNonUIActionPerformerTypeAttribute : TypeAttribute
{
	internal IFailoverReplicationNonUIActionPerformerTypeAttribute(Type implementingType)
		: base(implementingType)
	{
	}
}
