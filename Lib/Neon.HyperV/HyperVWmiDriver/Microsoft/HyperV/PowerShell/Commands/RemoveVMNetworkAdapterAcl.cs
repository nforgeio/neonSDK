using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Remove", "VMNetworkAdapterAcl", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMNetworkAdapterAclSetting) })]
internal sealed class RemoveVMNetworkAdapterAcl : VirtualizationCmdlet<VMNetworkAdapterAclSetting>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, IVMInternalNetworkAdapterCmdlet, IVMNetworkAdapterBaseCmdlet, IVMNetworkAdapterBaseByObjectCmdlet, IVMNetworkAdapterAclCmdlet, ISupportsPassthrough
{
    private const string InputObjectParameterSet = "InputObjectParameter";

    private VMNetworkAdapterAclAddresses m_Addresses;

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

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and array is easier for users to use.")]
    [Parameter(ParameterSetName = "InputObjectParameter", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Position = 0, Mandatory = true)]
    public VMNetworkAdapterAclSetting[] InputObject { get; set; }

    [Parameter(ParameterSetName = "VMObject", Mandatory = true)]
    [Parameter(ParameterSetName = "VMName", Mandatory = true)]
    [Parameter(ParameterSetName = "ResourceObject", Mandatory = true)]
    [Parameter(ParameterSetName = "ManagementOS", Mandatory = true)]
    public VMNetworkAdapterAclAction Action { get; set; }

    [Parameter(ParameterSetName = "VMObject", Mandatory = true)]
    [Parameter(ParameterSetName = "VMName", Mandatory = true)]
    [Parameter(ParameterSetName = "ResourceObject", Mandatory = true)]
    [Parameter(ParameterSetName = "ManagementOS", Mandatory = true)]
    public VMNetworkAdapterAclDirection Direction { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "ResourceObject")]
    [Parameter(ParameterSetName = "ManagementOS")]
    public string[] LocalIPAddress { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "ResourceObject")]
    [Parameter(ParameterSetName = "ManagementOS")]
    public string[] LocalMacAddress { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "ResourceObject")]
    [Parameter(ParameterSetName = "ManagementOS")]
    public string[] RemoteIPAddress { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject")]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "ResourceObject")]
    [Parameter(ParameterSetName = "ManagementOS")]
    public string[] RemoteMacAddress { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    protected override void NormalizeParameters()
    {
        base.NormalizeParameters();
        if (!CurrentParameterSetIs("InputObjectParameter"))
        {
            m_Addresses = VMNetworkAdapterAclAddresses.OrganizeAddressList(this);
        }
    }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();
        if (base.ParameterSetName != "InputObjectParameter")
        {
            VMNetworkAdapterAclAddresses.ValidateParameterCount(this);
        }
    }

    internal override IList<VMNetworkAdapterAclSetting> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        if (CurrentParameterSetIs("InputObjectParameter"))
        {
            return InputObject;
        }
        return ParameterResolvers.ResolveNetworkAdapters(this, VMNetworkAdapterName, operationWatcher).SelectManyWithLogging((VMNetworkAdapterBase adapter) => adapter.AclList, operationWatcher).Where(IsMatchingAcl)
            .ToList();
    }

    internal override void ProcessOneOperand(VMNetworkAdapterAclSetting operand, IOperationWatcher operationWatcher)
    {
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RemoveVMNetworkAdapterAcl, operand.ParentAdapter.Name)))
        {
            ((IRemovable)operand).Remove(operationWatcher);
            if (Passthru.IsPresent)
            {
                operationWatcher.WriteObject(operand);
            }
        }
    }

    private bool IsMatchingAcl(VMNetworkAdapterAclSetting acl)
    {
        if (acl.Action != Action)
        {
            return false;
        }
        if (acl.Direction != Direction && Direction != VMNetworkAdapterAclDirection.Both)
        {
            return false;
        }
        if (acl.IsMacAddress != m_Addresses.IsMacAddress)
        {
            return false;
        }
        string value = (m_Addresses.IsRemote ? acl.RemoteAddress : acl.LocalAddress);
        if (!m_Addresses.AddressList.Contains(value, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }
        return true;
    }
}
