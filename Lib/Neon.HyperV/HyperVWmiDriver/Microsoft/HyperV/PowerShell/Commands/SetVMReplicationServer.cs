using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Set", "VMReplicationServer", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true, DefaultParameterSetName = "AuthenticationPort")]
[OutputType(new Type[] { typeof(VMReplicationServer) })]
internal sealed class SetVMReplicationServer : VirtualizationCmdlet<VMReplicationServer>, ISupportsPassthrough, ISupportsForce
{
	private static class ParameterSetNames
	{
		public const string AuthenticationPort = "AuthenticationPort";

		public const string AuthenticationPortMapping = "AuthenticationPortMapping";
	}

	[Alias(new string[] { "RepEnabled" })]
	[ValidateNotNull]
	[Parameter(Position = 0)]
	public bool? ReplicationEnabled { get; set; }

	[Alias(new string[] { "AuthType" })]
	[ValidateNotNull]
	[Parameter(Position = 1)]
	public RecoveryAuthenticationType AllowedAuthenticationType { get; set; }

	[Alias(new string[] { "AllowAnyServer" })]
	[ValidateNotNull]
	[Parameter(Position = 2)]
	public bool? ReplicationAllowedFromAnyServer { get; set; }

	[Alias(new string[] { "Thumbprint" })]
	[ValidateNotNullOrEmpty]
	[Parameter(ValueFromPipelineByPropertyName = true)]
	public string CertificateThumbprint { get; set; }

	[Alias(new string[] { "StorageLoc" })]
	[ValidateNotNullOrEmpty]
	[Parameter]
	public string DefaultStorageLocation { get; set; }

	[Alias(new string[] { "KerbAuthPort" })]
	[ValidateRange(1, 65535)]
	[ValidateNotNull]
	[Parameter(ParameterSetName = "AuthenticationPort")]
	public int? KerberosAuthenticationPort { get; set; }

	[Alias(new string[] { "CertAuthPort" })]
	[ValidateRange(1, 65535)]
	[ValidateNotNull]
	[Parameter(ParameterSetName = "AuthenticationPort")]
	public int? CertificateAuthenticationPort { get; set; }

	[ValidateNotNull]
	[SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Need to accept input for the parameter.")]
	[Parameter(ParameterSetName = "AuthenticationPortMapping")]
	public Hashtable KerberosAuthenticationPortMapping { get; set; }

	[ValidateNotNull]
	[SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Need to accept input for the parameter.")]
	[Parameter(ParameterSetName = "AuthenticationPortMapping")]
	public Hashtable CertificateAuthenticationPortMapping { get; set; }

	[ValidateNotNull]
	[Parameter]
	public TimeSpan? MonitoringInterval { get; set; }

	[ValidateNotNull]
	[Parameter]
	public TimeSpan? MonitoringStartTime { get; set; }

	[Parameter]
	public SwitchParameter Force { get; set; }

	[Parameter]
	public SwitchParameter Passthru { get; set; }

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		if (!ReplicationEnabled.HasValue && !ReplicationAllowedFromAnyServer.HasValue && AllowedAuthenticationType == (RecoveryAuthenticationType)0 && !KerberosAuthenticationPort.HasValue && !CertificateAuthenticationPort.HasValue && KerberosAuthenticationPortMapping == null && CertificateAuthenticationPortMapping == null && !MonitoringInterval.HasValue && !MonitoringStartTime.HasValue && string.IsNullOrEmpty(DefaultStorageLocation) && string.IsNullOrEmpty(CertificateThumbprint))
		{
			throw ExceptionHelper.CreateOperationFailedException(CmdletErrorMessages.NoParametersSpecified);
		}
		if (!string.IsNullOrEmpty(DefaultStorageLocation))
		{
			DefaultStorageLocation = PathUtility.GetFullPath(DefaultStorageLocation, base.CurrentFileSystemLocation);
		}
		if (ReplicationAllowedFromAnyServer.GetValueOrDefault() && string.IsNullOrEmpty(DefaultStorageLocation))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_ParameterRequiresParameter, "ReplicationAllowedFromAnyServer", "DefaultStorageLocation"));
		}
		if (MonitoringInterval.HasValue && (MonitoringInterval.Value < TimeSpan.FromHours(1.0) || MonitoringInterval.Value > TimeSpan.FromDays(7.0)))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_ParameterOutOfRange, "MonitoringInterval"));
		}
		if (MonitoringStartTime.HasValue && (MonitoringStartTime.Value < TimeSpan.Zero || MonitoringStartTime.Value > TimeSpan.FromHours(24.0)))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_ParameterOutOfRange, "MonitoringStartTime"));
		}
		if ((AllowedAuthenticationType == RecoveryAuthenticationType.Certificate || AllowedAuthenticationType == RecoveryAuthenticationType.CertificateAndKerberos) && string.IsNullOrWhiteSpace(CertificateThumbprint))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidArgument_ParameterNotProvided, "CertificateThumbprint"));
		}
		if (AllowedAuthenticationType == RecoveryAuthenticationType.Kerberos && !string.IsNullOrWhiteSpace(CertificateThumbprint))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidArgument_ParameterNotRequired, "CertificateThumbprint", AllowedAuthenticationType));
		}
		if (ReplicationEnabled.GetValueOrDefault() && AllowedAuthenticationType == (RecoveryAuthenticationType)0)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidArgument_ParameterNotProvided, "AllowedAuthenticationType"));
		}
		if (KerberosAuthenticationPort.HasValue || CertificateAuthenticationPort.HasValue)
		{
			bool flag = KerberosAuthenticationPortMapping != null && KerberosAuthenticationPortMapping.Count > 0;
			if (flag || (CertificateAuthenticationPortMapping != null && CertificateAuthenticationPortMapping.Count > 0))
			{
				throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_VMReplicationServer_ListenerPortMappingShouldBeUsed, flag ? "KerberosAuthenticationPortMapping" : "CertificateAuthenticationPortMapping"));
			}
		}
	}

	internal override IList<VMReplicationServer> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		return ParameterResolvers.GetServers(this, operationWatcher).SelectWithLogging(VMReplicationServer.GetReplicationServer, operationWatcher).ToList();
	}

	internal override void ProcessOneOperand(VMReplicationServer server, IOperationWatcher operationWatcher)
	{
		if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMReplicationServer, server.Server)))
		{
			return;
		}
		if (ReplicationEnabled.HasValue)
		{
			server.ReplicationEnabled = ReplicationEnabled.Value;
			if (!ReplicationEnabled.Value)
			{
				server.AllowedAuthenticationType = (RecoveryAuthenticationType)0;
				server.CertificateThumbprint = null;
			}
		}
		if (MonitoringInterval.HasValue)
		{
			server.MonitoringInterval = MonitoringInterval.Value;
		}
		if (MonitoringStartTime.HasValue)
		{
			server.MonitoringStartTime = MonitoringStartTime.Value;
		}
		switch (AllowedAuthenticationType)
		{
		case RecoveryAuthenticationType.Kerberos:
			server.AllowedAuthenticationType = AllowedAuthenticationType;
			server.CertificateThumbprint = null;
			break;
		case RecoveryAuthenticationType.Certificate:
		case RecoveryAuthenticationType.CertificateAndKerberos:
			server.AllowedAuthenticationType = AllowedAuthenticationType;
			server.CertificateThumbprint = CertificateThumbprint;
			break;
		}
		if (!string.IsNullOrEmpty(DefaultStorageLocation) && ((ReplicationAllowedFromAnyServer.HasValue && !ReplicationAllowedFromAnyServer.Value) || (!ReplicationAllowedFromAnyServer.HasValue && !server.ReplicationAllowedFromAnyServer)))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplication_InvalidValueForRequiredDependentParameter, "ReplicationAllowedFromAnyServer", bool.TrueString, "DefaultStorageLocation"));
		}
		bool flag = false;
		if (KerberosAuthenticationPort.HasValue)
		{
			server.KerberosAuthenticationPort = KerberosAuthenticationPort.Value;
		}
		else if (KerberosAuthenticationPortMapping == null || KerberosAuthenticationPortMapping.Count == 0)
		{
			server.KerberosAuthenticationPort = 80;
		}
		else
		{
			flag = true;
		}
		bool flag2 = false;
		if (CertificateAuthenticationPort.HasValue)
		{
			server.CertificateAuthenticationPort = CertificateAuthenticationPort.Value;
		}
		else if (server.CertificateAuthenticationPortMapping == null || CertificateAuthenticationPortMapping.Count == 0)
		{
			server.CertificateAuthenticationPort = 443;
		}
		else
		{
			flag2 = true;
		}
		if (flag || flag2)
		{
			server.SetServerListenerPortMappings(KerberosAuthenticationPortMapping, CertificateAuthenticationPortMapping, operationWatcher.ShouldContinue);
		}
		((IUpdatable)server).Put(operationWatcher);
		bool flag3 = false;
		if (ReplicationAllowedFromAnyServer.HasValue)
		{
			if (ReplicationAllowedFromAnyServer.Value)
			{
				if (!server.ReplicationAllowedFromAnyServer)
				{
					if (server.AuthorizationEntries.Length != 0)
					{
						if (!operationWatcher.ShouldContinue(CmdletResources.ShouldContinue_SetVMReplication_RemoveAllAuthEntries))
						{
							return;
						}
						server.RemoveAllAuthorizationEntries(operationWatcher);
					}
					server.AddAuthorizationEntry("*", DefaultStorageLocation, "DEFAULT", operationWatcher);
					flag3 = true;
				}
			}
			else if (server.ReplicationAllowedFromAnyServer)
			{
				server.RemoveAuthorizationEntry("*", operationWatcher);
			}
		}
		if (!flag3 && server.ReplicationAllowedFromAnyServer && !string.IsNullOrEmpty(DefaultStorageLocation))
		{
			server.TryFindAuthorizationEntry("*", out var foundEntry);
			foundEntry.ReplicaStorageLocation = DefaultStorageLocation;
			((IUpdatable)foundEntry).Put(operationWatcher);
		}
		if ((bool)Passthru)
		{
			operationWatcher.WriteObject(server);
		}
	}
}
