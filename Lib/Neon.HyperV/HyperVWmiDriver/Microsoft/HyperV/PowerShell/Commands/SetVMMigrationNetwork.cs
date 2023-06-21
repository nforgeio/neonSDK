using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Net;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Set", "VMMigrationNetwork", SupportsShouldProcess = true, DefaultParameterSetName = "ComputerName")]
[OutputType(new Type[] { typeof(VMMigrationNetwork) })]
internal sealed class SetVMMigrationNetwork : VirtualizationCmdlet<VMHost>, ISupportsPassthrough
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "CimSession")]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "ComputerName")]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "ComputerName")]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    public string Subnet { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Position = 1)]
    public string NewSubnet { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter]
    public uint NewPriority { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();
        if (!IsParameterSpecified("NewSubnet") && !IsParameterSpecified("NewPriority"))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidArgument_SetVMMigrationNetwork);
        }
        if (IsParameterSpecified("NewSubnet"))
        {
            NetworkUtilities.ParseSubnetString(NewSubnet, out var address, out var _);
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
    }

    internal override IList<VMHost> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return ParameterResolvers.GetServers(this, operationWatcher).SelectWithLogging(GetLiveMigrationCapableHostOrThrow, operationWatcher).ToList();
    }

    internal override void ProcessOneOperand(VMHost host, IOperationWatcher operationWatcher)
    {
        if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMMigrationNetwork, Subnet, host.Server)))
        {
            return;
        }
        VMMigrationNetwork vMMigrationNetwork = GetSingleVMMigrationNetworkMatchOrThrow(host);
        uint? priority = (IsParameterSpecified("NewPriority") ? new uint?(NewPriority) : null);
        if (IsParameterSpecified("NewSubnet"))
        {
            if (host.GetUserManagedVMMigrationNetworks(new string[1] { NewSubnet }, null).Count != 0)
            {
                throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.SubnetAlreadyExists, Subnet));
            }
            VMMigrationNetwork vMMigrationNetwork2 = host.AddUserManagedMigrationNetwork(NewSubnet, priority);
            host.RemoveMigrationNetwork(vMMigrationNetwork);
            vMMigrationNetwork = vMMigrationNetwork2;
        }
        else if (priority.HasValue)
        {
            vMMigrationNetwork.SetPriority(NewPriority);
            ((IUpdatable)vMMigrationNetwork).Put(operationWatcher);
        }
        if ((bool)Passthru)
        {
            operationWatcher.WriteObject(vMMigrationNetwork);
        }
    }

    private static VMHost GetLiveMigrationCapableHostOrThrow(Server server)
    {
        VMHost vMHost = VirtualizationObjectLocator.GetVMHost(server);
        if (!vMHost.GetIsLiveMigrationSupported())
        {
            throw ExceptionHelper.CreateOperationFailedException(CmdletErrorMessages.OperationFailed_HostDoesNotSupportLM);
        }
        return vMHost;
    }

    private VMMigrationNetwork GetSingleVMMigrationNetworkMatchOrThrow(VMHost host)
    {
        List<VMMigrationNetwork> list = host.GetUserManagedVMMigrationNetworks(new string[1] { Subnet }, null).ToList();
        if (list.Count > 1)
        {
            throw ExceptionHelper.CreateOperationFailedException(CmdletErrorMessages.OperationFailed_CannotEditMultipleSubnets);
        }
        if (list.Count == 0)
        {
            throw ExceptionHelper.CreateObjectNotFoundException(CmdletErrorMessages.GetVMMigrationNetwork_NoMigrationNetworksFound, null);
        }
        return list.First();
    }
}
