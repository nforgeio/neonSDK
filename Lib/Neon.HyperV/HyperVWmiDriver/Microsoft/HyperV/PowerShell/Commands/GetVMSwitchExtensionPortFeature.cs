using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMSwitchExtensionPortFeature", DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMSwitchExtensionPortFeature) })]
internal sealed class GetVMSwitchExtensionPortFeature : VirtualizationCmdlet<VMSwitchExtensionPortFeature>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, IVMInternalNetworkAdapterCmdlet, IVMNetworkAdapterBaseCmdlet, IVMNetworkAdapterBaseByObjectCmdlet, IVMExternalSwitchPortCmdlet
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

    internal override IList<VMSwitchExtensionPortFeature> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return VMSwitchExtensionCustomFeature.FilterFeatureSettings(ParameterResolvers.ResolveNetworkAdapters(this, VMNetworkAdapterName, operationWatcher).SelectManyWithLogging((VMNetworkAdapterBase adapter) => adapter.GetPortFeatures(), operationWatcher), FeatureId, FeatureName, Extension, null, ExtensionName).ToList();
    }

    internal override void ProcessOneOperand(VMSwitchExtensionPortFeature operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}
