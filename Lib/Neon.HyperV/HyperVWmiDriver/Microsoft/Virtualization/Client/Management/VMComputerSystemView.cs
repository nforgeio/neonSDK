#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Management.Infrastructure;

namespace Microsoft.Virtualization.Client.Management;

internal class VMComputerSystemView : VMComputerSystemBaseView, IVMComputerSystem, IVMComputerSystemBase, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable, IMetricMeasurableElement
{
	public IFailoverReplicationAuthorizationSetting ReplicationAuthorizationSetting => GetRelatedObject<IFailoverReplicationAuthorizationSetting>(base.Associations.ElementSettingData, throwIfNotFound: false);

	public FailoverReplicationMode ReplicationMode => (FailoverReplicationMode)GetProperty<ushort>("ReplicationMode");

	public IVMComputerSystem TestReplicaSystem => GetRelatedObject<IVMComputerSystem>(base.Associations.ReplicaSystemDependency, throwIfNotFound: false);

	public EnhancedSessionModeStateType EnhancedSessionModeState
	{
		get
		{
			try
			{
				return (EnhancedSessionModeStateType)GetProperty<ushort>("EnhancedSessionModeState");
			}
			catch (Exception)
			{
				return EnhancedSessionModeStateType.Disabled;
			}
		}
	}

	public IEnumerable<IVMCollection> CollectingCollections => GetRelatedObjects<IVMCollection>(base.Associations.VirtualMachineCollections);

	public MetricEnabledState AggregateMetricEnabledState => MetricServiceView.CalculateAggregatedMetricEnabledState(GetRelatedObjects<IMeasuredElementToMetricDefinitionAssociation>(base.Associations.MeasuredElementToMetricDefRelationship));

	public event SnapshotCreatedEventHandler SnapshotCreated
	{
		add
		{
			if (m_SnapshotCreated == null && value != null)
			{
				InstanceEventManager.GetInstanceEventMonitor(base.SnapshotCreationKey)?.RegisterObject(base.SnapshotCreationKey, OnSnapshotCreated);
			}
			m_SnapshotCreated = (SnapshotCreatedEventHandler)Delegate.Combine(m_SnapshotCreated, value);
		}
		remove
		{
			if (m_SnapshotCreated != null)
			{
				m_SnapshotCreated = (SnapshotCreatedEventHandler)Delegate.Remove(m_SnapshotCreated, value);
				if (m_SnapshotCreated == null)
				{
					InstanceEventManager.GetInstanceEventMonitor(base.SnapshotCreationKey).UnregisterObject(base.SnapshotCreationKey, OnSnapshotCreated);
				}
			}
		}
	}

	private void OnSnapshotCreated(object sender, InstanceEventArrivedArgs eventArrivedArgs)
	{
		if (m_SnapshotCreated != null)
		{
			ICimInstance cimInstance = ((CimInstance)eventArrivedArgs.InstanceEvent.CimInstanceProperties["TargetInstance"].Value).ToICimInstance();
			string text = cimInstance.CimInstanceProperties["InstanceID"].Value as string;
			VMTrace.TraceWmiEvent(string.Format(CultureInfo.InvariantCulture, "ICE snapshot '{0}' created.", text));
			IVMComputerSystemSetting vMComputerSystemSetting = ObjectLocator.GetVMComputerSystemSetting(base.Server, text, cimInstance);
			if (vMComputerSystemSetting != null)
			{
				m_SnapshotCreated(this, vMComputerSystemSetting);
			}
		}
	}

	public IVMTask BeginSetReplicationStateEx(IVMReplicationRelationship replicationRelationship, FailoverReplicationState state)
	{
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.RequestReplicationStateChangeFailed, base.Name);
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Changing replication state of '{0}' to '{1}'", base.ManagementPath, state));
		object[] array = new object[4]
		{
			replicationRelationship?.GetEmbeddedInstance(),
			(ushort)state,
			null,
			null
		};
		uint result = InvokeMethod("RequestReplicationStateChangeEx", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[2]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndSetReplicationState(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.SetState);
		VMTrace.TraceUserActionCompleted("Replication state change completed successfully.");
	}

	public IVMTask BeginTakeSnapshot()
	{
		return BeginTakeSnapshot(takeAutomaticSnapshot: false);
	}

	public IVMTask BeginTakeSnapshot(bool takeAutomaticSnapshot)
	{
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.TakeSnapshotFailed, base.Name);
		IProxy snapshotServiceProxy = GetSnapshotServiceProxy();
		SnapshotType snapshotType = (takeAutomaticSnapshot ? SnapshotType.Automatic : SnapshotType.Regular);
		object[] array = new object[5]
		{
			base.ManagementPath,
			string.Empty,
			(ushort)snapshotType,
			null,
			null
		};
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Taking snapshot of virtual machine '{0}' ('{1}').", base.InstanceId, base.Name));
		uint result = snapshotServiceProxy.InvokeMethod("CreateSnapshot", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, array[3], array[4]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public IVMComputerSystemSetting EndTakeSnapshot(IVMTask snapshotTask)
	{
		IVMComputerSystemSetting result = EndMethodReturn<IVMComputerSystemSetting>(snapshotTask, VirtualizationOperation.TakeSnapshot, throwIfAffectedElementNotFound: false);
		VMTrace.TraceUserActionCompleted("Snapshot had been taken successfully.");
		return result;
	}

	public IVMTask BeginInjectNonMaskableInterrupt()
	{
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.InjectNonMaskableInterruptFailed, base.Name);
		object[] array = new object[1];
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Injecting NMI into VM '{0}' ('{1}')", base.InstanceId, base.Name));
		uint result = InvokeMethod("InjectNonMaskableInterrupt", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[0]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndInjectNonMaskableInterrupt(IVMTask injectNonMaskableInterruptTask)
	{
		EndMethod(injectNonMaskableInterruptTask, VirtualizationOperation.InjectNonMaskableInterrupt);
		VMTrace.TraceUserActionCompleted(string.Format(CultureInfo.InvariantCulture, "Non-maskable interrupt injected into virtual machine '{0}' ('{1}').", base.InstanceId, base.Name));
	}

	public IVMTask BeginUpgrade()
	{
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UpgradeVMConfigurationVersionFailed, base.Name);
		IProxy serviceProxy = GetServiceProxy();
		object[] array = new object[3]
		{
			this,
			string.Empty,
			null
		};
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Upgrading virtual machine '{0}' ('{1}').", base.InstanceId, base.Name));
		uint result = serviceProxy.InvokeMethod("UpgradeSystemVersion", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[2]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndUpgrade(IVMTask upgradeTask)
	{
		EndMethod(upgradeTask, VirtualizationOperation.Upgrade);
		VMTrace.TraceUserActionCompleted("Upgrade had been taken successfully.");
	}

	public ISummaryInformation GetVMSummaryInformation(SummaryInformationRequest requestedInformation)
	{
		VMTrace.TraceWmiGetSummaryInformation(base.Server, new IVMComputerSystem[1] { this }, requestedInformation);
		WmiOperationOptions options = new WmiOperationOptions
		{
			KeysOnly = true
		};
		ISummaryInformation summaryInformation = GetRelatedObject<ISummaryInformation>(base.Associations.SystemToSummary, throwIfNotFound: false, options);
		if (summaryInformation != null)
		{
			summaryInformation.UpdatePropertyCache(requestedInformation);
		}
		else
		{
			VMTrace.TraceWarning("GetSummaryInformation (VM) WMI method returned an empty SummaryInformation object. This usually means either the vm object no longer exists, or we do not have access to it.");
			summaryInformation = null;
		}
		return summaryInformation;
	}

	public ReplicationHealthInformation GetVMReplicationStatisticsEx(IVMReplicationRelationship replicationRelationship)
	{
		if (replicationRelationship == null)
		{
			throw new ArgumentNullException("replicationRelationship");
		}
		object[] array = new object[5]
		{
			this,
			replicationRelationship.GetEmbeddedInstance(),
			null,
			null,
			null
		};
		uint num = GetFailoverReplicationServiceProxy().InvokeMethod("GetReplicationStatisticsEx", array);
		ReplicationHealthInformation replicationHealthInformation = null;
		if (num == View.ErrorCodeSuccess && array[2] != null)
		{
			replicationHealthInformation = new ReplicationHealthInformation();
			string[] obj = (string[])array[2];
			string[] array2 = (string[])array[3];
			string[] array3 = obj;
			foreach (string text in array3)
			{
				if (!string.IsNullOrEmpty(text))
				{
					ReplicationStatistics item = EmbeddedInstance.ConvertTo<ReplicationStatistics>(base.Server, text);
					replicationHealthInformation.ReplicationStatistics.Add(item);
				}
			}
			for (int j = 0; j < array2.Length; j++)
			{
				if (!string.IsNullOrEmpty(array2[j]))
				{
					MsvmError item2 = EmbeddedInstance.ConvertTo<MsvmError>(base.Server, array2[j]);
					replicationHealthInformation.HealthMessages.Add(item2);
				}
			}
		}
		else if (array[2] != null)
		{
			VMTrace.TraceError("GetReplicationStatisticsEx (VM, ReplicationRelationship) WMI method call failed!");
		}
		return replicationHealthInformation;
	}

	public void RemoveKvpItem(string name, KvpItemPool pool)
	{
		if (!string.IsNullOrEmpty(name))
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			dictionary.Add("Name", name);
			dictionary.Add("Data", string.Empty);
			dictionary.Add("Source", (int)pool);
			string[] array = new string[1] { base.Server.GetNewEmbeddedInstance("Msvm_KvpExchangeDataItem", dictionary) };
			object[] array2 = new object[3] { this, array, null };
			uint num = GetServiceProxy().InvokeMethod("RemoveKvpItems", array2);
			if (num == View.ErrorCodeJob)
			{
				IVMTask taskFromPath = GetTaskFromPath((WmiObjectPath)array2[2]);
				taskFromPath.WaitForCompletion();
				num = (uint)taskFromPath.ErrorCode;
			}
			if (num != View.ErrorCodeSuccess)
			{
				ErrorCodeMapper errorCodeMapper = GetErrorCodeMapper();
				throw ThrowHelper.CreateVirtualizationOperationFailedException(ErrorMessages.RemoveKvpItemsFailed, VirtualizationOperation.RemoveKvpItem, num, errorCodeMapper, null);
			}
		}
	}

	public bool IsExtendedReplicationEnabled()
	{
		if (base.VMExtendedReplicationRelationship != null)
		{
			return base.VMExtendedReplicationRelationship.ReplicationState != FailoverReplicationState.Disabled;
		}
		return false;
	}

	public bool IsSnapshotAvailable()
	{
		bool result = false;
		if (base.Setting.UserSnapshotType == UserSnapshotType.ProductionOnly)
		{
			result = IsProductionSnapshotAvailable();
		}
		else if (base.Setting.UserSnapshotType == UserSnapshotType.Production || base.Setting.UserSnapshotType == UserSnapshotType.Standard)
		{
			VMComputerSystemState state = base.State;
			if (state == VMComputerSystemState.Running || state == VMComputerSystemState.Paused || state == VMComputerSystemState.PowerOff || state == VMComputerSystemState.Saved || state == VMComputerSystemState.Hibernated)
			{
				result = true;
			}
		}
		return result;
	}

	public bool IsProductionSnapshotAvailable()
	{
		bool result = false;
		switch (base.State)
		{
		case VMComputerSystemState.PowerOff:
			result = true;
			break;
		case VMComputerSystemState.Running:
		{
			IVMVssComponent vssComponent = base.VssComponent;
			if (vssComponent != null)
			{
				VMIntegrationComponentOperationalStatus[] operationalStatus = vssComponent.GetOperationalStatus();
				if (operationalStatus != null && operationalStatus.Length != 0 && (operationalStatus[0] == VMIntegrationComponentOperationalStatus.Ok || operationalStatus[0] == VMIntegrationComponentOperationalStatus.Degraded))
				{
					result = true;
				}
			}
			break;
		}
		}
		return result;
	}

	public bool WasOnlineProductionSnapshot()
	{
		if ((base.Setting.UserSnapshotType == UserSnapshotType.Production || base.Setting.UserSnapshotType == UserSnapshotType.ProductionOnly) && IsProductionSnapshotAvailable())
		{
			return base.State == VMComputerSystemState.Running;
		}
		return false;
	}

	public bool IsUpgradable()
	{
		bool result = false;
		IVMComputerSystemSetting defaultComputerSystemSetting = base.HostSystem.VirtualizationService.AllCapabilities.DefaultComputerSystemSetting;
		ISummaryInformation vMSummaryInformation = GetVMSummaryInformation(SummaryInformationRequest.Detail);
		if (!string.IsNullOrEmpty(defaultComputerSystemSetting.Version) && !string.IsNullOrEmpty(vMSummaryInformation.Version))
		{
			Version version = new Version(vMSummaryInformation.Version);
			Version version2 = new Version(defaultComputerSystemSetting.Version);
			result = version < version2;
		}
		return result;
	}

	public IEnumerable<ITerminalConnection> GetTerminalConnections()
	{
		return GetRelatedObjects<ITerminalConnection>(base.Associations.TerminalConnections);
	}

	public bool DoesTerminalConnectionExist()
	{
		using IEnumerator<ITerminalConnection> enumerator = GetTerminalConnections().GetEnumerator();
		return enumerator.MoveNext();
	}

	public IEnumerable<IVMMigrationTask> GetMigrationTasks()
	{
		return GetRelatedObjects<IVMMigrationTask>(base.Associations.VirtualMachineToMigrationJob);
	}

	public IReadOnlyCollection<IMetricValue> GetMetricValues()
	{
		return GetRelatedObjects<IMetricValue>(base.Associations.MeasuredElementToMetricValue).ToList();
	}
}
