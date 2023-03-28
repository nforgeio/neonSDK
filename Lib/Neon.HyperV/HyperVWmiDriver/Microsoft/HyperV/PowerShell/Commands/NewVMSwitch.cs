using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("New", "VMSwitch", DefaultParameterSetName = "NetAdapterName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMSwitch) })]
internal sealed class NewVMSwitch : VirtualizationCreationCmdlet<VMSwitch>
{
	internal static class ParameterSetNames
	{
		public const string NetAdapterName = "NetAdapterName";

		public const string NetAdapterInterfaceDescription = "NetAdapterInterfaceDescription";

		public const string SwitchType = "SwitchType";
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "NetAdapterInterfaceDescription", ValueFromPipelineByPropertyName = true)]
	[Parameter(ParameterSetName = "NetAdapterName", ValueFromPipelineByPropertyName = true)]
	[Parameter(ParameterSetName = "SwitchType", ValueFromPipelineByPropertyName = true)]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "NetAdapterInterfaceDescription", ValueFromPipelineByPropertyName = true)]
	[Parameter(ParameterSetName = "NetAdapterName", ValueFromPipelineByPropertyName = true)]
	[Parameter(ParameterSetName = "SwitchType", ValueFromPipelineByPropertyName = true)]
	[Alias(new string[] { "PSComputerName" })]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "NetAdapterInterfaceDescription", ValueFromPipelineByPropertyName = true)]
	[Parameter(ParameterSetName = "NetAdapterName", ValueFromPipelineByPropertyName = true)]
	[Parameter(ParameterSetName = "SwitchType", ValueFromPipelineByPropertyName = true)]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[Parameter(Mandatory = true, Position = 0)]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "SwitchName" })]
	public string Name { get; set; }

	[Parameter]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "SwitchId" })]
	public string Id { get; set; }

	[Parameter(Mandatory = true, ParameterSetName = "SwitchType")]
	[ValidateSet(new string[] { "Internal", "Private" }, IgnoreCase = true)]
	public VMSwitchType SwitchType { get; set; }

	[Parameter(ParameterSetName = "NetAdapterInterfaceDescription")]
	[Parameter(ParameterSetName = "NetAdapterName")]
	public bool AllowManagementOS { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "InterfaceAlias" })]
	[Parameter(ParameterSetName = "NetAdapterName", Mandatory = true, ValueFromPipelineByPropertyName = true)]
	public string[] NetAdapterName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "NetAdapterInterfaceDescription", Mandatory = true, ValueFromPipelineByPropertyName = true)]
	[Alias(new string[] { "InterfaceDescription" })]
	public string[] NetAdapterInterfaceDescription { get; set; }

	[ValidateNotNull]
	[Parameter]
	public string Notes { get; set; }

	[Parameter]
	public VMSwitchBandwidthMode MinimumBandwidthMode { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Iov", Justification = "This is by spec.")]
	[ValidateNotNull]
	[Parameter]
	public bool? EnableIov { get; set; }

	[Parameter]
	public bool? EnablePacketDirect { get; set; }

	[Parameter]
	public bool? EnableEmbeddedTeaming { get; set; }

	protected override void NormalizeParameters()
	{
		Notes = Notes ?? string.Empty;
		if (!IsParameterSpecified("AllowManagementOS"))
		{
			AllowManagementOS = true;
		}
		if (!IsParameterSpecified("MinimumBandwidthMode"))
		{
			MinimumBandwidthMode = VMSwitchBandwidthMode.Default;
		}
		if (!IsParameterSpecified("EnablePacketDirect"))
		{
			EnablePacketDirect = false;
		}
		if (!IsParameterSpecified("EnableEmbeddedTeaming"))
		{
			EnableEmbeddedTeaming = false;
			if ((CurrentParameterSetIs("NetAdapterInterfaceDescription") || CurrentParameterSetIs("NetAdapterName")) && ((NetAdapterName != null && NetAdapterName.Length > 1) || (NetAdapterInterfaceDescription != null && NetAdapterInterfaceDescription.Length > 1)))
			{
				EnableEmbeddedTeaming = true;
			}
		}
		if (!IsParameterSpecified("EnableIov"))
		{
			EnableIov = false;
		}
		if (!IsParameterSpecified("SwitchType") && (CurrentParameterSetIs("NetAdapterInterfaceDescription") || CurrentParameterSetIs("NetAdapterName")))
		{
			SwitchType = VMSwitchType.External;
		}
		base.NormalizeParameters();
	}

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		if (CurrentParameterSetIs("SwitchType") && SwitchType == VMSwitchType.External)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.NewVMSwitch_InvalidSwitchType);
		}
		if ((CurrentParameterSetIs("NetAdapterInterfaceDescription") || CurrentParameterSetIs("NetAdapterName")) && !EnableEmbeddedTeaming.GetValueOrDefault(false) && ((!NetAdapterName.IsNullOrEmpty() && NetAdapterName.Length > 1) || (!NetAdapterInterfaceDescription.IsNullOrEmpty() && NetAdapterInterfaceDescription.Length > 1)))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.NewVMSwitch_NonTeamingWithMultipleAdapters);
		}
		if (!string.IsNullOrEmpty(Id) && !Guid.TryParse(Id, out var _))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_InvalidFormat, "Id"));
		}
	}

	internal override IList<VMSwitch> CreateObjects(IOperationWatcher operationWatcher)
	{
		List<VMSwitch> list = new List<VMSwitch>();
		foreach (Server server in ParameterResolvers.GetServers(this, operationWatcher))
		{
			if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_NewVMSwitch, Name)))
			{
				continue;
			}
			VMSwitch vMSwitch = null;
			try
			{
				vMSwitch = VMSwitch.Create(server, Name, Id, Notes, EnableIov.Value, MinimumBandwidthMode, EnablePacketDirect.Value, EnableEmbeddedTeaming.GetValueOrDefault(), operationWatcher);
				if (EnableEmbeddedTeaming.GetValueOrDefault())
				{
					ConfigureNicTeaming(vMSwitch, operationWatcher);
				}
				vMSwitch.ConfigureConnections(SwitchType, AllowManagementOS, NetAdapterName, NetAdapterInterfaceDescription, operationWatcher);
				list.Add(vMSwitch);
			}
			catch (Exception e2)
			{
				if (vMSwitch != null)
				{
					try
					{
						((IRemovable)vMSwitch).Remove(operationWatcher);
					}
					catch (Exception innerException)
					{
						Exception e = ExceptionHelper.CreateInvalidStateException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.NewVMSwitch_FailedToRollBack, vMSwitch.Name), innerException, vMSwitch);
						ExceptionHelper.DisplayErrorOnException(e, operationWatcher);
					}
				}
				ExceptionHelper.DisplayErrorOnException(e2, operationWatcher);
			}
		}
		return list;
	}

	private static void ConfigureNicTeaming(VMSwitch virtualSwitch, IOperationWatcher operationWatcher)
	{
		VMSwitchNicTeamingSetting vMSwitchNicTeamingSetting = virtualSwitch.NicTeamingSetting;
		if (vMSwitchNicTeamingSetting == null)
		{
			vMSwitchNicTeamingSetting = VMSwitchNicTeamingSetting.CreateTemplateSwitchNicTeamingSetting(virtualSwitch);
		}
		vMSwitchNicTeamingSetting.LoadBalancingAlgorithm = 5u;
		vMSwitchNicTeamingSetting.TeamingMode = 1u;
		virtualSwitch.AddOrModifyOneFeatureSetting(vMSwitchNicTeamingSetting, operationWatcher);
	}
}
