using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Add", "VMSwitchExtensionSwitchFeature", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMSwitchExtensionSwitchFeature) })]
internal sealed class AddVMSwitchExtensionSwitchFeature : VirtualizationCmdlet<VMSwitch>, ISupportsPassthrough
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

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and array is easier for users to use.")]
    [Parameter(Mandatory = true, ValueFromPipeline = true)]
    [ValidateNotNullOrEmpty]
    public VMSwitchExtensionSwitchFeature[] VMSwitchExtensionFeature { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Array is more user friendly, and this is a user input parameter.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "SwitchName")]
    public string[] SwitchName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec, and array is more user friendly as this is an user input parameter.")]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "SwitchObject")]
    public VMSwitch[] VMSwitch { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    internal override IList<VMSwitch> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        if (CurrentParameterSetIs("SwitchName"))
        {
            return global::Microsoft.HyperV.PowerShell.VMSwitch.GetSwitchesByNamesAndServers(ParameterResolvers.GetServers(this, operationWatcher), SwitchName, allowWildcards: true, ErrorDisplayMode.WriteWarning, operationWatcher);
        }
        return VMSwitch;
    }

    internal override void ProcessOneOperand(VMSwitch operand, IOperationWatcher operationWatcher)
    {
        if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_AddVMSwitchExtensionSwitchFeature, operand.Name)))
        {
            return;
        }
        IEnumerable<VMSwitchExtensionSwitchFeature> enumerable = operand.AddCustomFeatureSettings(VMSwitchExtensionFeature, operationWatcher);
        if (!Passthru.IsPresent)
        {
            return;
        }
        foreach (VMSwitchExtensionSwitchFeature item in enumerable)
        {
            operationWatcher.WriteObject(item);
        }
    }
}
