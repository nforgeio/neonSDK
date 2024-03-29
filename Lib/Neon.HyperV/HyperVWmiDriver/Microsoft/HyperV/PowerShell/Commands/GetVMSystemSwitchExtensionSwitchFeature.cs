using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.ExtensionMethods;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMSystemSwitchExtensionSwitchFeature")]
[OutputType(new Type[] { typeof(VMSwitchExtensionSwitchFeature) })]
internal sealed class GetVMSystemSwitchExtensionSwitchFeature : VirtualizationCmdlet<VMSwitchExtensionSwitchFeature>
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and array is easier for users to use.")]
    [Parameter]
    public string[] FeatureName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and array is easier for users to use.")]
    [Parameter]
    public Guid[] FeatureId { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and array is easier for users to use.")]
    [Parameter]
    public string[] ExtensionName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and array is easier for users to use.")]
    [Parameter]
    public VMSystemSwitchExtension[] SystemSwitchExtension { get; set; }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();
        ValidateMutuallyExclusiveParameters("FeatureId", "FeatureName");
        ValidateMutuallyExclusiveParameters("SystemSwitchExtension", "ExtensionName");
    }

    internal override IList<VMSwitchExtensionSwitchFeature> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return VMSwitchExtensionCustomFeature.FilterFeatureSettings(ParameterResolvers.GetServers(this, operationWatcher).SelectManyWithLogging(VMSwitchExtensionSwitchFeature.GetTemplateSwitchFeatures, operationWatcher), FeatureId, FeatureName, null, SystemSwitchExtension, ExtensionName).ToList();
    }

    internal override void ProcessOneOperand(VMSwitchExtensionSwitchFeature operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}
