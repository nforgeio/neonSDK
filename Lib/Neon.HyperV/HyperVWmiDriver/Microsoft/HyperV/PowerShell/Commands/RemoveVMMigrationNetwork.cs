using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Remove", "VMMigrationNetwork", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMMigrationNetwork) })]
internal sealed class RemoveVMMigrationNetwork : VirtualizationCmdlet<VMMigrationNetwork>, ISupportsPassthrough
{
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    public string Subnet { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ValueFromPipelineByPropertyName = true)]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    internal override IList<VMMigrationNetwork> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        string[] subnetFilter = ((!IsParameterSpecified("Subnet")) ? null : new string[1] { Subnet });
        List<VMMigrationNetwork> list = VirtualizationObjectLocator.GetVMHosts(ParameterResolvers.GetServers(this, operationWatcher), operationWatcher).SelectMany((VMHost service) => service.GetUserManagedVMMigrationNetworks(subnetFilter, null)).ToList();
        if (list.Count == 0)
        {
            throw ExceptionHelper.CreateObjectNotFoundException(CmdletErrorMessages.GetVMMigrationNetwork_NoMigrationNetworksFound, null);
        }
        return list;
    }

    internal override void ProcessOneOperand(VMMigrationNetwork operand, IOperationWatcher operationWatcher)
    {
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RemoveVMMigrationNetwork, Subnet, operand.Server)))
        {
            VirtualizationObjectLocator.GetVMHost(operand.Server).RemoveMigrationNetwork(operand);
            if ((bool)Passthru)
            {
                operationWatcher.WriteObject(operand);
            }
        }
    }
}
