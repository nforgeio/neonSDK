using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Set", "VMSan", SupportsShouldProcess = true, DefaultParameterSetName = "HbaObject")]
[OutputType(new Type[] { typeof(VMSan) })]
internal sealed class SetVMSan : VirtualizationCmdlet<VMSan>, IVMSanCmdlet, IParameterSet, ISupportsPassthrough
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "StringWwn", ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "StringWwn")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "StringWwn")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    [Alias(new string[] { "SanName" })]
    public string Name { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [Parameter(ParameterSetName = "HbaObject")]
    [ValidateNotNullOrEmpty]
    public CimInstance[] HostBusAdapter { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [Parameter(ParameterSetName = "StringWwn", Mandatory = true)]
    [ValidateNotNullOrEmpty]
    [Alias(new string[] { "Wwnn", "NodeName", "Wwnns", "NodeNames", "WorldWideNodeNames", "NodeAddress" })]
    public string[] WorldWideNodeName { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [Parameter(ParameterSetName = "StringWwn", Mandatory = true)]
    [ValidateNotNullOrEmpty]
    [Alias(new string[] { "Wwpn", "PortName", "Wwpns", "PortNames", "WorldWidePortNames", "PortAddress" })]
    public string[] WorldWidePortName { get; set; }

    [Parameter]
    [ValidateNotNull]
    public string Note { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();
        ParameterValidator.ValidateHbaParameters(this);
    }

    internal override IList<VMSan> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        string[] sanNames = new string[1] { Name };
        List<VMSan> list = ParameterResolvers.GetServers(this, operationWatcher).SelectManyWithLogging((Server server) => VMSan.GetVMSans(server, sanNames, allowWildcards: false), operationWatcher).ToList();
        if (list.Count == 0)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.GetVMSan_NoneFound);
        }
        return list;
    }

    internal override void ProcessOneOperand(VMSan operand, IOperationWatcher operationWatcher)
    {
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMSan, operand.Name)))
        {
            if (IsParameterSpecified("HostBusAdapter") || IsParameterSpecified("WorldWideNodeName") || IsParameterSpecified("WorldWidePortName"))
            {
                ParameterResolvers.GetHbaNames(this, out var nodeWwns, out var portWwns);
                operand.SetHbas(nodeWwns, portWwns, operationWatcher);
            }
            if (IsParameterSpecified("Note"))
            {
                operand.Note = Note;
                ((IUpdatable)operand).Put(operationWatcher);
            }
            if (Passthru.IsPresent)
            {
                operationWatcher.WriteObject(operand);
            }
        }
    }
}
