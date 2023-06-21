using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Test", "VMReplicationConnection")]
[OutputType(new Type[] { typeof(string) })]
internal sealed class TestVMReplicationConnection : VirtualizationCmdlet<VMReplicationServer>
{
    [Alias(new string[] { "ReplicaServer" })]
    [ValidateNotNullOrEmpty]
    [Parameter(Position = 0, Mandatory = true)]
    public string ReplicaServerName { get; set; }

    [Alias(new string[] { "ReplicaPort" })]
    [ValidateRange(1, 65535)]
    [Parameter(Position = 1, Mandatory = true)]
    public int ReplicaServerPort { get; set; }

    [Alias(new string[] { "AuthType" })]
    [Parameter(Position = 2, Mandatory = true)]
    public ReplicationAuthenticationType AuthenticationType { get; set; }

    [Alias(new string[] { "Thumbprint" })]
    [ValidateNotNullOrEmpty]
    [Parameter(ValueFromPipelineByPropertyName = true, Position = 3)]
    public string CertificateThumbprint { get; set; }

    [ValidateNotNull]
    [Parameter]
    public bool? BypassProxyServer { get; set; }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();
        if (AuthenticationType == ReplicationAuthenticationType.Certificate)
        {
            if (string.IsNullOrEmpty(CertificateThumbprint))
            {
                throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidArgument_ParameterNotProvided, "CertificateThumbprint"));
            }
        }
        else if (!string.IsNullOrEmpty(CertificateThumbprint))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidArgument_ParameterNotRequired, "CertificateThumbprint", AuthenticationType));
        }
    }

    internal override IList<VMReplicationServer> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return ParameterResolvers.GetServers(this, operationWatcher).SelectWithLogging(VMReplicationServer.GetReplicationServer, operationWatcher).ToList();
    }

    internal override void ProcessOneOperand(VMReplicationServer operand, IOperationWatcher operationWatcher)
    {
        operand.TestReplicationConnection(ReplicaServerName, NumberConverter.Int32ToUInt16(ReplicaServerPort), AuthenticationType, CertificateThumbprint, BypassProxyServer.GetValueOrDefault(), operationWatcher);
        operationWatcher.WriteObject(CmdletResources.TestVMReplicationConnectionSuccessful);
    }
}
