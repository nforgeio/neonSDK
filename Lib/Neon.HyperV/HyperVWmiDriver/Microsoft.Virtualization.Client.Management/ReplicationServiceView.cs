#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class ReplicationServiceView : View, IReplicationService, IVirtualizationManagementObject
{
	internal static class WmiMemberNames
	{
		public const string AddAuthorizationEntry = "AddAuthorizationEntry";

		public const string ChangeReplicationModeToPrimary = "ChangeReplicationModeToPrimary";

		public const string CommitFailover = "CommitFailover";

		public const string CreateReplicationRelationship = "CreateReplicationRelationship";

		public const string GetSystemCertificates = "GetSystemCertificates";

		public const string ImportInitialReplica = "ImportInitialReplica";

		public const string InitiateFailover = "InitiateFailover";

		public const string ModifyAuthorizationEntry = "ModifyAuthorizationEntry";

		public const string OperationalStatus = "OperationalStatus";

		public const string RemoveAuthorizationEntry = "RemoveAuthorizationEntry";

		public const string RemoveReplicationRelationshipEx = "RemoveReplicationRelationshipEx";

		public const string ResetReplicationStatisticsEx = "ResetReplicationStatisticsEx";

		public const string Resynchronize = "Resynchronize";

		public const string ReverseReplicationRelationship = "ReverseReplicationRelationship";

		public const string RevertFailover = "RevertFailover";

		public const string SetAuthorizationEntry = "SetAuthorizationEntry";

		public const string StartReplication = "StartReplication";

		public const string StatusDescriptions = "StatusDescriptions";

		public const string TestReplicaSystem = "TestReplicaSystem";

		public const string TestReplicationConnection = "TestReplicationConnection";
	}

	public ushort[] OperationalStatus => GetProperty<ushort[]>("OperationalStatus");

	public string[] StatusDescriptions => GetProperty<string[]>("StatusDescriptions");

	public IFailoverReplicationServiceSetting Setting => GetRelatedObject<IFailoverReplicationServiceSetting>(base.Associations.FrServiceToSetting);

	public IEnumerable<IFailoverReplicationAuthorizationSetting> AuthorizationSettings => GetRelatedObjects<IFailoverReplicationAuthorizationSetting>(base.Associations.FrServiceToAuthSetting);

	public IVMTask BeginStartReplication(IVMComputerSystem computerSystem, InitialReplicationType initialReplicationType, string initialReplicationShare, DateTime scheduledDateTime)
	{
		if (computerSystem == null)
		{
			throw new ArgumentNullException("computerSystem");
		}
		if (initialReplicationType == InitialReplicationType.Invalid || initialReplicationType > InitialReplicationType.UsingBackup)
		{
			throw new ArgumentOutOfRangeException("initialReplicationType");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.StartReplicationFailed, computerSystem.Name);
		if (string.IsNullOrEmpty(initialReplicationShare))
		{
			VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting in-band virtual system replication of '{0}'", computerSystem.ManagementPath));
		}
		else
		{
			VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting virtual system replication of '{0}' to '{1}'", computerSystem.ManagementPath, initialReplicationShare));
		}
		object[] array = new object[5]
		{
			computerSystem,
			initialReplicationType,
			initialReplicationShare,
			(scheduledDateTime != DateTime.MinValue) ? new DateTime?(scheduledDateTime) : null,
			null
		};
		uint result = InvokeMethod("StartReplication", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[4]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndStartReplication(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.StartReplication);
		VMTrace.TraceUserActionCompleted("Start virtual system replication completed successfully.");
	}

	public IVMTask BeginImportInitialReplica(IVMComputerSystem computerSystem, string initialReplicationImportLocation)
	{
		if (computerSystem == null)
		{
			throw new ArgumentNullException("computerSystem");
		}
		if (string.IsNullOrEmpty(initialReplicationImportLocation))
		{
			throw new ArgumentNullException("initialReplicationImportLocation");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ImportReplicationFailed, computerSystem.Name, initialReplicationImportLocation);
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Importing virtual system replication of '{0}' from '{1}'", computerSystem.ManagementPath.ToString(), initialReplicationImportLocation));
		object[] array = new object[3] { computerSystem, initialReplicationImportLocation, null };
		uint result = InvokeMethod("ImportInitialReplica", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[2]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndImportInitialReplica(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.ImportInitialReplica);
		VMTrace.TraceUserActionCompleted("Import virtual system replication completed successfully.");
	}

	public IVMTask BeginRemoveReplicationRelationshipEx(IVMComputerSystem computerSystem, IVMReplicationRelationship replicationRelationship)
	{
		if (computerSystem == null)
		{
			throw new ArgumentNullException("computerSystem");
		}
		if (replicationRelationship == null)
		{
			throw new ArgumentNullException("replicationRelationship");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.RemoveReplicationSettingsFailed, computerSystem.Name);
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Removing virtual system replication settings '{0}'", computerSystem));
		object[] array = new object[3]
		{
			computerSystem,
			replicationRelationship.GetEmbeddedInstance(),
			null
		};
		uint result = InvokeMethod("RemoveReplicationRelationshipEx", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[2]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndRemoveReplicationRelationshipEx(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.RemoveReplication);
		VMTrace.TraceUserActionCompleted("Remove virtual system replication completed successfully.");
	}

	public IVMTask BeginChangeReplicationModeToPrimary(IVMComputerSystem computerSystem, IVMReplicationRelationship replicationRelationship)
	{
		if (computerSystem == null)
		{
			throw new ArgumentNullException("computerSystem");
		}
		if (replicationRelationship == null)
		{
			throw new ArgumentNullException("replicationRelationship");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ChangeReplicationModeToPrimaryFailed, computerSystem.Name);
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Changing replication mode to primary for virtual system '{0}'", computerSystem.ManagementPath.ToString()));
		object[] array = new object[3]
		{
			computerSystem,
			replicationRelationship.GetEmbeddedInstance(),
			null
		};
		uint result = InvokeMethod("ChangeReplicationModeToPrimary", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[2]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndChangeReplicationModeToPrimary(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.ChangeReplicationModeToPrimary);
		VMTrace.TraceUserActionCompleted("Change replication mode to primary completed successfully.");
	}

	public IVMTask BeginAddAuthorizationEntry(ReplicationAuthorizationEntry replicationAuthEntry)
	{
		if (replicationAuthEntry == null)
		{
			throw new ArgumentNullException("replicationAuthEntry");
		}
		VMTrace.TraceUserActionInitiated("Adding failover replication authorization entry");
		object[] array = new object[2] { replicationAuthEntry, null };
		uint result = InvokeMethod("AddAuthorizationEntry", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
		iVMTask.ClientSideFailedMessage = ErrorMessages.AddReplicationAuthSettingsFailed;
		return iVMTask;
	}

	public void AddAuthorizationEntry(ReplicationAuthorizationEntry replicationAuthEntry)
	{
		using IVMTask iVMTask = BeginAddAuthorizationEntry(replicationAuthEntry);
		iVMTask.WaitForCompletion();
		EndAddAuthorizationEntry(iVMTask);
	}

	public void EndAddAuthorizationEntry(IVMTask task)
	{
		if (task == null)
		{
			throw new ArgumentNullException("task");
		}
		EndMethod(task, VirtualizationOperation.AddAuthorization);
		if (task.ErrorCode == View.ErrorCodeSuccess)
		{
			VMTrace.TraceUserActionCompleted("Add Authorization entry completed successfully.");
		}
	}

	public void ModifyAuthorizationEntry(ReplicationAuthorizationEntry replicationAuthEntry)
	{
		if (replicationAuthEntry == null)
		{
			throw new ArgumentNullException("replicationAuthEntry");
		}
		VMTrace.TraceUserActionInitiated("Modifying failover replication authorization entry");
		object[] array = new object[2] { replicationAuthEntry, null };
		uint num = InvokeMethod("ModifyAuthorizationEntry", array);
		using IVMTask iVMTask = BeginMethodTaskReturn(num, null, array[1]);
		iVMTask.ClientSideFailedMessage = ErrorMessages.AddReplicationAuthSettingsFailed;
		iVMTask.WaitForCompletion();
		if (num == View.ErrorCodeSuccess)
		{
			VMTrace.TraceUserActionCompleted("Modifying Authorization entry completed successfully.");
		}
		else
		{
			EndMethod(iVMTask, VirtualizationOperation.AddAuthorization);
		}
	}

	public IVMTask BeginCreateReplicationRelationship(IVMComputerSystem computerSystem, IVMReplicationSettingData replicationSettingData)
	{
		if (computerSystem == null)
		{
			throw new ArgumentNullException("computerSystem");
		}
		if (replicationSettingData == null)
		{
			throw new ArgumentNullException("replicationSettingData");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.AddReplicationSettingsFailed, computerSystem.Name);
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Adding virtual system replication settings '{0}'", computerSystem.ManagementPath.ToString()));
		string embeddedInstance = replicationSettingData.GetEmbeddedInstance();
		object[] array = new object[3] { computerSystem, embeddedInstance, null };
		uint result = InvokeMethod("CreateReplicationRelationship", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[2]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndCreateReplicationRelationship(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.CreateReplication);
		VMTrace.TraceUserActionCompleted("Create virtual system replication settings completed successfully.");
	}

	public IVMTask BeginReverseReplicationRelationship(IVMComputerSystem computerSystem, IVMReplicationSettingData replicationSettingData)
	{
		if (computerSystem == null)
		{
			throw new ArgumentNullException("computerSystem");
		}
		if (replicationSettingData == null)
		{
			throw new ArgumentNullException("replicationSettingData");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ReverseReplicationSettingsFailed, computerSystem.Name);
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Reversing virtual system replication settings '{0}'", computerSystem.ManagementPath.ToString()));
		string embeddedInstance = replicationSettingData.GetEmbeddedInstance();
		object[] array = new object[3] { computerSystem, embeddedInstance, null };
		uint result = InvokeMethod("ReverseReplicationRelationship", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[2]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndReverseReplicationRelationship(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.ReverseReplication);
		VMTrace.TraceUserActionCompleted("Reverse virtual system replication settings completed successfully.");
	}

	public IVMTask BeginResynchronizeReplication(IVMComputerSystem computerSystem, DateTime scheduledDateTime)
	{
		if (computerSystem == null)
		{
			throw new ArgumentNullException("computerSystem");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ResynchronizationFailed, computerSystem.Name);
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting resynchronize of virtual system replication '{0}'", computerSystem.ManagementPath.ToString()));
		object[] array = new object[3]
		{
			computerSystem,
			(scheduledDateTime != DateTime.MinValue) ? new DateTime?(scheduledDateTime) : null,
			null
		};
		uint result = InvokeMethod("Resynchronize", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[2]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndResynchronizeReplication(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.ResynchronizeReplication);
		VMTrace.TraceUserActionCompleted("Resynchronize replication for virtual system completed successfully.");
	}

	public IVMTask BeginResetReplicationStatisticsEx(IVMComputerSystem computerSystem, IVMReplicationRelationship replicationRelationship)
	{
		if (computerSystem == null)
		{
			throw new ArgumentNullException("computerSystem");
		}
		if (replicationRelationship == null)
		{
			throw new ArgumentNullException("replicationRelationship");
		}
		VMTrace.TraceUserActionInitiated("Starting reset replication statistics ex.");
		object[] array = new object[3]
		{
			computerSystem,
			replicationRelationship.GetEmbeddedInstance(),
			null
		};
		uint result = InvokeMethod("ResetReplicationStatisticsEx", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[2]);
		iVMTask.ClientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ResetReplicationStatisticsFailed, computerSystem.Name);
		return iVMTask;
	}

	public void ResetReplicationStatisticsEx(IVMComputerSystem computerSystem, IVMReplicationRelationship replicationRelationship)
	{
		using IVMTask iVMTask = BeginResetReplicationStatisticsEx(computerSystem, replicationRelationship);
		iVMTask.WaitForCompletion();
		EndResetReplicationStatisticsEx(iVMTask);
	}

	public void EndResetReplicationStatisticsEx(IVMTask task)
	{
		if (task == null)
		{
			throw new ArgumentNullException("task");
		}
		EndMethod(task, VirtualizationOperation.ResetReplicationStatistics);
		if (task.ErrorCode == View.ErrorCodeSuccess)
		{
			VMTrace.TraceUserActionCompleted("Resetting replication statistics completed successfully");
		}
	}

	public string[] GetSystemCertificates()
	{
		VMTrace.TraceUserActionInitiated("Getting replication certificates");
		object[] array = new object[1];
		uint num = InvokeMethod("GetSystemCertificates", array);
		string[] result = null;
		if (num == View.ErrorCodeSuccess && array[0] != null)
		{
			result = (string[])array[0];
		}
		VMTrace.TraceUserActionCompleted("Getting replication certificates completed successfully");
		return result;
	}

	public IVMTask BeginInitiateFailover(IVMComputerSystem computerSystem, IVMComputerSystemSetting snapshot)
	{
		if (computerSystem == null)
		{
			throw new ArgumentNullException("computerSystem");
		}
		string arg = snapshot?.ManagementPath.ToString();
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ApplyReplicaFailed, arg, computerSystem.Name);
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Applying replica {0} to virtual system '{1}'", arg, computerSystem.ManagementPath.ToString()));
		object[] array = new object[3] { computerSystem, snapshot, null };
		uint result = InvokeMethod("InitiateFailover", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[2]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndInitiateFailover(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.ApplyReplica);
		VMTrace.TraceUserActionCompleted("Apply replica snapshot for virtual system completed successfully.");
	}

	public IVMTask BeginRevertFailover(IVMComputerSystem computerSystem)
	{
		if (computerSystem == null)
		{
			throw new ArgumentNullException("computerSystem");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.RevertReplicaFailed, computerSystem.Name);
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Reverting replica for virtual system '{0}'", computerSystem.ManagementPath.ToString()));
		object[] array = new object[2] { computerSystem, null };
		uint result = InvokeMethod("RevertFailover", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndRevertFailover(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.RevertReplica);
		VMTrace.TraceUserActionCompleted("Revert replica for virtual system completed successfully.");
	}

	public IVMTask BeginCommitFailover(IVMComputerSystem computerSystem)
	{
		if (computerSystem == null)
		{
			throw new ArgumentNullException("computerSystem");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.CommitReplicaFailed, computerSystem.Name);
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Committing replica for virtual system '{0}'", computerSystem.ManagementPath.ToString()));
		object[] array = new object[2] { computerSystem, null };
		uint result = InvokeMethod("CommitFailover", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndCommitFailover(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.CommitReplica);
		VMTrace.TraceUserActionCompleted("Commit replica for virtual system completed successfully.");
	}

	public IVMTask BeginCreateTestVirtualSystem(IVMComputerSystem computerSystem, IVMComputerSystemSetting snapshot)
	{
		if (computerSystem == null)
		{
			throw new ArgumentNullException("computerSystem");
		}
		string arg = snapshot?.ManagementPath.ToString();
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.CreateTestSystemFailed, computerSystem.Name);
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Creating test virtual system of '{0}' using snapshot '{1}'", computerSystem.ManagementPath.ToString(), arg));
		object[] array = new object[4] { computerSystem, snapshot, null, null };
		uint result = InvokeMethod("TestReplicaSystem", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, array[2], array[3]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public IVMComputerSystem EndCreateTestVirtualSystem(IVMTask task, string instanceId)
	{
		IVMComputerSystem iVMComputerSystem = null;
		foreach (IVMComputerSystem item in EndMethodReturnEnumeration<IVMComputerSystem>(task, VirtualizationOperation.CreateVirtualSystem))
		{
			if (item.InstanceId != instanceId)
			{
				iVMComputerSystem = item;
				break;
			}
		}
		if (iVMComputerSystem == null)
		{
			throw ThrowHelper.CreateVirtualizationOperationFailedException(task.ClientSideFailedMessage, VirtualizationOperation.CreateVirtualSystem, 0L);
		}
		VMTrace.TraceUserActionCompleted("Creating test virtual system completed successfully.");
		return iVMComputerSystem;
	}

	public IVMTask BeginSetAuthorizationEntry(IVMComputerSystem computerSystem, ReplicationAuthorizationEntry replicationAuthEntry)
	{
		if (computerSystem == null)
		{
			throw new ArgumentNullException("computerSystem");
		}
		if (replicationAuthEntry == null)
		{
			throw new ArgumentNullException("replicationAuthEntry");
		}
		VMTrace.TraceUserActionInitiated("Setting Authorization Entry.");
		object[] array = new object[3] { computerSystem, replicationAuthEntry, null };
		uint result = InvokeMethod("SetAuthorizationEntry", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[2]);
		iVMTask.ClientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ModifyFailoverReplicationServiceSettingsFailed, computerSystem.Name);
		return iVMTask;
	}

	public void SetAuthorizationEntry(IVMComputerSystem computerSystem, ReplicationAuthorizationEntry replicationAuthEntry)
	{
		using IVMTask iVMTask = BeginSetAuthorizationEntry(computerSystem, replicationAuthEntry);
		iVMTask.WaitForCompletion();
		EndSetAuthorizationEntry(iVMTask);
	}

	public void EndSetAuthorizationEntry(IVMTask task)
	{
		if (task == null)
		{
			throw new ArgumentNullException("task");
		}
		EndMethod(task, VirtualizationOperation.SetAuthorizationEntry);
		if (task.ErrorCode == View.ErrorCodeSuccess)
		{
			VMTrace.TraceUserActionCompleted("Set Authorization entry completed successfully.");
		}
	}

	public void RemoveAuthorizationEntry(string allowedPrimaryHostSystem)
	{
		if (string.IsNullOrEmpty(allowedPrimaryHostSystem))
		{
			throw new ArgumentNullException("allowedPrimaryHostSystem");
		}
		VMTrace.TraceUserActionInitiated("Remove authorization entry.");
		object[] array = new object[2] { allowedPrimaryHostSystem, null };
		uint num = InvokeMethod("RemoveAuthorizationEntry", array);
		using IVMTask iVMTask = BeginMethodTaskReturn(num, null, array[1]);
		iVMTask.ClientSideFailedMessage = ErrorMessages.RemoveReplicationAuthSettingFailed;
		iVMTask.WaitForCompletion();
		if (num == View.ErrorCodeSuccess)
		{
			VMTrace.TraceUserActionCompleted("Remove authorization entry completed successfully.");
		}
		EndMethod(iVMTask, VirtualizationOperation.RemoveReplicationAuthorizationEntry);
	}

	public IVMTask BeginTestReplicationConnection(string recoveryConnectionPoint, ushort recoveryServerPortNumber, RecoveryAuthenticationType authenticationType, string certificateThumbPrint, bool bypassProxyServer)
	{
		object[] array = new object[6]
		{
			recoveryConnectionPoint,
			recoveryServerPortNumber,
			(ushort)authenticationType,
			certificateThumbPrint,
			bypassProxyServer,
			null
		};
		uint result = InvokeMethod("TestReplicationConnection", array);
		return BeginMethodTaskReturn(result, null, array[5]);
	}

	public void EndTestReplicationConnection(IVMTask task)
	{
		if (task == null)
		{
			throw new ArgumentNullException("task");
		}
		EndMethod(task, VirtualizationOperation.TestReplicationConnection);
	}
}
