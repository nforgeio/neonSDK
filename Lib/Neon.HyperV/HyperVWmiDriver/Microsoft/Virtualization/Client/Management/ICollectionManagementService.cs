using System;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_CollectionManagementService")]
internal interface ICollectionManagementService : IVirtualizationManagementObject
{
	IVMTask BeginCreateCollection(string name, Guid instanceId, CollectionType type);

	IHyperVCollection EndCreateCollection(IVMTask task);

	IVMTask BeginRemoveMemberById(IHyperVCollection member, string collectionId);

	IVMTask BeginRemoveMemberById(IVMComputerSystemBase member, string collectionId);

	void EndRemoveMemberById(IVMTask task);
}
