using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client;
using Microsoft.Virtualization.Client.Management;
using Microsoft.Virtualization.Client.Management.Clustering;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMReplicationServer : VirtualizationObject, IUpdatable
{
	private readonly DataUpdater<IMSClusterReplicaBrokerResource> m_ClusterBroker;

	private readonly DataUpdater<IReplicationService> m_Service;

	private readonly DataUpdater<IFailoverReplicationServiceSetting> m_ServiceSetting;

	private volatile bool m_RequiresListenerPortMappingParse = true;

	private volatile bool m_RequiresAuthorizationEntryParse = true;

	private Hashtable m_KerberosListenerPortMapping;

	private Hashtable m_CertificateListenerPortMapping;

	private VMReplicationAuthorizationEntry[] m_ClusterAuthorizationEntries;

	public RecoveryAuthenticationType AllowedAuthenticationType
	{
		get
		{
			if (m_ClusterBroker != null)
			{
				return (RecoveryAuthenticationType)m_ClusterBroker.GetData(UpdatePolicy.EnsureUpdated).AuthenticationType;
			}
			return (RecoveryAuthenticationType)m_ServiceSetting.GetData(UpdatePolicy.EnsureUpdated).AllowedAuthenticationType;
		}
		internal set
		{
			if (m_ClusterBroker != null)
			{
				m_ClusterBroker.GetData(UpdatePolicy.None).AuthenticationType = (FailoverReplicationAuthenticationType)value;
			}
			else
			{
				m_ServiceSetting.GetData(UpdatePolicy.None).AllowedAuthenticationType = (global::Microsoft.Virtualization.Client.Management.RecoveryAuthenticationType)value;
			}
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "By spec.")]
	public VMReplicationAuthorizationEntry[] AuthorizationEntries
	{
		get
		{
			if (m_ClusterBroker != null)
			{
				IMSClusterReplicaBrokerResource data = m_ClusterBroker.GetData(UpdatePolicy.EnsureUpdated);
				if (m_RequiresAuthorizationEntryParse)
				{
					ParseClusterAuthorizationEntries(base.Server, data.Authorization, out m_ClusterAuthorizationEntries);
					m_RequiresAuthorizationEntryParse = false;
				}
				return m_ClusterAuthorizationEntries;
			}
			return m_Service.GetData(UpdatePolicy.EnsureAssociatorsUpdated).AuthorizationSettings.Select((IFailoverReplicationAuthorizationSetting setting) => new VMReplicationAuthorizationEntry(setting)).ToArray();
		}
	}

	public int CertificateAuthenticationPort
	{
		get
		{
			if (m_ClusterBroker != null)
			{
				return m_ClusterBroker.GetData(UpdatePolicy.EnsureUpdated).HttpsPort;
			}
			return m_ServiceSetting.GetData(UpdatePolicy.EnsureUpdated).HttpsPort;
		}
		internal set
		{
			if (m_ClusterBroker != null)
			{
				m_ClusterBroker.GetData(UpdatePolicy.None).HttpsPort = value;
			}
			else
			{
				m_ServiceSetting.GetData(UpdatePolicy.None).HttpsPort = value;
			}
		}
	}

	public Hashtable CertificateAuthenticationPortMapping
	{
		get
		{
			if (m_ClusterBroker != null)
			{
				IMSClusterReplicaBrokerResource data = m_ClusterBroker.GetData(UpdatePolicy.EnsureUpdated);
				if (m_RequiresListenerPortMappingParse)
				{
					ParseClusterPortMapping(data.ListenerPortMapping, data.GetCapName(), data.HttpPort, data.HttpsPort, out m_KerberosListenerPortMapping, out m_CertificateListenerPortMapping);
					m_RequiresListenerPortMappingParse = false;
				}
				return m_CertificateListenerPortMapping;
			}
			return null;
		}
	}

	public string CertificateThumbprint
	{
		get
		{
			if (m_ClusterBroker != null)
			{
				return m_ClusterBroker.GetData(UpdatePolicy.EnsureUpdated).CertificateThumbprint;
			}
			return m_ServiceSetting.GetData(UpdatePolicy.EnsureUpdated).CertificateThumbprint;
		}
		internal set
		{
			if (m_ClusterBroker != null)
			{
				m_ClusterBroker.GetData(UpdatePolicy.None).CertificateThumbprint = value ?? string.Empty;
			}
			else
			{
				m_ServiceSetting.GetData(UpdatePolicy.None).CertificateThumbprint = value ?? string.Empty;
			}
		}
	}

	public string DefaultStorageLocation
	{
		get
		{
			if (!TryFindAuthorizationEntry("*", out var foundEntry))
			{
				return string.Empty;
			}
			return foundEntry.ReplicaStorageLocation;
		}
	}

	public int KerberosAuthenticationPort
	{
		get
		{
			if (m_ClusterBroker != null)
			{
				return m_ClusterBroker.GetData(UpdatePolicy.EnsureUpdated).HttpPort;
			}
			return m_ServiceSetting.GetData(UpdatePolicy.EnsureUpdated).HttpPort;
		}
		internal set
		{
			if (m_ClusterBroker != null)
			{
				m_ClusterBroker.GetData(UpdatePolicy.None).HttpPort = value;
			}
			else
			{
				m_ServiceSetting.GetData(UpdatePolicy.None).HttpPort = value;
			}
		}
	}

	public Hashtable KerberosAuthenticationPortMapping
	{
		get
		{
			if (m_ClusterBroker != null)
			{
				IMSClusterReplicaBrokerResource data = m_ClusterBroker.GetData(UpdatePolicy.EnsureUpdated);
				if (m_RequiresListenerPortMappingParse)
				{
					ParseClusterPortMapping(data.ListenerPortMapping, data.GetCapName(), data.HttpPort, data.HttpsPort, out m_KerberosListenerPortMapping, out m_CertificateListenerPortMapping);
					m_RequiresListenerPortMappingParse = false;
				}
				return m_KerberosListenerPortMapping;
			}
			return null;
		}
	}

	public TimeSpan MonitoringInterval
	{
		get
		{
			uint num = ((m_ClusterBroker == null) ? m_ServiceSetting.GetData(UpdatePolicy.EnsureUpdated).MonitoringInterval : m_ClusterBroker.GetData(UpdatePolicy.EnsureUpdated).MonitoringInterval);
			return TimeSpan.FromSeconds(num);
		}
		internal set
		{
			uint monitoringInterval = Convert.ToUInt32(value.TotalSeconds);
			if (m_ClusterBroker != null)
			{
				m_ClusterBroker.GetData(UpdatePolicy.None).MonitoringInterval = monitoringInterval;
			}
			else
			{
				m_ServiceSetting.GetData(UpdatePolicy.None).MonitoringInterval = monitoringInterval;
			}
		}
	}

	public TimeSpan MonitoringStartTime
	{
		get
		{
			if (m_ClusterBroker != null)
			{
				return TimeSpan.FromSeconds(m_ClusterBroker.GetData(UpdatePolicy.EnsureUpdated).MonitoringStartTime);
			}
			return m_ServiceSetting.GetData(UpdatePolicy.EnsureUpdated).MonitoringStartTime;
		}
		internal set
		{
			if (m_ClusterBroker != null)
			{
				m_ClusterBroker.GetData(UpdatePolicy.None).MonitoringStartTime = Convert.ToUInt32(value.TotalSeconds);
			}
			else
			{
				m_ServiceSetting.GetData(UpdatePolicy.None).MonitoringStartTime = value;
			}
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "By spec.")]
	public VMReplicationServerOperationalStatus[] OperationalStatus => m_Service.GetData(UpdatePolicy.EnsureUpdated).OperationalStatus.Select((ushort status) => (VMReplicationServerOperationalStatus)status).ToArray();

	[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
	public bool ReplicationAllowedFromAnyServer
	{
		get
		{
			VMReplicationAuthorizationEntry foundEntry;
			return TryFindAuthorizationEntry("*", out foundEntry);
		}
	}

	public bool ReplicationEnabled
	{
		get
		{
			if (m_ClusterBroker != null)
			{
				return m_ClusterBroker.GetData(UpdatePolicy.EnsureUpdated).RecoveryServerEnabled;
			}
			return m_ServiceSetting.GetData(UpdatePolicy.EnsureUpdated).RecoveryServerEnabled;
		}
		internal set
		{
			if (m_ClusterBroker != null)
			{
				m_ClusterBroker.GetData(UpdatePolicy.None).RecoveryServerEnabled = value;
			}
			else
			{
				m_ServiceSetting.GetData(UpdatePolicy.None).RecoveryServerEnabled = value;
			}
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "By spec.")]
	public string[] StatusDescriptions => m_Service.GetData(UpdatePolicy.EnsureUpdated).StatusDescriptions;

	private VMReplicationServer(IReplicationService service, [Optional] IMSClusterReplicaBrokerResource broker)
		: base(service.Setting)
	{
		m_ServiceSetting = InitializePrimaryDataUpdater(service.Setting);
		m_Service = new DataUpdater<IReplicationService>(service);
		if (broker != null)
		{
			m_ClusterBroker = new DataUpdater<IMSClusterReplicaBrokerResource>(broker);
			m_ClusterBroker.GetData(UpdatePolicy.None).CacheUpdated += ClusterReplicationBroker_CacheUpdated;
		}
	}

	void IUpdatable.Put(IOperationWatcher operationWatcher)
	{
		if (m_ClusterBroker != null)
		{
			IMSClusterReplicaBrokerResource data = m_ClusterBroker.GetData(UpdatePolicy.None);
			operationWatcher.PerformPut(data, TaskDescriptions.SetReplicationService, null);
		}
		else
		{
			IFailoverReplicationServiceSetting data2 = m_ServiceSetting.GetData(UpdatePolicy.None);
			operationWatcher.PerformPut(data2, TaskDescriptions.SetReplicationService, null);
		}
	}

	internal static VMReplicationServer GetReplicationServer(Server server)
	{
		IReplicationService failoverReplicationService = ObjectLocator.GetFailoverReplicationService(server);
		TryGetClusterAndBroker(server, out var _, out var broker);
		return new VMReplicationServer(failoverReplicationService, broker);
	}

	internal static bool TryGetClusterAndBroker(Server server, out IMSClusterCluster cluster, out IMSClusterReplicaBrokerResource broker)
	{
		if (ObjectLocator.TryGetClusterObject(server, out cluster))
		{
			broker = cluster.GetReplicaBroker();
			return true;
		}
		broker = null;
		return false;
	}

	internal void AddAuthorizationEntry(string allowedPrimaryServer, string replicaStorageLocation, string trustGroup, IOperationWatcher watcher)
	{
		ReplicationAuthorizationEntry entry = new ReplicationAuthorizationEntry(base.Server, allowedPrimaryServer, replicaStorageLocation, trustGroup);
		if (m_ClusterBroker != null)
		{
			if (!FileUtilities.IsValidClusterStorage(replicaStorageLocation, base.Server))
			{
				throw ExceptionHelper.CreateOperationFailedException(ErrorMessages.VMReplication_ClusterStorageError);
			}
			try
			{
				AddAuthorizationEntryLocal(entry, watcher);
			}
			catch (VirtualizationOperationFailedException ex)
			{
				if (ex.ErrorCode != 32770)
				{
					throw;
				}
			}
			List<VMReplicationAuthorizationEntry> entries = AuthorizationEntries.Concat(new VMReplicationAuthorizationEntry[1]
			{
				new VMReplicationAuthorizationEntry(base.Server, allowedPrimaryServer, replicaStorageLocation, trustGroup)
			}).ToList();
			CommitBrokerAuthorizationEntries(entries, watcher);
		}
		else
		{
			AddAuthorizationEntryLocal(entry, watcher);
		}
	}

	internal void ChangeReplicationModeToPrimary(VMReplication replication, IOperationWatcher watcher)
	{
		IVMComputerSystem vm = replication.VirtualMachine.GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.None);
		IVMReplicationRelationship relationship = replication.GetReplicationRelationship(UpdatePolicy.None);
		IReplicationService service = m_Service.GetData(UpdatePolicy.None);
		watcher.PerformOperation(() => service.BeginChangeReplicationModeToPrimary(vm, relationship), service.EndChangeReplicationModeToPrimary, TaskDescriptions.Task_ChangeReplicationModeToPrimary, null);
		vm.InvalidatePropertyCache();
		relationship.InvalidatePropertyCache();
	}

	internal void CommitFailover(VirtualMachine vm, IOperationWatcher watcher)
	{
		IVMComputerSystem vmObject = vm.GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.None);
		IReplicationService service = m_Service.GetData(UpdatePolicy.None);
		watcher.PerformOperation(() => service.BeginCommitFailover(vmObject), service.EndCommitFailover, TaskDescriptions.Task_CommitFailover, null);
		vmObject.InvalidatePropertyCache();
		vmObject.InvalidateAssociationCache();
	}

	internal void CreateReplicationRelationship(VMReplication relationship, IOperationWatcher operationWatcher)
	{
		IReplicationService service = m_Service.GetData(UpdatePolicy.None);
		IVMComputerSystem vmObject = relationship.VirtualMachine.GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.None);
		IVMReplicationSettingData replicationSettingData = relationship.GetReplicationSetting(UpdatePolicy.None);
		operationWatcher.PerformOperation(() => service.BeginCreateReplicationRelationship(vmObject, replicationSettingData), service.EndCreateReplicationRelationship, TaskDescriptions.Task_CreateReplicationRelationship, null);
		vmObject.InvalidatePropertyCache();
		vmObject.InvalidateAssociationCache();
		replicationSettingData.InvalidatePropertyCache();
		relationship.GetReplicationRelationship(UpdatePolicy.None).InvalidatePropertyCache();
	}

	internal void ImportInitialReplica(VirtualMachine vm, string initialReplicationImportLocation, IOperationWatcher watcher)
	{
		IVMComputerSystem vmObject = vm.GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.None);
		IReplicationService service = m_Service.GetData(UpdatePolicy.None);
		watcher.PerformOperation(() => service.BeginImportInitialReplica(vmObject, initialReplicationImportLocation), service.EndImportInitialReplica, TaskDescriptions.Task_ImportReplication, null);
		vmObject.InvalidatePropertyCache();
		vmObject.InvalidateAssociationCache();
	}

	internal void InitiateFailover(VirtualMachine vm, IOperationWatcher watcher, [Optional] VMSnapshot snapshot)
	{
		IVMComputerSystem vmObject = vm.GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.None);
		IVMComputerSystemSetting snapshotObject = snapshot?.GetSettings(UpdatePolicy.None);
		IReplicationService service = m_Service.GetData(UpdatePolicy.None);
		watcher.PerformOperation(() => service.BeginInitiateFailover(vmObject, snapshotObject), service.EndInitiateFailover, TaskDescriptions.Task_InitiateFailover, null);
		vmObject.InvalidatePropertyCache();
		vmObject.InvalidateAssociationCache();
	}

	internal void RemoveAllAuthorizationEntries(IOperationWatcher watcher)
	{
		if (m_ClusterBroker != null)
		{
			CommitBrokerAuthorizationEntry(null, watcher);
			return;
		}
		VMReplicationAuthorizationEntry[] authorizationEntries = AuthorizationEntries;
		foreach (VMReplicationAuthorizationEntry vMReplicationAuthorizationEntry in authorizationEntries)
		{
			RemoveAuthorizationEntry(vMReplicationAuthorizationEntry.AllowedPrimaryServer, watcher);
		}
	}

	internal void RemoveAuthorizationEntry(string allowedPrimaryServer, IOperationWatcher watcher)
	{
		if (m_ClusterBroker != null)
		{
			VMReplicationAuthorizationEntry[] authorizationEntries = AuthorizationEntries;
			List<VMReplicationAuthorizationEntry> list = authorizationEntries.Where((VMReplicationAuthorizationEntry entry) => !string.Equals(entry.AllowedPrimaryServer, allowedPrimaryServer)).ToList();
			if (list.Count == authorizationEntries.Length)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(ErrorMessages.VMReplication_AuthorizationEntryNotFound);
			}
			CommitBrokerAuthorizationEntries(list, watcher);
		}
		else
		{
			IReplicationService data = m_Service.GetData(UpdatePolicy.None);
			data.RemoveAuthorizationEntry(allowedPrimaryServer);
			data.InvalidateAssociationCache();
		}
	}

	internal void CommitBrokerAuthorizationEntries(ICollection<VMReplicationAuthorizationEntry> entries, IOperationWatcher watcher)
	{
		string authorizations = CreateClusterAuthorizationEntries(entries);
		CommitBrokerAuthorizationEntry(authorizations, watcher);
	}

	internal void RemoveReplicationRelationshipEx(VMReplication relationshipToRemove, IOperationWatcher operationWatcher)
	{
		IReplicationService service = m_Service.GetData(UpdatePolicy.None);
		IVMComputerSystem virtualMachine = relationshipToRemove.VirtualMachine.GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.None);
		IVMReplicationRelationship relationship = relationshipToRemove.GetReplicationRelationship(UpdatePolicy.None);
		operationWatcher.PerformOperation(() => service.BeginRemoveReplicationRelationshipEx(virtualMachine, relationship), service.EndRemoveReplicationRelationshipEx, TaskDescriptions.Task_RemoveReplication, null);
		virtualMachine.InvalidatePropertyCache();
		virtualMachine.InvalidateAssociationCache();
	}

	internal void ResetReplicationStatisticsEx(VMReplication replication, IOperationWatcher operationWatcher)
	{
		IVMReplicationRelationship relationship = replication.GetReplicationRelationship(UpdatePolicy.None);
		IVMComputerSystem vm = replication.VirtualMachine.GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.None);
		IReplicationService service = m_Service.GetData(UpdatePolicy.None);
		operationWatcher.PerformOperation(() => service.BeginResetReplicationStatisticsEx(vm, relationship), service.EndResetReplicationStatisticsEx, TaskDescriptions.Task_ResetReplicationStatistics, null);
		vm.InvalidatePropertyCache();
		relationship.InvalidatePropertyCache();
	}

	internal void ResynchronizeAsync(VirtualMachine vm, DateTime scheduledResyncTime, IOperationWatcher operationWatcher, bool monitorProgress)
	{
		IVMTask replicationResyncTask = vm.GetReplicationResyncTask();
		if (replicationResyncTask != null && replicationResyncTask.Status == VMTaskStatus.Running)
		{
			replicationResyncTask.Cancel();
		}
		IVMComputerSystem vmObject = vm.GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.EnsureUpdated);
		IReplicationService service = m_Service.GetData(UpdatePolicy.None);
		Func<IVMTask> func = () => service.BeginResynchronizeReplication(vmObject, scheduledResyncTime);
		if (monitorProgress)
		{
			operationWatcher.PerformOperation(func, service.EndResynchronizeReplication, TaskDescriptions.Task_Resynchronize, null);
		}
		else
		{
			using (func())
			{
			}
		}
		vmObject.InvalidatePropertyCache();
		vmObject.InvalidateAssociationCache();
	}

	internal void ReverseReplicationRelationship(VMReplication replication, IOperationWatcher watcher)
	{
		IReplicationService service = m_Service.GetData(UpdatePolicy.None);
		IVMComputerSystem vm = replication.VirtualMachine.GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.None);
		IVMReplicationSettingData replicationSettings = replication.GetReplicationSetting(UpdatePolicy.None);
		watcher.PerformOperation(() => service.BeginReverseReplicationRelationship(vm, replicationSettings), service.EndReverseReplicationRelationship, TaskDescriptions.Task_ReverseReplicationRelationship, null);
		vm.InvalidatePropertyCache();
		replicationSettings.InvalidatePropertyCache();
	}

	internal void RevertFailover(VirtualMachine vm, IOperationWatcher watcher)
	{
		IVMComputerSystem vmObject = vm.GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.None);
		IReplicationService service = m_Service.GetData(UpdatePolicy.None);
		watcher.PerformOperation(() => service.BeginRevertFailover(vmObject), service.EndRevertFailover, TaskDescriptions.Task_RevertVMFailover, null);
		vmObject.InvalidatePropertyCache();
		vmObject.InvalidateAssociationCache();
	}

	internal void StartReplication(VirtualMachine vm, InitialReplicationType initialReplicationType, string initialReplicationExportLocation, DateTime startTime, IOperationWatcher operationWatcher)
	{
		IVMComputerSystem vmObject = vm.GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.EnsureUpdated);
		IReplicationService service = m_Service.GetData(UpdatePolicy.None);
		operationWatcher.PerformOperation(() => service.BeginStartReplication(vmObject, (global::Microsoft.Virtualization.Client.Management.InitialReplicationType)initialReplicationType, initialReplicationExportLocation, startTime), service.EndStartReplication, TaskDescriptions.Task_StartReplication, null);
		vmObject.InvalidatePropertyCache();
		vmObject.InvalidateAssociationCache();
	}

	internal void SetAuthorizationEntry(VirtualMachine vm, string allowedPrimaryServer, string replicaStorageLocation, string trustGroup, IOperationWatcher watcher)
	{
		ReplicationAuthorizationEntry instance = new ReplicationAuthorizationEntry(base.Server, allowedPrimaryServer, replicaStorageLocation, trustGroup);
		IVMComputerSystem virtualMachineObject = vm.GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.None);
		IReplicationService service = m_Service.GetData(UpdatePolicy.None);
		watcher.PerformOperation(() => service.BeginSetAuthorizationEntry(virtualMachineObject, instance), service.EndSetAuthorizationEntry, TaskDescriptions.Task_SetAuthorizationEntry, null);
	}

	[SuppressMessage("Microsoft.Usage", "#pw26501", Scope = "member", Target = "kerberosListenerPortMapping")]
	[SuppressMessage("Microsoft.Usage", "#pw26501", Scope = "member", Target = "certificateListenerPortMapping")]
	internal void SetServerListenerPortMappings(Hashtable kerberosListenerPortMapping, Hashtable certificateListenerPortMapping, Func<string, bool> cmdletShouldContinueFunc)
	{
		IMSClusterReplicaBrokerResource iMSClusterReplicaBrokerResource = null;
		if (m_ClusterBroker != null)
		{
			iMSClusterReplicaBrokerResource = m_ClusterBroker.GetData(UpdatePolicy.None);
		}
		string text = ((iMSClusterReplicaBrokerResource == null) ? string.Empty : iMSClusterReplicaBrokerResource.GetCapName());
		if (string.IsNullOrEmpty(text))
		{
			throw ExceptionHelper.CreateOperationFailedException(ErrorMessages.VMReplication_BrokerResourceNotFound);
		}
		string domain = ObjectLocator.GetWin32ComputerSystem(base.Server).Domain;
		IMSClusterNode[] clusterNodes = ObjectLocator.GetClusterObject(base.Server).GetClusterNodes().ToArray();
		string duplicatePorts = string.Empty;
		if (!kerberosListenerPortMapping.IsNullOrEmpty())
		{
			ValidateListenerPortMap(kerberosListenerPortMapping, "KerberosAuthenticationPortMapping", text, domain, clusterNodes);
			ValidateUniqueListenerPorts(kerberosListenerPortMapping, ref duplicatePorts);
		}
		if (!certificateListenerPortMapping.IsNullOrEmpty())
		{
			ValidateListenerPortMap(certificateListenerPortMapping, "CertificateAuthenticationPortMapping", text, domain, clusterNodes);
			ValidateUniqueListenerPorts(certificateListenerPortMapping, ref duplicatePorts);
		}
		if (kerberosListenerPortMapping != null && certificateListenerPortMapping != null)
		{
			foreach (DictionaryEntry item in kerberosListenerPortMapping)
			{
				object obj = certificateListenerPortMapping[item.Key];
				if (obj != null && item.Value.Equals(obj))
				{
					throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.VMReplication_KerberosAndCertificatePortsShouldBeDifferent, item.Key));
				}
			}
		}
		if (duplicatePorts.Length <= 0 || cmdletShouldContinueFunc(string.Format(CultureInfo.CurrentCulture, ErrorMessages.VMReplication_UniquePortsNotSpecified, duplicatePorts)))
		{
			if (!kerberosListenerPortMapping.IsNullOrEmpty())
			{
				KerberosAuthenticationPort = int.Parse(kerberosListenerPortMapping[text].ToString(), CultureInfo.CurrentCulture);
			}
			if (!certificateListenerPortMapping.IsNullOrEmpty())
			{
				CertificateAuthenticationPort = int.Parse(certificateListenerPortMapping[text].ToString(), CultureInfo.CurrentCulture);
			}
			iMSClusterReplicaBrokerResource.ListenerPortMapping = CreateClusterPortMapping(text, kerberosListenerPortMapping, certificateListenerPortMapping);
		}
	}

	internal void TestReplicationConnection(string recoveryConnectionPoint, ushort recoveryServerPortNumber, ReplicationAuthenticationType authenticationType, string certificateThumbPrint, bool bypassProxyServer, IOperationWatcher operationWatcher)
	{
		IReplicationService service = m_Service.GetData(UpdatePolicy.None);
		operationWatcher.PerformOperation(() => service.BeginTestReplicationConnection(recoveryConnectionPoint, recoveryServerPortNumber, (global::Microsoft.Virtualization.Client.Management.RecoveryAuthenticationType)authenticationType, certificateThumbPrint, bypassProxyServer), service.EndTestReplicationConnection, TaskDescriptions.Task_TestReplicationConnection, null);
	}

	internal VirtualMachine TestReplicaSystem(VirtualMachine vm, IOperationWatcher watcher, [Optional] VMSnapshot snapshot)
	{
		IVMComputerSystem vmObject = vm.GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.None);
		IVMComputerSystemSetting snapshotObject = snapshot?.GetSettings(UpdatePolicy.None);
		IReplicationService service = m_Service.GetData(UpdatePolicy.None);
		IVMComputerSystem computerSystem = watcher.PerformOperationWithReturn(() => service.BeginCreateTestVirtualSystem(vmObject, snapshotObject), (IVMTask task) => service.EndCreateTestVirtualSystem(task, vmObject.InstanceId), TaskDescriptions.Task_TestReplicaSystem, null);
		vmObject.InvalidateAssociationCache();
		return new VirtualMachine(computerSystem);
	}

	internal bool TryFindAuthorizationEntry(string allowedPrimaryServer, out VMReplicationAuthorizationEntry foundEntry)
	{
		foundEntry = AuthorizationEntries.FirstOrDefault((VMReplicationAuthorizationEntry entry) => string.Equals(entry.AllowedPrimaryServer, allowedPrimaryServer, StringComparison.OrdinalIgnoreCase));
		return foundEntry != null;
	}

	private static string CreateClusterPortMapping(string brokerCapFqdn, Hashtable kerberosPortMapping, Hashtable certificatePortMapping)
	{
		StringBuilder stringBuilder = new StringBuilder(1024);
		bool flag = kerberosPortMapping != null && kerberosPortMapping.Count > 0;
		if (certificatePortMapping != null && certificatePortMapping.Count > 0)
		{
			certificatePortMapping.Remove(brokerCapFqdn);
			foreach (DictionaryEntry item in certificatePortMapping)
			{
				string text = string.Empty;
				if (flag)
				{
					text = kerberosPortMapping[item.Key].ToString();
					kerberosPortMapping.Remove(item.Key);
				}
				stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0}{3}{1}{3}{2}{3}", item.Key, text, item.Value, '\u00a0'));
			}
		}
		if (kerberosPortMapping != null && kerberosPortMapping.Count > 0)
		{
			kerberosPortMapping.Remove(brokerCapFqdn);
			foreach (DictionaryEntry item2 in kerberosPortMapping)
			{
				stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0}{3}{1}{3}{2}{3}", item2.Key, item2.Value, string.Empty, '\u00a0'));
			}
		}
		return stringBuilder.ToString();
	}

	private static void ParseClusterPortMapping(string portMapping, string capFqdn, int kerberosPort, int certificatePort, out Hashtable kerberosPortMapping, out Hashtable certificatePortMapping)
	{
		kerberosPortMapping = null;
		certificatePortMapping = null;
		Hashtable hashtable = new Hashtable();
		Hashtable hashtable2 = new Hashtable();
		string[] array = portMapping.Split(new char[1] { '\u00a0' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i + 2 < array.Length; i += 3)
		{
			if (Uri.CheckHostName(array[i]) != UriHostNameType.Dns)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.InvalidArgument_ReplicaNodeExpectingFqdn, array[i]));
			}
			if (!string.IsNullOrEmpty(array[i + 1]))
			{
				if (!int.TryParse(array[i + 1], out var result) || result < 1 || result > 65535)
				{
					throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.InvalidArgument_ReplicaBrokerInvalidPort, result));
				}
				hashtable.Add(array[i], result);
			}
			if (!string.IsNullOrEmpty(array[i + 2]))
			{
				if (!int.TryParse(array[i + 2], out var result2) || result2 < 1 || result2 > 65535)
				{
					throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.InvalidArgument_ReplicaBrokerInvalidPort, result2));
				}
				hashtable2.Add(array[i], result2);
			}
		}
		if (hashtable.Count > 0)
		{
			hashtable.Add(capFqdn, kerberosPort);
			kerberosPortMapping = hashtable;
		}
		if (hashtable2.Count > 0)
		{
			hashtable2.Add(capFqdn, certificatePort);
			certificatePortMapping = hashtable2;
		}
	}

	private static string CreateClusterAuthorizationEntries(IEnumerable<VMReplicationAuthorizationEntry> entries)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (VMReplicationAuthorizationEntry entry in entries)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}{3}{1}{3}{2}{3}", entry.AllowedPrimaryServer, entry.ReplicaStorageLocation, entry.TrustGroup, '\u00a0');
		}
		return stringBuilder.ToString();
	}

	private static void ParseClusterAuthorizationEntries(Server server, string authorizationEntries, out VMReplicationAuthorizationEntry[] entries)
	{
		List<VMReplicationAuthorizationEntry> list = new List<VMReplicationAuthorizationEntry>();
		string[] array = authorizationEntries.Split(new char[1] { '\u00a0' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i + 2 < array.Length; i += 3)
		{
			string allowedPrimaryServer = array[i];
			string replicaStorageLocation = array[i + 1];
			string trustGroup = array[i + 2];
			list.Add(new VMReplicationAuthorizationEntry(server, allowedPrimaryServer, replicaStorageLocation, trustGroup));
		}
		entries = list.ToArray();
	}

	private static void ValidateListenerPortMap(Hashtable portMap, string mapName, string replicaBrokerCap, string domainName, IEnumerable<IMSClusterNode> clusterNodes)
	{
		if (!portMap.ContainsKey(replicaBrokerCap))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.VMReplication_BrokerResourcePortMappingNotSpecified, replicaBrokerCap, mapName));
		}
		foreach (DictionaryEntry item in portMap)
		{
			string text = item.Key.ToString();
			if (Uri.CheckHostName(text) != UriHostNameType.Dns)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.VMReplication_ExpectingFqdn, text, mapName));
			}
			string text2 = item.Value.ToString();
			if (string.IsNullOrEmpty(text2.Trim()))
			{
				throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.VMReplication_PortValueNotSpecified, text, mapName));
			}
			if (!int.TryParse(text2, out var result) || result < 1 || result > 65535)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.VMReplication_InvalidPortValue, text2, mapName));
			}
			if (!text.EndsWith(domainName, StringComparison.OrdinalIgnoreCase))
			{
				throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.VMReplication_DomainShouldBeSpecified, text, mapName, domainName));
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (IMSClusterNode clusterNode in clusterNodes)
		{
			if (!portMap.ContainsKey(clusterNode.Name))
			{
				stringBuilder.AppendLine(clusterNode.Name);
			}
		}
		if (stringBuilder.Length > 0)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.VMReplication_NodePortMappingNotSpecified, stringBuilder, mapName));
		}
	}

	private static void ValidateUniqueListenerPorts(Hashtable portMap, ref string duplicatePorts)
	{
		HashSet<uint> hashSet = new HashSet<uint>();
		HashSet<uint> hashSet2 = new HashSet<uint>();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (DictionaryEntry item2 in portMap)
		{
			uint item = uint.Parse(item2.Value.ToString(), CultureInfo.CurrentCulture);
			if (!hashSet.Add(item) && hashSet2.Add(item))
			{
				stringBuilder.AppendLine(item2.Value.ToString());
			}
		}
		if (stringBuilder.Length > 0)
		{
			duplicatePorts += stringBuilder.ToString();
		}
	}

	private void AddAuthorizationEntryLocal(ReplicationAuthorizationEntry entry, IOperationWatcher watcher)
	{
		IReplicationService replicationService = m_Service.GetData(UpdatePolicy.None);
		watcher.PerformOperation(() => replicationService.BeginAddAuthorizationEntry(entry), replicationService.EndAddAuthorizationEntry, TaskDescriptions.Task_AddReplicationAuthorizationEntry, null);
		replicationService.InvalidateAssociationCache();
	}

	private void ClusterReplicationBroker_CacheUpdated(object sender, EventArgs e)
	{
		m_RequiresListenerPortMappingParse = true;
		m_RequiresAuthorizationEntryParse = true;
	}

	private void CommitBrokerAuthorizationEntry(string authorizations, IOperationWatcher watcher)
	{
		IMSClusterReplicaBrokerResource data = m_ClusterBroker.GetData(UpdatePolicy.None);
		data.Authorization = authorizations;
		((IUpdatable)this).Put(watcher);
		data.InvalidatePropertyCache();
	}
}
