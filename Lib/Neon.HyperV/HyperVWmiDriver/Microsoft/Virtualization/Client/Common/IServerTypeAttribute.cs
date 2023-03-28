using System;

namespace Microsoft.Virtualization.Client.Common;

internal class IServerTypeAttribute : TypeAttribute
{
	internal IServerTypeAttribute(Type implementingType)
		: base(implementingType)
	{
	}
}
