using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Enable", "VMReplication", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMReplication) })]
internal sealed class EnableVMReplication : VirtualizationCmdlet<VirtualMachine>, IVmByVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByObjectCmdlet, ISupportsAsJob, ISupportsPassthrough
{
	private static class AdditionalParameterSetNames
	{
		public const string VMNameAsReplica = "VMName_AsReplica";

		public const string VMObjectAsReplica = "VMObject_AsReplica";
	}

	private VMReplicationAuthorizationEntry authorizationEntry;

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMName_AsReplica")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMName_AsReplica")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMName_AsReplica")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[Alias(new string[] { "Name" })]
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	[Parameter(ParameterSetName = "VMName_AsReplica", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public string[] VMName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	[Parameter(ParameterSetName = "VMObject_AsReplica", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VirtualMachine[] VM { get; set; }

	[Alias(new string[] { "ReplicaServer" })]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName", Position = 1, Mandatory = true)]
	[Parameter(ParameterSetName = "VMObject", Position = 1, Mandatory = true)]
	public string ReplicaServerName { get; set; }

	[Alias(new string[] { "ReplicaPort" })]
	[ValidateRange(1, 65535)]
	[Parameter(ParameterSetName = "VMName", Position = 2, Mandatory = true)]
	[Parameter(ParameterSetName = "VMObject", Position = 2, Mandatory = true)]
	public int ReplicaServerPort { get; set; }

	[Alias(new string[] { "AuthType" })]
	[ValidateNotNull]
	[Parameter(ParameterSetName = "VMName", Position = 3, Mandatory = true)]
	[Parameter(ParameterSetName = "VMObject", Position = 3, Mandatory = true)]
	public ReplicationAuthenticationType AuthenticationType { get; set; }

	[Alias(new string[] { "Thumbprint", "Cert" })]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName", ValueFromPipelineByPropertyName = true)]
	[Parameter(ParameterSetName = "VMObject", ValueFromPipelineByPropertyName = true)]
	public string CertificateThumbprint { get; set; }

	[ValidateNotNull]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMObject")]
	public bool? CompressionEnabled { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Kvp", Justification = "This is by spec.")]
	[ValidateNotNull]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMObject")]
	public bool? ReplicateHostKvpItems { get; set; }

	[ValidateNotNull]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMObject")]
	public bool? BypassProxyServer { get; set; }

	[ValidateNotNull]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMObject")]
	public bool? EnableWriteOrderPreservationAcrossDisks { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "VSS", Justification = "This is per spec.")]
	[Alias(new string[] { "VSSFreq" })]
	[ValidateRange(1, 12)]
	[ValidateNotNull]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMObject")]
	public int? VSSSnapshotFrequencyHour { get; set; }

	[Alias(new string[] { "RecHist" })]
	[ValidateRange(0, 24)]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMObject")]
	public int? RecoveryHistory { get; set; }

	[Alias(new string[] { "RepFreq" })]
	[ValidateRange(30, 900)]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMObject")]
	public int? ReplicationFrequencySec { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Usability is more important than the slight gain in efficiency here.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMObject")]
	public HardDiskDrive[] ExcludedVhd { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Usability is more important than the slight gain in efficiency here.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMObject")]
	public string[] ExcludedVhdPath { get; set; }

	[Alias(new string[] { "AutoResync" })]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMObject")]
	[ValidateNotNull]
	public bool? AutoResynchronizeEnabled { get; set; }

	[Alias(new string[] { "AutoResyncStart" })]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMObject")]
	[ValidateNotNull]
	public TimeSpan? AutoResynchronizeIntervalStart { get; set; }

	[Alias(new string[] { "AutoResyncEnd" })]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMObject")]
	[ValidateNotNull]
	public TimeSpan? AutoResynchronizeIntervalEnd { get; set; }

	[Parameter(ParameterSetName = "VMName_AsReplica")]
	[Parameter(ParameterSetName = "VMObject_AsReplica")]
	public SwitchParameter AsReplica { get; set; }

	[Alias(new string[] { "AllowedPS" })]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName_AsReplica", ValueFromPipelineByPropertyName = true)]
	[Parameter(ParameterSetName = "VMObject_AsReplica", ValueFromPipelineByPropertyName = true)]
	public string AllowedPrimaryServer { get; set; }

	[Parameter]
	public SwitchParameter AsJob { get; set; }

	[Parameter]
	public SwitchParameter Passthru { get; set; }

	protected override void ValidateParameters()
	{
		if (AsReplica.IsPresent)
		{
			return;
		}
		if (!string.IsNullOrEmpty(AllowedPrimaryServer))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidArgument_ParameterNotProvided, "AllowedPrimaryServer"));
		}
		if (Uri.CheckHostName(ReplicaServerName) != UriHostNameType.Dns)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_InvalidUri, "ReplicaServerName"), new UriFormatException("ReplicaServerName"));
		}
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
		if (VSSSnapshotFrequencyHour.HasValue && (!RecoveryHistory.HasValue || RecoveryHistory.Value == 0))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMReplication_AppConsistentWithoutRecoveryHistory);
		}
		if (ReplicationFrequencySec.HasValue && !VMReplication.IsValidReplicationFrequency(ReplicationFrequencySec.Value))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidArgument_ReplicationFrequencyNotValid);
		}
	}

	internal override IList<VirtualMachine> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		IList<VirtualMachine> inputs = ((CurrentParameterSetIs("VMName") || CurrentParameterSetIs("VMObject")) ? ParameterResolvers.ResolveVirtualMachines(this, operationWatcher) : ((!CurrentParameterSetIs("VMObject_AsReplica")) ? VirtualizationObjectLocator.GetVirtualMachinesByNamesAndServers(ParameterResolvers.GetServers(this, operationWatcher), VMName, allowWildcards: true, ErrorDisplayMode.WriteError, operationWatcher) : VM));
		return inputs.SelectWithLogging(ValidateVirtualMachine, operationWatcher).ToList();
	}

	internal override void ProcessOneOperand(VirtualMachine operand, IOperationWatcher operationWatcher)
	{
		string format = (AsReplica ? CmdletResources.ShouldProcess_EnableVMReplication_AsReplica : CmdletResources.ShouldProcess_EnableVMReplication);
		if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, format, operand.Name)))
		{
			return;
		}
		VMReplication vMReplication = VMReplication.GetVMReplication(operand, VMReplicationRelationshipType.Simple);
		if ((bool)AsReplica)
		{
			operand.SetReplicationStateEx(vMReplication, ReplicationWmiState.WaitingToCompleteInitialReplication, operationWatcher);
			if (!string.IsNullOrEmpty(AllowedPrimaryServer))
			{
				VMReplicationServer.GetReplicationServer(operand.Server).SetAuthorizationEntry(operand, authorizationEntry.AllowedPrimaryServer, authorizationEntry.ReplicaStorageLocation, authorizationEntry.TrustGroup, operationWatcher);
			}
		}
		else
		{
			if (operand.ReplicationMode == VMReplicationMode.Replica)
			{
				vMReplication = VMReplication.GetVMReplication(operand, VMReplicationRelationshipType.Extended);
			}
			VMReplication.ValidateReplicaServerName(vMReplication);
			vMReplication.ReplicaServerName = ReplicaServerName;
			ValidateAndSetReplicationProperties(vMReplication, operationWatcher);
			VMReplicationServer.GetReplicationServer(operand.Server).CreateReplicationRelationship(vMReplication, operationWatcher);
		}
		if ((bool)Passthru)
		{
			operationWatcher.WriteObject(vMReplication);
		}
	}

	private void ValidateAndSetExcludeDisk(VMReplication replication)
	{
		bool flag = !ExcludedVhd.IsNullOrEmpty();
		bool flag2 = !ExcludedVhdPath.IsNullOrEmpty();
		HardDiskDrive[] array = replication.VirtualMachine.GetVirtualHardDiskDrives().ToArray();
		if (!flag && !flag2)
		{
			replication.ReplicatedDisks = array;
			return;
		}
		List<HardDiskDrive> list = new List<HardDiskDrive>();
		if (flag)
		{
			HardDiskDrive[] excludedVhd = ExcludedVhd;
			foreach (HardDiskDrive hardDiskDrive in excludedVhd)
			{
				HardDiskDrive currentDisk = hardDiskDrive;
				if (!array.Any((HardDiskDrive vmDisk) => string.Equals(currentDisk.Path, vmDisk.Path)))
				{
					throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplication_HardDiskNotAttachedToVM, hardDiskDrive.Path, replication.VMName));
				}
			}
			list.AddRange(ExcludedVhd);
		}
		if (flag2)
		{
			string[] excludedVhdPath = ExcludedVhdPath;
			foreach (string text in excludedVhdPath)
			{
				string diskPath = text.Trim();
				HardDiskDrive item;
				if ((item = array.FirstOrDefault((HardDiskDrive vmDisk) => string.Equals(diskPath, vmDisk.Path))) == null)
				{
					throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplication_HardDiskNotAttachedToVM, diskPath, replication.VMName));
				}
				list.Add(item);
			}
		}
		replication.ExcludedDisks = list;
	}

	private void ValidateAndSetReplicationProperties(VMReplication replication, IOperationWatcher watcher)
	{
		replication.ReplicaServerPort = ReplicaServerPort;
		replication.AuthenticationType = AuthenticationType;
		replication.CertificateThumbprint = CertificateThumbprint;
		if (RecoveryHistory.HasValue)
		{
			replication.RecoveryHistory = RecoveryHistory.Value;
		}
		bool flag = replication.ReplicationRelationshipType == VMReplicationRelationshipType.Extended;
		if (ReplicationFrequencySec.HasValue)
		{
			if (flag)
			{
				if (ReplicationFrequencySec.Value < replication.ReplicationFrequencySec)
				{
					throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidArgument_ReplicationFrequencyNotValid);
				}
				if (ReplicationFrequencySec.Value == 30)
				{
					throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplication_InvalidExtendedReplicationFrequency, ReplicationFrequencySec.Value));
				}
			}
			replication.ReplicationFrequencySec = ReplicationFrequencySec.Value;
		}
		if (!flag)
		{
			if (VSSSnapshotFrequencyHour.HasValue)
			{
				replication.VSSSnapshotFrequencyHour = VSSSnapshotFrequencyHour.Value;
			}
			ValidateAndSetExcludeDisk(replication);
		}
		if (CompressionEnabled.HasValue)
		{
			replication.CompressionEnabled = CompressionEnabled.Value;
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
		if (BypassProxyServer.HasValue)
		{
			replication.BypassProxyServer = BypassProxyServer.Value;
		}
		if (EnableWriteOrderPreservationAcrossDisks.HasValue)
		{
			watcher.WriteWarning(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidArgument_ParameterNotRequired, "EnableWriteOrderPreservationAcrossDisks", "Enable-VMReplication"));
		}
		replication.EnableWriteOrderPreservationAcrossDisks = true;
	}

	private VirtualMachine ValidateVirtualMachine(VirtualMachine vm)
	{
		switch (vm.ReplicationMode)
		{
		case VMReplicationMode.Primary:
			throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplication_AlreadyEnabled, vm.Name));
		case VMReplicationMode.TestReplica:
		case VMReplicationMode.ExtendedReplica:
		{
			VMReplication vMReplication = VMReplication.GetVMReplication(vm, VMReplicationRelationshipType.Simple);
			VMReplication.ReportInvalidModeError("Enable-VMReplication", vMReplication);
			break;
		}
		case VMReplicationMode.Replica:
		{
			if ((bool)AsReplica)
			{
				throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplication_ActionNotApplicableOnReplica, vm.Name));
			}
			VMReplication vMReplication2 = VMReplication.GetVMReplication(vm, VMReplicationRelationshipType.Extended);
			if (vMReplication2 != null && vMReplication2.IsEnabled)
			{
				throw ExceptionHelper.CreateInvalidStateException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplication_AlreadyEnabled, vm.Name), null, null);
			}
			break;
		}
		case VMReplicationMode.None:
			if ((bool)AsReplica)
			{
				VMReplicationServer replicationServer = VMReplicationServer.GetReplicationServer(vm.Server);
				if (!replicationServer.TryFindAuthorizationEntry("*", out authorizationEntry) && string.IsNullOrEmpty(AllowedPrimaryServer))
				{
					throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidArgument_ParameterNotProvided, "AllowedPrimaryServer"));
				}
				if (!string.IsNullOrEmpty(AllowedPrimaryServer) && !replicationServer.TryFindAuthorizationEntry(AllowedPrimaryServer, out authorizationEntry))
				{
					throw ExceptionHelper.CreateOperationFailedException(CmdletErrorMessages.VMReplication_AuthorizationEntryNotFound);
				}
			}
			break;
		}
		return vm;
	}
}
