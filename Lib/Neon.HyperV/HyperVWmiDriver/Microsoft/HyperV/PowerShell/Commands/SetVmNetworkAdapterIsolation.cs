using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Set", "VMNetworkAdapterIsolation", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMNetworkAdapterIsolationSetting) })]
internal sealed class SetVmNetworkAdapterIsolation : VirtualizationCmdlet<VMNetworkAdapterBase>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, IVMInternalNetworkAdapterCmdlet, IVMNetworkAdapterBaseCmdlet, IVMNetworkAdapterBaseByObjectCmdlet, ISupportsPassthrough
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and array is easier for users to use.")]
    [Parameter(ParameterSetName = "ResourceObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VMNetworkAdapterBase[] VMNetworkAdapter { get; set; }

    [Parameter(ParameterSetName = "ManagementOS", Mandatory = true)]
    public SwitchParameter ManagementOS { get; set; }

    [Parameter(ParameterSetName = "VMObject")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "ManagementOS")]
    public string VMNetworkAdapterName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "ManagementOS")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "ManagementOS")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "ManagementOS")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public string[] VMName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VirtualMachine[] VM { get; set; }

    [ValidateNotNull]
    [Parameter]
    public VMNetworkAdapterIsolationMode? IsolationMode { get; set; }

    [ValidateNotNull]
    [Parameter]
    public bool? AllowUntaggedTraffic { get; set; }

    [ValidateNotNull]
    [Parameter]
    public int? DefaultIsolationID { get; set; }

    [ValidateNotNull]
    [Parameter]
    public OnOffState? MultiTenantStack { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Passthru { get; set; }

    internal override IList<VMNetworkAdapterBase> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return ParameterResolvers.ResolveNetworkAdapters(this, VMNetworkAdapterName, operationWatcher);
    }

    internal override void ProcessOneOperand(VMNetworkAdapterBase adapter, IOperationWatcher operationWatcher)
    {
        if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMNetworkAdapterIsolation, adapter.Name)))
        {
            return;
        }
        adapter.PrepareForModify(operationWatcher);
        VMNetworkAdapterIsolationSetting isolationSetting = adapter.IsolationSetting;
        if (IsolationMode == VMNetworkAdapterIsolationMode.None)
        {
            if (!isolationSetting.IsTemplate)
            {
                ((IRemovable)isolationSetting).Remove(operationWatcher);
            }
        }
        else if (UpdateIsolationSettings(isolationSetting))
        {
            isolationSetting = adapter.AddOrModifyOneFeatureSetting(isolationSetting, operationWatcher);
            if (Passthru.IsPresent)
            {
                operationWatcher.WriteObject(isolationSetting);
            }
        }
    }

    private bool UpdateIsolationSettings(VMNetworkAdapterIsolationSetting isolation)
    {
        if (!isolation.IsTemplate)
        {
            isolation.ClearSettings();
        }
        bool result = false;
        if (IsolationMode.HasValue)
        {
            isolation.IsolationMode = IsolationMode.Value;
            result = true;
        }
        if (AllowUntaggedTraffic.HasValue)
        {
            isolation.AllowUntaggedTraffic = AllowUntaggedTraffic.Value;
            result = true;
        }
        if (DefaultIsolationID.HasValue)
        {
            isolation.DefaultIsolationID = DefaultIsolationID.Value;
            result = true;
        }
        if (MultiTenantStack.HasValue)
        {
            isolation.MultiTenantStack = MultiTenantStack.Value;
            result = true;
        }
        return result;
    }
}
