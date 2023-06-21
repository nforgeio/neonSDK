using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Set", "VMSwitchExtensionSwitchFeature", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMSwitchExtensionSwitchFeature) })]
internal sealed class SetVMSwitchExtensionSwitchFeature : VirtualizationCmdlet<VMSwitchExtensionSwitchFeature>, ISupportsPassthrough
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and array is easier for users to use.")]
    [Parameter(Mandatory = true, ValueFromPipeline = true)]
    [ValidateNotNullOrEmpty]
    public VMSwitchExtensionSwitchFeature[] VMSwitchExtensionFeature { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Array is more user friendly, and this is a user input parameter.")]
    [Parameter]
    public string[] SwitchName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec, and array is more user friendly as this is an user input parameter.")]
    [Parameter]
    public VMSwitch[] VMSwitch { get; set; }

    internal override IList<VMSwitchExtensionSwitchFeature> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return VMSwitchExtensionFeature;
    }

    internal override void ProcessOneOperand(VMSwitchExtensionSwitchFeature operand, IOperationWatcher operationWatcher)
    {
        VMSwitch parentSwitch = operand.ParentSwitch;
        if (parentSwitch == null)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMSwitchExtensionFeature_CannotModifyOrRemoveTemplateFeature, operand.Name));
        }
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMSwitchExtensionSwitchFeature, parentSwitch.Name)))
        {
            ((IUpdatable)operand).Put(operationWatcher);
            if (Passthru.IsPresent)
            {
                operationWatcher.WriteObject(operand);
            }
        }
    }
}
