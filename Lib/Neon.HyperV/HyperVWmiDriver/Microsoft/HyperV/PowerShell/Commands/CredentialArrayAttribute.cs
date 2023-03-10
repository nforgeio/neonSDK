using System;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.HyperV.PowerShell.Commands;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
internal sealed class CredentialArrayAttribute : ArgumentTransformationAttribute
{
	private static readonly CredentialAttribute gm_CredentialAttribute = new CredentialAttribute();

	public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
	{
		if (engineIntrinsics == null || engineIntrinsics.Host == null || engineIntrinsics.Host.UI == null)
		{
			throw new ArgumentNullException("engineIntrinsics");
		}
		if (inputData == null || !inputData.GetType().IsArray)
		{
			return new PSCredential[1] { TransformOne(engineIntrinsics, inputData) };
		}
		return ((object[])inputData).Select((object input) => TransformOne(engineIntrinsics, input)).ToArray();
	}

	private static PSCredential TransformOne(EngineIntrinsics engineIntrinsics, object inputData)
	{
		return (PSCredential)gm_CredentialAttribute.Transform(engineIntrinsics, inputData);
	}
}
