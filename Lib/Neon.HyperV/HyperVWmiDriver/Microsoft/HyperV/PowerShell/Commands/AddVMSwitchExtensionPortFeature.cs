using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Add", "VMSwitchExtensionPortFeature", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMSwitchExtensionPortFeature) })]
internal sealed class AddVMSwitchExtensionPortFeature : VirtualizationCmdlet<VMNetworkAdapterBase>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, IVMInternalNetworkAdapterCmdlet, IVMNetworkAdapterBaseCmdlet, IVMNetworkAdapterBaseByObjectCmdlet, IVMExternalSwitchPortCmdlet, ISupportsPassthrough
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0)]
    public string[] VMName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and array is easier for users to use.")]
    [Parameter(ParameterSetName = "ResourceObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VMNetworkAdapterBase[] VMNetworkAdapter { get; set; }

    [Parameter(ParameterSetName = "ManagementOS", Mandatory = true)]
    public SwitchParameter ManagementOS { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = "ExternalPort")]
    public SwitchParameter ExternalPort { get; set; }

    [Parameter(ParameterSetName = "ExternalPort")]
    public string SwitchName { get; set; }

    [Parameter(ParameterSetName = "VMObject")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "ManagementOS")]
    public string VMNetworkAdapterName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "ManagementOS")]
    [Parameter(ParameterSetName = "ExternalPort")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "ManagementOS")]
    [Parameter(ParameterSetName = "ExternalPort")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "ManagementOS")]
    [Parameter(ParameterSetName = "ExternalPort")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VirtualMachine[] VM { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and array is easier for users to use.")]
    [Parameter(Mandatory = true, ValueFromPipeline = true)]
    [ValidateNotNullOrEmpty]
    public VMSwitchExtensionPortFeature[] VMSwitchExtensionFeature { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    internal override IList<VMNetworkAdapterBase> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return ParameterResolvers.ResolveNetworkAdapters(this, VMNetworkAdapterName, operationWatcher);
    }

    internal override void ProcessOneOperand(VMNetworkAdapterBase operand, IOperationWatcher operationWatcher)
    {
        if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_AddVMSwitchExtensionPortFeature, operand.Name)))
        {
            return;
        }
        IEnumerable<VMSwitchExtensionPortFeature> enumerable = operand.AddCustomFeatureSettings(VMSwitchExtensionFeature, operationWatcher);
        if (!Passthru.IsPresent)
        {
            return;
        }
        foreach (VMSwitchExtensionPortFeature item in enumerable)
        {
            operationWatcher.WriteObject(item);
        }
    }
}
