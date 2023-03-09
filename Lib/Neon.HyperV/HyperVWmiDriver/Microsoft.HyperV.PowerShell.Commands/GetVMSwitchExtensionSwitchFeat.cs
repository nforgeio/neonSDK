using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMSwitchExtensionSwitchFeature", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMSwitchExtensionSwitchFeature) })]
internal sealed class GetVMSwitchExtensionSwitchFeature : VirtualizationCmdlet<VMSwitchExtensionSwitchFeature>
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "SwitchName")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "SwitchName")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "SwitchName")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Array is more user friendly, and this is a user input parameter.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "SwitchName")]
	public string[] SwitchName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec, and array is more user friendly as this is an user input parameter.")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "SwitchObject")]
	public VMSwitch[] VMSwitch { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and array is easier for users to use.")]
	[Parameter]
	[ValidateNotNullOrEmpty]
	public string[] FeatureName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and array is easier for users to use.")]
	[Parameter]
	[ValidateNotNullOrEmpty]
	public Guid[] FeatureId { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and array is easier for users to use.")]
	[Parameter]
	[ValidateNotNullOrEmpty]
	public VMSwitchExtension[] Extension { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and array is easier for users to use.")]
	[Parameter]
	[ValidateNotNullOrEmpty]
	public string[] ExtensionName { get; set; }

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		ValidateMutuallyExclusiveParameters("FeatureId", "FeatureName");
		ValidateMutuallyExclusiveParameters("Extension", "ExtensionName");
	}

	internal override IList<VMSwitchExtensionSwitchFeature> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		IEnumerable<VMSwitch> inputs = ((!CurrentParameterSetIs("SwitchName")) ? VMSwitch : Microsoft.HyperV.PowerShell.VMSwitch.GetSwitchesByNamesAndServers(ParameterResolvers.GetServers(this, operationWatcher), SwitchName, allowWildcards: true, ErrorDisplayMode.WriteWarning, operationWatcher));
		return VMSwitchExtensionCustomFeature.FilterFeatureSettings(inputs.SelectManyWithLogging((VMSwitch virtualSwitch) => virtualSwitch.GetSwitchFeatures(), operationWatcher), FeatureId, FeatureName, Extension, null, ExtensionName).ToList();
	}

	internal override void ProcessOneOperand(VMSwitchExtensionSwitchFeature operand, IOperationWatcher operationWatcher)
	{
		operationWatcher.WriteObject(operand);
	}
}
