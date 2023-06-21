using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Add", "VMNetworkAdapterExtendedAcl", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMNetworkAdapterExtendedAclSetting) })]
internal sealed class AddVMNetworkAdapterExtendedAcl : VirtualizationCmdlet<VMNetworkAdapterBase>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, IVMInternalNetworkAdapterCmdlet, IVMNetworkAdapterBaseCmdlet, IVMNetworkAdapterBaseByObjectCmdlet, ISupportsPassthrough
{
    [Parameter(Position = 1, Mandatory = true)]
    public VMNetworkAdapterExtendedAclAction Action { get; set; }

    [Parameter(Position = 2, Mandatory = true)]
    public VMNetworkAdapterExtendedAclDirection Direction { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Position = 3)]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    public string LocalIPAddress { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Position = 4)]
    public string RemoteIPAddress { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Position = 5)]
    public string LocalPort { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Position = 6)]
    public string RemotePort { get; set; }

    [ValidateNotNull]
    [Parameter(Position = 7)]
    public string Protocol { get; set; }

    [ValidateNotNull]
    [Parameter(Position = 8, Mandatory = true)]
    public int? Weight { get; set; }

    [ValidateNotNull]
    [Parameter]
    public bool? Stateful { get; set; }

    [ValidateNotNull]
    [Parameter]
    public int? IdleSessionTimeout { get; set; }

    [ValidateNotNull]
    [Parameter]
    public int? IsolationID { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

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

    internal override IList<VMNetworkAdapterBase> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return ParameterResolvers.ResolveNetworkAdapters(this, VMNetworkAdapterName, operationWatcher);
    }

    internal override void ProcessOneOperand(VMNetworkAdapterBase operand, IOperationWatcher operationWatcher)
    {
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_AddVMNetworkAdapterExtendedAcl, operand.Name)))
        {
            VMNetworkAdapterExtendedAclSetting vMNetworkAdapterExtendedAclSetting = VMNetworkAdapterExtendedAclSetting.CreateTemplateExtendedAclSetting(operand);
            ConfigureExtendedAcl(vMNetworkAdapterExtendedAclSetting);
            VMNetworkAdapterExtendedAclSetting output = operand.AddOrModifyOneFeatureSetting(vMNetworkAdapterExtendedAclSetting, operationWatcher);
            if (Passthru.IsPresent)
            {
                operationWatcher.WriteObject(output);
            }
        }
    }

    private void ConfigureExtendedAcl(VMNetworkAdapterExtendedAclSetting extendedAcl)
    {
        extendedAcl.Direction = Direction;
        extendedAcl.Action = Action;
        if (LocalIPAddress != null)
        {
            extendedAcl.LocalIPAddress = LocalIPAddress;
        }
        if (RemoteIPAddress != null)
        {
            extendedAcl.RemoteIPAddress = RemoteIPAddress;
        }
        if (LocalPort != null)
        {
            extendedAcl.LocalPort = LocalPort;
        }
        if (RemotePort != null)
        {
            extendedAcl.RemotePort = RemotePort;
        }
        if (Protocol != null)
        {
            extendedAcl.Protocol = Protocol;
        }
        if (Weight.HasValue)
        {
            extendedAcl.Weight = Weight.Value;
        }
        if (Stateful.HasValue)
        {
            extendedAcl.Stateful = Stateful.Value;
        }
        if (IdleSessionTimeout.HasValue)
        {
            extendedAcl.IdleSessionTimeout = IdleSessionTimeout.Value;
        }
        else if (!Stateful.GetValueOrDefault())
        {
            extendedAcl.IdleSessionTimeout = 0;
        }
        if (IsolationID.HasValue)
        {
            extendedAcl.IsolationID = IsolationID.Value;
        }
    }
}
