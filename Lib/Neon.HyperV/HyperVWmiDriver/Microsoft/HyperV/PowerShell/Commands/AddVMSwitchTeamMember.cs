using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Add", "VMSwitchTeamMember", DefaultParameterSetName = "SwitchName_NetAdapterName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMSwitch) })]
internal sealed class AddVMSwitchTeamMember : VirtualizationCmdlet<VMSwitch>, ISupportsPassthrough
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "SwitchName_NetAdapterInterfaceDescription", ValueFromPipelineByPropertyName = true)]
    [Parameter(ParameterSetName = "SwitchName_NetAdapterName", ValueFromPipelineByPropertyName = true)]
    [Parameter(ParameterSetName = "SwitchObject_NetAdapterInterfaceDescription", ValueFromPipelineByPropertyName = true)]
    [Parameter(ParameterSetName = "SwitchObject_NetAdapterName", ValueFromPipelineByPropertyName = true)]
    [Alias(new string[] { "PSComputerName" })]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec, and array is more user friendly as this is an user input parameter.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "SwitchObject_NetAdapterName")]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "SwitchObject_NetAdapterInterfaceDescription")]
    public VMSwitch[] VMSwitch { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Array is more user friendly, and this is a user input parameter.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "SwitchName_NetAdapterInterfaceDescription")]
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "SwitchName_NetAdapterName")]
    [Alias(new string[] { "SwitchName" })]
    public string[] VMSwitchName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec, and array is more user friendly as this is an user input parameter.")]
    [ValidateNotNullOrEmpty]
    [Alias(new string[] { "InterfaceAlias" })]
    [Parameter(ParameterSetName = "SwitchName_NetAdapterName")]
    [Parameter(ParameterSetName = "SwitchObject_NetAdapterName")]
    public string[] NetAdapterName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec, and array is more user friendly as this is an user input parameter.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "SwitchName_NetAdapterInterfaceDescription")]
    [Parameter(ParameterSetName = "SwitchObject_NetAdapterInterfaceDescription")]
    public string[] NetAdapterInterfaceDescription { get; set; }

    [Parameter]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is per spec. Also Passthru is a standard PowerShell cmdlet parameter name.")]
    public SwitchParameter Passthru { get; set; }

    internal override IList<VMSwitch> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        IList<VMSwitch> list = ((!IsParameterSpecified("VMSwitch")) ? global::Microsoft.HyperV.PowerShell.VMSwitch.GetSwitchesByNamesAndServers(ParameterResolvers.GetServers(this, operationWatcher), VMSwitchName, allowWildcards: true, ErrorDisplayMode.WriteError, operationWatcher) : VMSwitch);
        if (list != null && list.Any((VMSwitch vmSwitch) => !vmSwitch.EmbeddedTeamingEnabled))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_VMSwitchIsNotTeamingEnabled, list.First((VMSwitch vmSwitch) => !vmSwitch.EmbeddedTeamingEnabled).Name));
        }
        return list;
    }

    internal override void ValidateOperandList(IList<VMSwitch> operands, IOperationWatcher operationWatcher)
    {
        if (operands.Count() > 1)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_AddNicToMultipleVMSwitches);
        }
    }

    internal override void ProcessOneOperand(VMSwitch operand, IOperationWatcher operationWatcher)
    {
        if (!ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMSwitchTeam, operand.Name)))
        {
            return;
        }
        string[] interfaceDescriptionToAdd = NetAdapterInterfaceDescription;
        if (IsParameterSpecified("NetAdapterName"))
        {
            IExternalNetworkPort[] source = NetworkingUtilities.FindExternalNetworkPorts(operand.Server, NetAdapterName, null, Constants.UpdateThreshold);
            interfaceDescriptionToAdd = source.Select((IExternalNetworkPort adapter) => adapter.FriendlyName).ToArray();
        }
        if (operand.NetAdapterInterfaceDescriptions.Any((string intf) => interfaceDescriptionToAdd.Contains(intf, StringComparer.OrdinalIgnoreCase)))
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            string invalidParameter_AddExistingTeamMember = CmdletErrorMessages.InvalidParameter_AddExistingTeamMember;
            object[] args = interfaceDescriptionToAdd;
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(currentCulture, invalidParameter_AddExistingTeamMember, args));
        }
        string[] externalNicDescription = operand.NetAdapterInterfaceDescriptions.Concat(interfaceDescriptionToAdd).ToArray();
        operand.ConfigureConnections(operand.SwitchType, operand.AllowManagementOS, null, externalNicDescription, operationWatcher);
        if (Passthru.IsPresent)
        {
            operationWatcher.WriteObject(operand);
        }
    }
}
