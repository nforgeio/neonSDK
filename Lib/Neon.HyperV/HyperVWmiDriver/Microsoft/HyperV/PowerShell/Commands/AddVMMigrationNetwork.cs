using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using System.Net;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Add", "VMMigrationNetwork", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMMigrationNetwork) })]
internal sealed class AddVMMigrationNetwork : VirtualizationCmdlet<VMHost>, ISupportsPassthrough
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [Parameter(ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
    public string Subnet { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Position = 1)]
    public uint Priority { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();
        NetworkUtilities.ParseSubnetString(Subnet, out var address, out var _);
        IPAddress iPAddress = IPAddress.Parse(address);
        if (IPAddress.IsLoopback(iPAddress))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.MigrationNetworkUnacceptableLoopback);
        }
        if (VMHost.IsApipaAddress(iPAddress))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.MigrationNetworkUnacceptableLinkLocal);
        }
    }

    internal override IList<VMHost> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return VirtualizationObjectLocator.GetVMHosts(ParameterResolvers.GetServers(this, operationWatcher), operationWatcher);
    }

    internal override void ProcessOneOperand(VMHost operand, IOperationWatcher operationWatcher)
    {
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_AddVMMigrationNetwork, Subnet, operand.Server)))
        {
            if (!operand.GetIsLiveMigrationSupported())
            {
                throw ExceptionHelper.CreateOperationFailedException(CmdletErrorMessages.OperationFailed_HostDoesNotSupportLM);
            }
            if (operand.GetUserManagedVMMigrationNetworks(new string[1] { Subnet }, null).Count != 0)
            {
                throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.SubnetAlreadyExists, Subnet));
            }
            VMMigrationNetwork output = operand.AddUserManagedMigrationNetwork(Subnet, IsParameterSpecified("Priority") ? new uint?(Priority) : null);
            if ((bool)Passthru)
            {
                operationWatcher.WriteObject(output);
            }
        }
    }
}
