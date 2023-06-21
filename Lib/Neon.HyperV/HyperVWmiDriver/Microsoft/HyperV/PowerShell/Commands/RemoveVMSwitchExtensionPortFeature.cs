using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Remove", "VMSwitchExtensionPortFeature", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMSwitchExtensionPortFeature) })]
internal sealed class RemoveVMSwitchExtensionPortFeature : VirtualizationCmdlet<VMSwitchExtensionPortFeature>, ISupportsPassthrough
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and array is easier for users to use.")]
    [Parameter(Mandatory = true, ValueFromPipeline = true)]
    [ValidateNotNullOrEmpty]
    public VMSwitchExtensionPortFeature[] VMSwitchExtensionFeature { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter]
    public string[] VMName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and array is easier for users to use.")]
    [Parameter]
    public VMNetworkAdapterBase[] VMNetworkAdapter { get; set; }

    [Parameter]
    public SwitchParameter ManagementOS { get; set; }

    [Parameter]
    public SwitchParameter ExternalPort { get; set; }

    [Parameter]
    public string SwitchName { get; set; }

    [Parameter]
    public string VMNetworkAdapterName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [Parameter]
    public VirtualMachine[] VM { get; set; }

    internal override IList<VMSwitchExtensionPortFeature> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return VMSwitchExtensionFeature;
    }

    internal override void ProcessOneOperand(VMSwitchExtensionPortFeature operand, IOperationWatcher operationWatcher)
    {
        VMNetworkAdapterBase parentAdapter = operand.ParentAdapter;
        if (parentAdapter == null)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMSwitchExtensionFeature_CannotModifyOrRemoveTemplateFeature, operand.Name));
        }
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RemoveVMSwitchExtensionPortFeature, parentAdapter.Name)))
        {
            ((IRemovable)operand).Remove(operationWatcher);
            if (Passthru.IsPresent)
            {
                operationWatcher.WriteObject(operand);
            }
        }
    }
}
