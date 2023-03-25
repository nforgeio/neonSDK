using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMGroup : VirtualizationObject, IRemovable, IUpdatable
{
	private readonly DataUpdater<IHyperVCollection> m_HyperVCollection;

	[VirtualizationObjectIdentifier(IdentifierFlags.FriendlyName)]
	public string Name
	{
		get
		{
			return m_HyperVCollection.GetData(UpdatePolicy.EnsureUpdated).Name;
		}
		internal set
		{
			m_HyperVCollection.GetData(UpdatePolicy.None).Name = value;
		}
	}

	[VirtualizationObjectIdentifier(IdentifierFlags.UniqueIdentifier)]
	public Guid InstanceId => m_HyperVCollection.GetData(UpdatePolicy.EnsureUpdated).InstanceId;

	public GroupType GroupType => (GroupType)m_HyperVCollection.GetData(UpdatePolicy.EnsureUpdated).Type;

	public IReadOnlyList<VirtualMachine> VMMembers
	{
		get
		{
			IReadOnlyList<VirtualMachine> result = null;
			if (m_HyperVCollection.GetData(UpdatePolicy.EnsureAssociatorsUpdated) is IVMCollection iVMCollection)
			{
				result = iVMCollection.CollectedVirtualMachines.Select((IVMComputerSystem vm) => new VirtualMachine(vm)).ToList().AsReadOnly();
			}
			return result;
		}
	}

	public IReadOnlyList<VMGroup> VMGroupMembers
	{
		get
		{
			IReadOnlyList<VMGroup> result = null;
			if (m_HyperVCollection.GetData(UpdatePolicy.EnsureAssociatorsUpdated) is IManagementCollection managementCollection)
			{
				result = managementCollection.CollectedCollections.Select((IHyperVCollection group) => new VMGroup(group)).ToList().AsReadOnly();
			}
			return result;
		}
	}

	internal VMGroup(IHyperVCollection collection)
		: base(collection)
	{
		m_HyperVCollection = InitializePrimaryDataUpdater(collection);
	}

	internal static VMGroup Create(Server server, string name, GroupType type, Guid instanceId, IOperationWatcher operationWatcher)
	{
		ICollectionManagementService service = ObjectLocator.GetCollectionManagementService(server);
		return new VMGroup(operationWatcher.PerformOperationWithReturn(() => service.BeginCreateCollection(name, instanceId, (CollectionType)type), service.EndCreateCollection, TaskDescriptions.NewVMGroup, null));
	}

	internal static IList<VMGroup> GetVMGroupsByName(IEnumerable<Server> servers, IList<string> groupName, IOperationWatcher operationWatcher)
	{
		IEnumerable<IHyperVCollection> enumerable = servers.SelectManyWithLogging((Server server) => ObjectLocator.GetHyperVCollections(server), operationWatcher);
		if (!groupName.IsNullOrEmpty())
		{
			WildcardPatternMatcher matcher = new WildcardPatternMatcher(groupName);
			enumerable = enumerable.Where((IHyperVCollection group) => matcher.MatchesAny(group.Name));
		}
		return enumerable.SelectWithLogging((IHyperVCollection collection) => new VMGroup(collection), operationWatcher).ToList();
	}

	internal static IList<VMGroup> GetVMGroupsById(IEnumerable<Server> servers, Guid id, IOperationWatcher operationWatcher)
	{
		return (from @group in servers.SelectManyWithLogging((Server server) => ObjectLocator.GetHyperVCollections(server), operationWatcher)
			where @group.InstanceId == id
			select @group).SelectWithLogging((IHyperVCollection collection) => new VMGroup(collection), operationWatcher).ToList();
	}

	internal static void RemoveGroupMemberById(VirtualMachine member, Guid collectionId, IOperationWatcher watcher)
	{
		ICollectionManagementService service = ObjectLocator.GetCollectionManagementService(member.Server);
		VMGroup vMGroup = GetVMGroupsById(new Server[1] { member.Server }, collectionId, watcher).SingleOrDefault();
		if (vMGroup != null && vMGroup.GroupType == GroupType.ManagementCollectionType)
		{
			throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.VMGroup_VMGroupType_IncorrectType, null);
		}
		watcher.PerformOperation(() => service.BeginRemoveMemberById(member.GetComputerSystem(UpdatePolicy.None), collectionId.ToString()), service.EndRemoveMemberById, TaskDescriptions.RemoveVMGroupMember, null);
	}

	internal static void RemoveGroupMemberById(VMGroup member, Guid collectionId, IOperationWatcher watcher)
	{
		ICollectionManagementService service = ObjectLocator.GetCollectionManagementService(member.Server);
		VMGroup vMGroup = GetVMGroupsById(new Server[1] { member.Server }, collectionId, watcher).SingleOrDefault();
		if (vMGroup != null && vMGroup.GroupType == GroupType.VMCollectionType)
		{
			throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.VMGroup_VMType_IncorrectType, null);
		}
		watcher.PerformOperation(() => service.BeginRemoveMemberById(member.GetHyperVCollection(UpdatePolicy.None), collectionId.ToString()), service.EndRemoveMemberById, TaskDescriptions.RemoveVMGroupMember, null);
	}

	void IRemovable.Remove(IOperationWatcher operationWatcher)
	{
		operationWatcher.PerformDelete(m_HyperVCollection.GetData(UpdatePolicy.EnsureUpdated), TaskDescriptions.RemoveVMGroup, this);
	}

	void IUpdatable.Put(IOperationWatcher operationWatcher)
	{
		operationWatcher.PerformPut(m_HyperVCollection.GetData(UpdatePolicy.None), TaskDescriptions.SetVMGroupName, this);
	}

	internal void AddVM(VirtualMachine vm, IOperationWatcher operationWatcher)
	{
		IVMCollection collection = (IVMCollection)m_HyperVCollection.GetData(UpdatePolicy.None);
		operationWatcher.PerformOperation(() => collection.BeginAddVirtualMachine(vm.GetComputerSystem(UpdatePolicy.None)), collection.EndAddVirtualMachine, TaskDescriptions.AddVMGroupMember, this);
		collection.InvalidateAssociationCache();
	}

	internal void AddGroup(VMGroup group, IOperationWatcher operationWatcher)
	{
		IManagementCollection collection = (IManagementCollection)m_HyperVCollection.GetData(UpdatePolicy.None);
		operationWatcher.PerformOperation(() => collection.BeginAddCollection(group.GetHyperVCollection(UpdatePolicy.None)), collection.EndAddCollection, TaskDescriptions.AddVMGroupMember, this);
		collection.InvalidateAssociationCache();
	}

	internal IHyperVCollection GetHyperVCollection(UpdatePolicy policy)
	{
		return m_HyperVCollection.GetData(policy);
	}
}
