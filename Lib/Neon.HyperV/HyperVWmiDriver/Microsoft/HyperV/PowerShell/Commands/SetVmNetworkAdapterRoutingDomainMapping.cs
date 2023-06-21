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

[Cmdlet("Set", "VMNetworkAdapterRoutingDomainMapping", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMNetworkAdapterRoutingDomainSetting) })]
internal sealed class SetVmNetworkAdapterRoutingDomainMapping : VirtualizationCmdlet<VMNetworkAdapterRoutingDomainSetting>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, IVMInternalNetworkAdapterCmdlet, IVMNetworkAdapterBaseCmdlet, IVMNetworkAdapterBaseByObjectCmdlet, ISupportsPassthrough
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
    public VMNetworkAdapterRoutingDomainSetting InputObject { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject", Mandatory = false)]
    [Parameter(ParameterSetName = "VMName", Mandatory = false)]
    [Parameter(ParameterSetName = "ResourceObject", Mandatory = false)]
    [Parameter(ParameterSetName = "ManagementOS", Mandatory = false)]
    public Guid RoutingDomainID { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject", Mandatory = false)]
    [Parameter(ParameterSetName = "VMName", Mandatory = false)]
    [Parameter(ParameterSetName = "ResourceObject", Mandatory = false)]
    [Parameter(ParameterSetName = "ManagementOS", Mandatory = false)]
    public string RoutingDomainName { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = false)]
    public string NewRoutingDomainName { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = false)]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    public int[] IsolationID { get; set; }

    [Parameter(Mandatory = false)]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    public string[] IsolationName { get; set; }

    [Parameter(Mandatory = false)]
    public SwitchParameter Passthru { get; set; }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();
        if (RoutingDomainID == Guid.Empty && string.IsNullOrEmpty(RoutingDomainName) && InputObject == null)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_SetRoutingDomainCriteriaMissing);
        }
        if (NewRoutingDomainName == null && IsolationID == null && IsolationName == null)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.NoParametersSpecified);
        }
    }

    internal override IList<VMNetworkAdapterRoutingDomainSetting> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        if (!CurrentParameterSetIs("InputObjectParameter"))
        {
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
        return new VMNetworkAdapterRoutingDomainSetting[1] { InputObject };
    }

    internal override void ProcessOneOperand(VMNetworkAdapterRoutingDomainSetting routingDomain, IOperationWatcher operationWatcher)
    {
        VMNetworkAdapterBase parentAdapter = routingDomain.ParentAdapter;
        if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMNetworkAdapterRoutingDomainMapping, parentAdapter.Name)))
        {
            return;
        }
        if (parentAdapter.CurrentIsolationMode == VMNetworkAdapterIsolationMode.None)
        {
            throw ExceptionHelper.CreateInvalidStateException(CmdletErrorMessages.VMNetworkAdapterRoutingDomainMapping_IsolationModeDoesNotSupportRoutingDomain, null, parentAdapter);
        }
        if (!string.IsNullOrEmpty(NewRoutingDomainName))
        {
            routingDomain.RoutingDomainName = NewRoutingDomainName;
        }
        if (IsolationID != null)
        {
            routingDomain.IsolationID = IsolationID;
            if (IsolationName == null)
            {
                routingDomain.IsolationName = new string[IsolationID.Length];
            }
        }
        if (IsolationName != null)
        {
            routingDomain.IsolationName = IsolationName;
        }
        ((IUpdatable)routingDomain).Put(operationWatcher);
        if (Passthru.IsPresent)
        {
            operationWatcher.WriteObject(routingDomain);
        }
    }
}
