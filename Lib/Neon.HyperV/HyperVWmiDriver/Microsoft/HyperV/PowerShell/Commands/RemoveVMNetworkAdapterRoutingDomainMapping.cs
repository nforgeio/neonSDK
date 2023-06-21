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

[Cmdlet("Remove", "VMNetworkAdapterRoutingDomainMapping", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMNetworkAdapterRoutingDomainSetting) })]
internal sealed class RemoveVMNetworkAdapterRoutingDomainMapping : VirtualizationCmdlet<VMNetworkAdapterRoutingDomainSetting>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, IVMInternalNetworkAdapterCmdlet, IVMNetworkAdapterBaseCmdlet, IVMNetworkAdapterBaseByObjectCmdlet, ISupportsPassthrough
{
    private const string InputObjectParameterSet = "InputObjectParameter";

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
    public VMNetworkAdapterRoutingDomainSetting[] InputObject { get; set; }

    [Parameter(ParameterSetName = "VMObject", Mandatory = false)]
    [Parameter(ParameterSetName = "VMName", Mandatory = false)]
    [Parameter(ParameterSetName = "ResourceObject", Mandatory = false)]
    [Parameter(ParameterSetName = "ManagementOS", Mandatory = false)]
    public Guid RoutingDomainID { get; set; }

    [Parameter(ParameterSetName = "VMObject", Mandatory = false)]
    [Parameter(ParameterSetName = "VMName", Mandatory = false)]
    [Parameter(ParameterSetName = "ResourceObject", Mandatory = false)]
    [Parameter(ParameterSetName = "ManagementOS", Mandatory = false)]
    public string RoutingDomainName { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    internal override IList<VMNetworkAdapterRoutingDomainSetting> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        if (CurrentParameterSetIs("InputObjectParameter"))
        {
            return InputObject;
        }
        IEnumerable<VMNetworkAdapterRoutingDomainSetting> source = ParameterResolvers.ResolveNetworkAdapters(this, VMNetworkAdapterName, operationWatcher).SelectManyWithLogging((VMNetworkAdapterBase adapter) => adapter.RoutingDomainList, operationWatcher);
        if (RoutingDomainID != Guid.Empty)
        {
            source = source.Where((VMNetworkAdapterRoutingDomainSetting routingDomain) => routingDomain.RoutingDomainID == RoutingDomainID);
        }
        if (!string.IsNullOrEmpty(RoutingDomainName))
        {
            source = source.Where((VMNetworkAdapterRoutingDomainSetting routingDomain) => string.Equals(routingDomain.RoutingDomainName, RoutingDomainName, StringComparison.OrdinalIgnoreCase));
        }
        List<VMNetworkAdapterRoutingDomainSetting> list = source.ToList();
        if (list.Count == 0)
        {
            throw ExceptionHelper.CreateObjectNotFoundException(CmdletErrorMessages.RouteDomainMapping_NoneFound, null);
        }
        return list;
    }

    internal override void ProcessOneOperand(VMNetworkAdapterRoutingDomainSetting operand, IOperationWatcher operationWatcher)
    {
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RemoveVMNetworkAdapterRoutingDomainMapping, operand.ParentAdapter.Name)))
        {
            ((IRemovable)operand).Remove(operationWatcher);
            if (Passthru.IsPresent)
            {
                operationWatcher.WriteObject(operand);
            }
        }
    }
}
