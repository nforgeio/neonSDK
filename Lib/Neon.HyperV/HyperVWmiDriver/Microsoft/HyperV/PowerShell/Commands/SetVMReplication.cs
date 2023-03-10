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

[Cmdlet("Set", "VMReplication", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMReplication) })]
internal sealed class SetVMReplication : VirtualizationCmdlet<VMReplication>, IVmByVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, ISupportsPassthrough, ISupportsAsJob
{
	private bool replicationSettingsPassed;

	private bool otherSettingsPassed;

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter]
	[Parameter(ParameterSetName = "VMName")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[Alias(new string[] { "Name" })]
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public string[] VMName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VirtualMachine[] VM { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNull]
	[Parameter(ParameterSetName = "VMReplication", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VMReplication[] VMReplication { get; set; }

	[Alias(new string[] { "ReplicaServer" })]
	[ValidateNotNullOrEmpty]
	[Parameter(Position = 1)]
	public string ReplicaServerName { get; set; }

	[Alias(new string[] { "ReplicaPort" })]
	[ValidateRange(1, 65535)]
	[ValidateNotNull]
	[Parameter(Position = 2)]
	public int? ReplicaServerPort { get; set; }

	[Alias(new string[] { "AuthType" })]
	[ValidateNotNull]
	[Parameter(Position = 3)]
	public ReplicationAuthenticationType? AuthenticationType { get; set; }

	[Alias(new string[] { "Thumbprint", "Cert" })]
	[Parameter(ValueFromPipelineByPropertyName = true)]
	public string CertificateThumbprint { get; set; }

	[ValidateNotNull]
	[Parameter]
	public bool? CompressionEnabled { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Kvp", Justification = "This is by spec.")]
	[ValidateNotNull]
	[Parameter]
	public bool? ReplicateHostKvpItems { get; set; }

	[ValidateNotNull]
	[Parameter]
	public bool? BypassProxyServer { get; set; }

	[ValidateNotNull]
	[Parameter]
	public bool? EnableWriteOrderPreservationAcrossDisks { get; set; }

	[Alias(new string[] { "IRTime" })]
	[ValidateNotNull]
	[Parameter]
	public DateTime? InitialReplicationStartTime { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "VSS", Justification = "This is per spec.")]
	[Alias(new string[] { "DisableVSS" })]
	[Parameter]
	public SwitchParameter DisableVSSSnapshotReplication { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "VSS", Justification = "This is per spec.")]
	[Alias(new string[] { "VSSFreq" })]
	[ValidateRange(1, 12)]
	[ValidateNotNull]
	[Parameter]
	public int? VSSSnapshotFrequencyHour { get; set; }

	[Alias(new string[] { "RecHist" })]
	[ValidateRange(0, 24)]
	[ValidateNotNull]
	[Parameter]
	public int? RecoveryHistory { get; set; }

	[Alias(new string[] { "RepFreq" })]
	[ValidateRange(30, 900)]
	[Parameter]
	public int? ReplicationFrequencySec { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Usability is more important than the slight gain in efficiency here.")]
	[ValidateNotNull]
	[Parameter]
	public HardDiskDrive[] ReplicatedDisks { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Usability is more important than the slight gain in efficiency here.")]
	[ValidateNotNull]
	[Parameter]
	public string[] ReplicatedDiskPaths { get; set; }

	[Parameter]
	public SwitchParameter Reverse { get; set; }

	[Alias(new string[] { "AutoResync" })]
	[Parameter]
	[ValidateNotNull]
	public bool? AutoResynchronizeEnabled { get; set; }

	[Alias(new string[] { "AutoResyncStart" })]
	[Parameter]
	[ValidateNotNull]
	public TimeSpan? AutoResynchronizeIntervalStart { get; set; }

	[Alias(new string[] { "AutoResyncEnd" })]
	[Parameter]
	[ValidateNotNull]
	public TimeSpan? AutoResynchronizeIntervalEnd { get; set; }

	[Parameter]
	public SwitchParameter AsReplica { get; set; }

	[Parameter]
	public SwitchParameter UseBackup { get; set; }

	[Alias(new string[] { "AllowedPS" })]
	[Parameter(ValueFromPipelineByPropertyName = true)]
	public string AllowedPrimaryServer { get; set; }

	[Parameter]
	public SwitchParameter AsJob { get; set; }

	[Parameter]
	public SwitchParameter Passthru { get; set; }

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		bool num = AutoResynchronizeEnabled.HasValue || AutoResynchronizeIntervalStart.HasValue || AutoResynchronizeIntervalEnd.HasValue || InitialReplicationStartTime.HasValue || (bool)UseBackup || !string.IsNullOrEmpty(CertificateThumbprint);
		if (CompressionEnabled.HasValue || BypassProxyServer.HasValue || !string.IsNullOrEmpty(ReplicaServerName) || AuthenticationType.HasValue || ReplicaServerPort.HasValue || (bool)DisableVSSSnapshotReplication || VSSSnapshotFrequencyHour.HasValue || RecoveryHistory.HasValue || EnableWriteOrderPreservationAcrossDisks.HasValue || ReplicateHostKvpItems.HasValue || ReplicationFrequencySec.HasValue || ReplicatedDisks != null || ReplicatedDiskPaths != null)
		{
			otherSettingsPassed = true;
		}
		if (num || otherSettingsPassed)
		{
			replicationSettingsPassed = true;
		}
		if (string.IsNullOrEmpty(AllowedPrimaryServer) && !AsReplica && !Reverse && !replicationSettingsPassed)
		{
			throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidOperation_NoSettingsChanged, "Set-VMReplication"));
		}
		if ((bool)Reverse && (ReplicatedDiskPaths != null || ReplicatedDisks != null))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_ParametersAreMutuallyExclusive, "ReplicatedDisks, ReplicatedDiskPaths", "Reverse"));
		}
		if ((bool)UseBackup && !InitialReplicationStartTime.HasValue)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMReplication_UseBackup_ScheduledTime);
		}
	}

	internal override IList<VMReplication> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		IList<VMReplication> inputs;
		if (CurrentParameterSetIs("VMReplication"))
		{
			inputs = VMReplication;
		}
		else
		{
			IList<VirtualMachine> inputs2 = ((!CurrentParameterSetIs("VMObject")) ? ParameterResolvers.ResolveVirtualMachines(this, operationWatcher) : VM);
			inputs = inputs2.SelectWithLogging(GetVMReplicationObjectFromVirtualMachine, operationWatcher).ToList();
		}
		return inputs.SelectWithLogging(ValidateReplication, operationWatcher).ToList();
	}

	internal override void ProcessOneOperand(VMReplication replication, IOperationWatcher operationWatcher)
	{
		if (!operationWatcher.ShouldProcess(string.Format(format: ((replication.ReplicationMode == VMReplicationMode.Primary || replication.ReplicationMode == VMReplicationMode.None) && AsReplica.IsPresent) ? CmdletResources.ShouldProcess_SetVMReplication_AsReplica : ((replication.ReplicationMode == VMReplicationMode.None || !Reverse.IsPresent) ? CmdletResources.ShouldProcess_SetVMReplication : CmdletResources.ShouldProcess_SetVMReplication_Reverse), provider: CultureInfo.CurrentCulture, arg0: replication.VMName)))
		{
			return;
		}
		VMReplicationServer vMReplicationServer = null;
		if (AsReplica.IsPresent)
		{
			replication.VirtualMachine.SetReplicationStateEx(replication, ReplicationWmiState.WaitingToCompleteInitialReplication, operationWatcher);
		}
		else if (replicationSettingsPassed || (bool)Reverse)
		{
			ValidateAndSetReplicationProperties(replication, operationWatcher);
			if ((bool)Reverse)
			{
				string replicaServerName = (string.IsNullOrEmpty(ReplicaServerName) ? replication.PrimaryConnectionPoint : ReplicaServerName);
				replication.ReplicaServerName = replicaServerName;
				vMReplicationServer = VMReplicationServer.GetReplicationServer(replication.Server);
				vMReplicationServer.ReverseReplicationRelationship(replication, operationWatcher);
			}
			else
			{
				((IUpdatable)replication).Put(operationWatcher);
			}
			if (InitialReplicationStartTime.HasValue)
			{
				InitialReplicationType initialReplicationType;
				if ((bool)UseBackup)
				{
					initialReplicationType = InitialReplicationType.UsingBackup;
				}
				else
				{
					initialReplicationType = InitialReplicationType.OverNetwork;
					IVMTask replicationStartTask = replication.VirtualMachine.GetReplicationStartTask();
					if (replicationStartTask != null)
					{
						switch (replicationStartTask.JobType)
						{
						case 94:
							initialReplicationType = InitialReplicationType.OverNetwork;
							break;
						case 117:
							initialReplicationType = InitialReplicationType.UsingBackup;
							break;
						}
					}
				}
				if (vMReplicationServer == null)
				{
					vMReplicationServer = VMReplicationServer.GetReplicationServer(replication.Server);
				}
				vMReplicationServer.StartReplication(replication.VirtualMachine, initialReplicationType, string.Empty, InitialReplicationStartTime.Value, operationWatcher);
			}
		}
		if (!string.IsNullOrEmpty(AllowedPrimaryServer))
		{
			if (vMReplicationServer == null)
			{
				vMReplicationServer = VMReplicationServer.GetReplicationServer(replication.Server);
			}
			vMReplicationServer.TryFindAuthorizationEntry(AllowedPrimaryServer, out var foundEntry);
			vMReplicationServer.SetAuthorizationEntry(replication.VirtualMachine, foundEntry.AllowedPrimaryServer, foundEntry.ReplicaStorageLocation, foundEntry.TrustGroup, operationWatcher);
		}
		if ((bool)Passthru)
		{
			operationWatcher.WriteObject(replication);
		}
	}

	private VMReplication GetVMReplicationObjectFromVirtualMachine(VirtualMachine vm)
	{
		if ((bool)AsReplica || (bool)Reverse || vm.ReplicationMode != VMReplicationMode.Replica)
		{
			return global::Microsoft.HyperV.PowerShell.VMReplication.GetVMReplication(vm, VMReplicationRelationshipType.Simple);
		}
		return global::Microsoft.HyperV.PowerShell.VMReplication.GetVMReplication(vm, VMReplicationRelationshipType.Extended);
	}

	private void ValidateAndSetAuthentication(VMReplication replication)
	{
		if (AuthenticationType.HasValue)
		{
			if (AuthenticationType.Value == ReplicationAuthenticationType.Certificate)
			{
				if (string.IsNullOrEmpty(replication.CertificateThumbprint) && string.IsNullOrEmpty(CertificateThumbprint))
				{
					throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidArgument_ParameterNotProvided, "CertificateThumbprint"));
				}
				replication.AuthenticationType = AuthenticationType.Value;
			}
			else
			{
				if (!string.IsNullOrEmpty(CertificateThumbprint))
				{
					throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidArgument_ParameterNotRequired, "CertificateThumbprint", "AuthenticationType"));
				}
				replication.AuthenticationType = AuthenticationType.Value;
				replication.CertificateThumbprint = null;
			}
		}
		else if (replication.AuthenticationType == (ReplicationAuthenticationType)0)
		{
			replication.AuthenticationType = ReplicationAuthenticationType.Kerberos;
			replication.CertificateThumbprint = null;
		}
		if (!string.IsNullOrEmpty(CertificateThumbprint))
		{
			if (replication.AuthenticationType != ReplicationAuthenticationType.Certificate)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidArgument_ParameterNotRequired, "CertificateThumbprint", "AuthenticationType"));
			}
			replication.CertificateThumbprint = CertificateThumbprint;
		}
	}

	private void ValidateAndSetIncludedDisks(VMReplication replication)
	{
		bool num = !ReplicatedDisks.IsNullOrEmpty();
		bool flag = !ReplicatedDiskPaths.IsNullOrEmpty();
		HardDiskDrive[] source = replication.VirtualMachine.GetVirtualHardDiskDrives().ToArray();
		List<HardDiskDrive> list = new List<HardDiskDrive>();
		if (num)
		{
			HardDiskDrive[] replicatedDisks = ReplicatedDisks;
			foreach (HardDiskDrive hardDiskDrive in replicatedDisks)
			{
				HardDiskDrive currentDisk = hardDiskDrive;
				if (!source.Any((HardDiskDrive vmDisk) => string.Equals(currentDisk.Path, vmDisk.Path)))
				{
					throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplication_HardDiskNotAttachedToVM, hardDiskDrive.Path, replication.VMName));
				}
			}
			list.AddRange(ReplicatedDisks);
		}
		if (flag)
		{
			string[] replicatedDiskPaths = ReplicatedDiskPaths;
			foreach (string text in replicatedDiskPaths)
			{
				string diskPath = text.Trim();
				HardDiskDrive item;
				if ((item = source.FirstOrDefault((HardDiskDrive vmDisk) => string.Equals(diskPath, vmDisk.Path))) == null)
				{
					throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplication_HardDiskNotAttachedToVM, diskPath, replication.VMName));
				}
				list.Add(item);
			}
		}
		if (list.Count > 0)
		{
			replication.ReplicatedDisks = list;
		}
	}

	private void ValidateAndSetReplicationFrequency(VMReplication replication)
	{
		if (ReplicationFrequencySec.HasValue)
		{
			if (!global::Microsoft.HyperV.PowerShell.VMReplication.IsValidReplicationFrequency(ReplicationFrequencySec.Value))
			{
				throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidArgument_ReplicationFrequencyNotValid);
			}
			if (replication.ReplicationRelationshipType == VMReplicationRelationshipType.Extended && ReplicationFrequencySec.Value < replication.ReplicationFrequencySec)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidArgument_ReplicationFrequencyNotValid);
			}
			replication.ReplicationFrequencySec = ReplicationFrequencySec.Value;
		}
		if (RecoveryHistory.HasValue)
		{
			replication.RecoveryHistory = RecoveryHistory.Value;
		}
		if (VSSSnapshotFrequencyHour.HasValue)
		{
			if (replication.RecoveryHistory == 0)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMReplication_AppConsistentWithoutRecoveryHistory);
			}
			if ((bool)DisableVSSSnapshotReplication)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidArgument_ParameterNotRequired, "VSSSnapshotFrequencyHour", "DisableVSSSnapshotReplication"));
			}
			replication.VSSSnapshotFrequencyHour = VSSSnapshotFrequencyHour.Value;
		}
		else if (!DisableVSSSnapshotReplication && replication.VSSSnapshotFrequencyHour == 0 && replication.ReplicationWmiState == ReplicationWmiState.Disabled && replication.RecoveryHistory > 0)
		{
			replication.VSSSnapshotFrequencyHour = 4;
		}
		if ((bool)DisableVSSSnapshotReplication || replication.RecoveryHistory == 0)
		{
			replication.VSSSnapshotFrequencyHour = 0;
		}
	}

	private void ValidateAndSetReplicationProperties(VMReplication replication, IOperationWatcher watcher)
	{
		if (!string.IsNullOrEmpty(ReplicaServerName))
		{
			if (Uri.CheckHostName(ReplicaServerName) != UriHostNameType.Dns)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_InvalidUri, "ReplicaServerName"), new UriFormatException("ReplicaServerName"));
			}
			global::Microsoft.HyperV.PowerShell.VMReplication.ValidateReplicaServerName(replication);
			replication.ReplicaServerName = ReplicaServerName;
		}
		else if (string.IsNullOrEmpty(replication.ReplicaServerName))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidArgument_ParameterNotProvided, "ReplicaServerName"));
		}
		if (InitialReplicationStartTime.HasValue)
		{
			DateTime value = InitialReplicationStartTime.Value;
			if (value < DateTime.Now)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMReplication_StartTimeOccursInPast);
			}
			if (value > DateTime.Now.AddDays(7.0))
			{
				throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMReplication_StartTimeOccursTooMuchInFuture);
			}
			ReplicationWmiState replicationWmiState = replication.ReplicationWmiState;
			if (replicationWmiState != 0 && replicationWmiState != ReplicationWmiState.Ready && (!Reverse || replication.ReplicationMode != VMReplicationMode.Replica))
			{
				throw ExceptionHelper.CreateInvalidStateException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_InvalidReplicationState, replication.VMName), null, null);
			}
		}
		ValidateAndSetAuthentication(replication);
		ValidateAndSetReplicationFrequency(replication);
		ValidateAndSetIncludedDisks(replication);
		if (ReplicaServerPort.HasValue)
		{
			replication.ReplicaServerPort = ReplicaServerPort.Value;
		}
		else if (replication.ReplicaServerPort == 0)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidArgument_ParameterNotProvided, "ReplicaServerPort"));
		}
		if (CompressionEnabled.HasValue)
		{
			replication.CompressionEnabled = CompressionEnabled.Value;
		}
		if (BypassProxyServer.HasValue)
		{
			replication.BypassProxyServer = BypassProxyServer.Value;
		}
		if (AutoResynchronizeEnabled.HasValue)
		{
			replication.AutoResynchronizeEnabled = AutoResynchronizeEnabled.Value;
		}
		if (AutoResynchronizeIntervalStart.HasValue)
		{
			replication.AutoResynchronizeIntervalStart = AutoResynchronizeIntervalStart.Value;
		}
		if (AutoResynchronizeIntervalEnd.HasValue)
		{
			replication.AutoResynchronizeIntervalEnd = AutoResynchronizeIntervalEnd.Value;
		}
		if (ReplicateHostKvpItems.HasValue)
		{
			replication.ReplicateHostKvpItems = ReplicateHostKvpItems.Value;
		}
		if (EnableWriteOrderPreservationAcrossDisks.HasValue)
		{
			replication.EnableWriteOrderPreservationAcrossDisks = true;
			watcher.WriteWarning(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidArgument_ParameterNotRequired, "EnableWriteOrderPreservationAcrossDisks", "Set-VMReplication"));
		}
	}

	private void ValidateParametersForAsReplica()
	{
		if (!string.IsNullOrEmpty(ReplicaServerName) || ReplicaServerPort.HasValue || VSSSnapshotFrequencyHour.HasValue || AuthenticationType.HasValue || !string.IsNullOrEmpty(CertificateThumbprint) || CompressionEnabled.HasValue || BypassProxyServer.HasValue || EnableWriteOrderPreservationAcrossDisks.HasValue || DisableVSSSnapshotReplication.IsPresent || RecoveryHistory.HasValue || Reverse.IsPresent || AutoResynchronizeEnabled.HasValue || AutoResynchronizeIntervalStart.HasValue || AutoResynchronizeIntervalEnd.HasValue || InitialReplicationStartTime.HasValue || UseBackup.IsPresent || ReplicateHostKvpItems.HasValue || ReplicationFrequencySec.HasValue || ReplicatedDisks != null || ReplicatedDiskPaths != null)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_ParameterRequiresParameter, "AllowedPrimaryServer", "AsReplica"));
		}
	}

	private VMReplication ValidateReplication(VMReplication replication)
	{
		if (!replication.IsEnabled)
		{
			throw ExceptionHelper.CreateInvalidStateException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplication_NotEnabled, replication.VMName), null, null);
		}
		VMReplicationMode replicationMode = replication.ReplicationMode;
		if (replicationMode != VMReplicationMode.Primary && replicationMode != VMReplicationMode.Replica && replicationMode != VMReplicationMode.ExtendedReplica)
		{
			global::Microsoft.HyperV.PowerShell.VMReplication.ReportInvalidModeError("Set-VMReplication", replication);
		}
		if (!string.IsNullOrEmpty(AllowedPrimaryServer) && !VMReplicationServer.GetReplicationServer(replication.Server).TryFindAuthorizationEntry(AllowedPrimaryServer, out var _))
		{
			throw ExceptionHelper.CreateOperationFailedException(CmdletErrorMessages.VMReplication_AuthorizationEntryNotFound);
		}
		switch (replicationMode)
		{
		case VMReplicationMode.Primary:
			if ((bool)Reverse)
			{
				global::Microsoft.HyperV.PowerShell.VMReplication.ReportInvalidModeError("Set-VMReplication", replication);
			}
			if (otherSettingsPassed && replication.IsReplicatingToExternalProvider)
			{
				throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_NotSupportedForExternalReplicationProvider, replication.VMName));
			}
			break;
		case VMReplicationMode.Replica:
			if (!Reverse.IsPresent && replicationSettingsPassed && replication.ReplicationRelationshipType == VMReplicationRelationshipType.Simple)
			{
				throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplication_CannotModifySettingsOnReplica, replication.VMName));
			}
			if (replication.ReplicationRelationshipType == VMReplicationRelationshipType.Extended && (ReplicatedDiskPaths != null || ReplicatedDisks != null))
			{
				throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplication_CannotModifyIncludedVhdsOnReplica, replication.VMName));
			}
			break;
		default:
			if (!Reverse && replicationSettingsPassed)
			{
				throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplication_CannotModifySettingsOnReplica, replication.VMName));
			}
			break;
		}
		if ((bool)AsReplica)
		{
			if (!VMReplicationServer.GetReplicationServer(replication.Server).TryFindAuthorizationEntry("*", out var _) && string.IsNullOrEmpty(AllowedPrimaryServer))
			{
				throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidArgument_ParameterNotProvided, "AllowedPrimaryServer"));
			}
			ValidateParametersForAsReplica();
		}
		if ((bool)Reverse && replication.AuthenticationType == ReplicationAuthenticationType.Certificate && string.IsNullOrEmpty(CertificateThumbprint))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplication_Reverse_CertificateThumbprintNotProvided, replication.VMName));
		}
		return replication;
	}
}
