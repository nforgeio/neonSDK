using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Net.Sockets;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Set", "VMNetworkAdapterFailoverConfiguration", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMNetworkAdapterFailoverSetting) })]
internal sealed class SetVMNetworkAdapterFailoverConfiguration : VirtualizationCmdlet<VMNetworkAdapter>, IVmBySingularVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmBySingularObjectCmdlet, IVMNetworkAdapterBaseCmdlet, ISupportsPassthrough
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and array is easier for users to use.")]
	[Parameter(ParameterSetName = "ResourceObject", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Position = 0, Mandatory = true)]
	[ValidateNotNull]
	public VMNetworkAdapter VMNetworkAdapter { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public string VMName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VirtualMachine VM { get; set; }

	[Parameter(ParameterSetName = "VMObject")]
	[Parameter(ParameterSetName = "VMName")]
	public string VMNetworkAdapterName { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "This is by spec.")]
	[Parameter]
	public string IPv4Address { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "This is by spec.")]
	[Parameter]
	public string IPv6Address { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "This is by spec.")]
	[Parameter]
	public string IPv4SubnetMask { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "This is by spec.")]
	[ValidateRange(0, 128)]
	[Parameter]
	public int? IPv6SubnetPrefixLength { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "This is by spec.")]
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "DNS", Justification = "This is by spec.")]
	[Parameter]
	public string IPv4PreferredDNSServer { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "This is by spec.")]
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "DNS", Justification = "This is by spec.")]
	[Parameter]
	public string IPv4AlternateDNSServer { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "This is by spec.")]
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "DNS", Justification = "This is by spec.")]
	[Parameter]
	public string IPv6PreferredDNSServer { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "This is by spec.")]
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "DNS", Justification = "This is by spec.")]
	[Parameter]
	public string IPv6AlternateDNSServer { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "This is by spec.")]
	[Parameter]
	public string IPv4DefaultGateway { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "This is by spec.")]
	[Parameter]
	public string IPv6DefaultGateway { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "This is by spec.")]
	[ValidateNotNull]
	[Parameter]
	public SwitchParameter ClearFailoverIPv4Settings { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "This is by spec.")]
	[ValidateNotNull]
	[Parameter]
	public SwitchParameter ClearFailoverIPv6Settings { get; set; }

	[Parameter]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	public SwitchParameter Passthru { get; set; }

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		if (!ClearFailoverIPv4Settings.IsPresent && !ClearFailoverIPv6Settings.IsPresent && string.IsNullOrEmpty(IPv4Address) && string.IsNullOrEmpty(IPv4SubnetMask) && string.IsNullOrEmpty(IPv4DefaultGateway) && string.IsNullOrEmpty(IPv4PreferredDNSServer) && string.IsNullOrEmpty(IPv4AlternateDNSServer) && string.IsNullOrEmpty(IPv6Address) && !IPv6SubnetPrefixLength.HasValue && string.IsNullOrEmpty(IPv6DefaultGateway) && string.IsNullOrEmpty(IPv6PreferredDNSServer) && string.IsNullOrEmpty(IPv6AlternateDNSServer))
		{
			throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidOperation_NoSettingsChanged, "Set-VMNetworkAdapterFailoverConfiguration"));
		}
		if (ClearFailoverIPv4Settings.IsPresent && (!string.IsNullOrEmpty(IPv4Address) || !string.IsNullOrEmpty(IPv4SubnetMask) || !string.IsNullOrEmpty(IPv4DefaultGateway) || !string.IsNullOrEmpty(IPv4PreferredDNSServer) || !string.IsNullOrEmpty(IPv4AlternateDNSServer)))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_ConflictingParameters);
		}
		if (ClearFailoverIPv6Settings.IsPresent && (!string.IsNullOrEmpty(IPv6Address) || IPv6SubnetPrefixLength.HasValue || !string.IsNullOrEmpty(IPv6DefaultGateway) || !string.IsNullOrEmpty(IPv6PreferredDNSServer) || !string.IsNullOrEmpty(IPv6AlternateDNSServer)))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_ConflictingParameters);
		}
		ValidateIPParameterIfSet(IPv4Address, AddressFamily.InterNetwork, "IPv4Address");
		if (!string.IsNullOrEmpty(IPv4SubnetMask) && !VMNetworkAdapterFailoverSetting.IsValidSubnetMask(IPv4SubnetMask, AddressFamily.InterNetwork))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_InvalidFormat, "IPv4SubnetMask"));
		}
		ValidateIPParameterIfSet(IPv4DefaultGateway, AddressFamily.InterNetwork, "IPv4DefaultGateway");
		ValidateIPParameterIfSet(IPv4PreferredDNSServer, AddressFamily.InterNetwork, "IPv4PreferredDNSServer");
		ValidateIPParameterIfSet(IPv4AlternateDNSServer, AddressFamily.InterNetwork, "IPv4AlternateDNSServer");
		ValidateIPParameterIfSet(IPv6Address, AddressFamily.InterNetworkV6, "IPv6Address");
		ValidateIPParameterIfSet(IPv6DefaultGateway, AddressFamily.InterNetworkV6, "IPv6DefaultGateway");
		ValidateIPParameterIfSet(IPv6PreferredDNSServer, AddressFamily.InterNetworkV6, "IPv6PreferredDNSServer");
		ValidateIPParameterIfSet(IPv6AlternateDNSServer, AddressFamily.InterNetworkV6, "IPv6AlternateDNSServer");
	}

	internal override IList<VMNetworkAdapter> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (CurrentParameterSetIs("ResourceObject"))
		{
			return new VMNetworkAdapter[1] { VMNetworkAdapter };
		}
		return (from adapter in ParameterResolvers.ResolveNetworkAdapters(this, VMNetworkAdapterName, operationWatcher).OfType<VMNetworkAdapter>()
			where adapter.IsSynthetic
			select adapter).ToList();
	}

	internal override void ProcessOneOperand(VMNetworkAdapter operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMNetworkAdapter, operand.Name)))
		{
			VirtualMachine parentAs = operand.GetParentAs<VirtualMachine>();
			VMReplication vMReplication = VMReplication.GetVMReplication(parentAs, VMReplicationRelationshipType.Simple);
			if (vMReplication.ReplicationState == VMReplicationState.Disabled)
			{
				throw ExceptionHelper.CreateInvalidStateException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplication_NotEnabled, parentAs.Name), null, null);
			}
			if (vMReplication.IsReplicatingToExternalProvider)
			{
				throw ExceptionHelper.CreateInvalidOperationException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_NotSupportedForExternalReplicationProvider, parentAs.Name), null, null);
			}
			VMNetworkAdapterFailoverSetting failoverSetting = operand.FailoverSetting;
			if (failoverSetting == null)
			{
				throw new InvalidOperationException(CmdletErrorMessages.VMNetworkAdapterFailoverConfiguration_NotSupportedAdapter);
			}
			ConfigureIPv4Settings(failoverSetting);
			ConfigureIPv6Settings(failoverSetting);
			((IUpdatable)failoverSetting).Put(operationWatcher);
			if ((bool)Passthru)
			{
				operationWatcher.WriteObject(failoverSetting);
			}
		}
	}

	private static void ValidateIPParameterIfSet(string value, AddressFamily family, string parameterName)
	{
		if (!string.IsNullOrEmpty(value) && !VMNetworkAdapterFailoverSetting.IsValidIP(value, family))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_InvalidFormat, parameterName));
		}
	}

	private static void ValidateIPValueNotSetOrThrow(string value, string propertyName)
	{
		if (!string.IsNullOrEmpty(value))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMNetworkAdapterFailoverConfiguration_IPSettingsSpecifiedWithoutAddress, propertyName));
		}
	}

	private void ConfigureIPv4Settings(VMNetworkAdapterFailoverSetting failoverSetting)
	{
		if ((bool)ClearFailoverIPv4Settings)
		{
			failoverSetting.IPv4Address = string.Empty;
			failoverSetting.IPv4SubnetMask = string.Empty;
			failoverSetting.IPv4DefaultGateway = string.Empty;
			failoverSetting.IPv4PreferredDNSServer = string.Empty;
			failoverSetting.IPv4AlternateDNSServer = string.Empty;
		}
		else if (!string.IsNullOrEmpty(IPv4Address))
		{
			failoverSetting.IPv4Address = IPv4Address;
		}
		if (!string.IsNullOrEmpty(failoverSetting.IPv4Address))
		{
			if (!string.IsNullOrEmpty(IPv4SubnetMask))
			{
				failoverSetting.IPv4SubnetMask = IPv4SubnetMask;
			}
			if (string.IsNullOrEmpty(failoverSetting.IPv4SubnetMask))
			{
				throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidArgument_ParameterNotProvided, "IPv4SubnetMask"));
			}
			if (!string.IsNullOrEmpty(IPv4DefaultGateway))
			{
				failoverSetting.IPv4DefaultGateway = IPv4DefaultGateway;
			}
			if (!string.IsNullOrEmpty(IPv4PreferredDNSServer))
			{
				failoverSetting.IPv4PreferredDNSServer = IPv4PreferredDNSServer;
			}
			if (!string.IsNullOrEmpty(IPv4AlternateDNSServer))
			{
				failoverSetting.IPv4AlternateDNSServer = IPv4AlternateDNSServer;
			}
		}
		else
		{
			ValidateIPValueNotSetOrThrow(IPv4SubnetMask, "IPv4SubnetMask");
			ValidateIPValueNotSetOrThrow(IPv4DefaultGateway, "IPv4DefaultGateway");
			ValidateIPValueNotSetOrThrow(IPv4PreferredDNSServer, "IPv4PreferredDNSServer");
			ValidateIPValueNotSetOrThrow(IPv4AlternateDNSServer, "IPv4AlternateDNSServer");
		}
	}

	private void ConfigureIPv6Settings(VMNetworkAdapterFailoverSetting failoverSetting)
	{
		if (ClearFailoverIPv6Settings.IsPresent)
		{
			failoverSetting.IPv6Address = string.Empty;
			failoverSetting.IPv6SubnetPrefixLength = string.Empty;
			failoverSetting.IPv6DefaultGateway = string.Empty;
			failoverSetting.IPv6PreferredDNSServer = string.Empty;
			failoverSetting.IPv6AlternateDNSServer = string.Empty;
		}
		else if (!string.IsNullOrEmpty(IPv6Address))
		{
			failoverSetting.IPv6Address = IPv6Address;
		}
		if (!string.IsNullOrEmpty(failoverSetting.IPv6Address))
		{
			if (IPv6SubnetPrefixLength.HasValue)
			{
				failoverSetting.IPv6SubnetPrefixLength = IPv6SubnetPrefixLength.Value.ToString(CultureInfo.InvariantCulture);
			}
			if (string.IsNullOrEmpty(failoverSetting.IPv6SubnetPrefixLength))
			{
				throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidArgument_ParameterNotProvided, "IPv6SubnetPrefixLength"));
			}
			if (!string.IsNullOrEmpty(IPv6DefaultGateway))
			{
				failoverSetting.IPv6DefaultGateway = IPv6DefaultGateway;
			}
			if (!string.IsNullOrEmpty(IPv6PreferredDNSServer))
			{
				failoverSetting.IPv6PreferredDNSServer = IPv6PreferredDNSServer;
			}
			if (!string.IsNullOrEmpty(IPv6AlternateDNSServer))
			{
				failoverSetting.IPv6AlternateDNSServer = IPv6AlternateDNSServer;
			}
		}
		else
		{
			if (IPv6SubnetPrefixLength.HasValue)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMNetworkAdapterFailoverConfiguration_IPSettingsSpecifiedWithoutAddress, "IPv6SubnetPrefixLength"));
			}
			ValidateIPValueNotSetOrThrow(IPv6DefaultGateway, "IPv6DefaultGateway");
			ValidateIPValueNotSetOrThrow(IPv6PreferredDNSServer, "IPv6PreferredDNSServer");
			ValidateIPValueNotSetOrThrow(IPv6AlternateDNSServer, "IPv6AlternateDNSServer");
		}
	}
}
