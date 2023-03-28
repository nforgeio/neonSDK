#define TRACE
using System;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class CollectionManagementServiceView : View, ICollectionManagementService, IVirtualizationManagementObject
{
	internal static class WmiMemberNames
	{
		public const string DefineCollection = "DefineCollection";

		public const string DestroyCollection = "DestroyCollection";

		public const string RenameCollection = "RenameCollection";

		public const string AddMember = "AddMember";

		public const string RemoveMember = "RemoveMember";

		public const string RemoveMemberById = "RemoveMemberById";
	}

	public IVMTask BeginCreateCollection(string name, Guid instanceId, CollectionType type)
	{
		string text = null;
		if (instanceId != Guid.Empty)
		{
			text = instanceId.ToString("D");
		}
		IProxy collectionManagementServiceProxy = GetCollectionManagementServiceProxy();
		object[] array = new object[5]
		{
			name,
			text,
			(ushort)type,
			null,
			null
		};
		VMTrace.TraceUserActionInitiated("Start creating collection", name, text, type.ToString());
		uint result = collectionManagementServiceProxy.InvokeMethod("DefineCollection", array);
		return BeginMethodTaskReturn(result, array[3], array[4]);
	}

	public IHyperVCollection EndCreateCollection(IVMTask task)
	{
		IHyperVCollection hyperVCollection = EndMethodReturn<IHyperVCollection>(task, VirtualizationOperation.CreateCollection);
		VMTrace.TraceUserActionCompleted(string.Format(CultureInfo.CurrentCulture, "Completed creation of collection '{0}' ({1})", hyperVCollection.InstanceId.ToString("D"), hyperVCollection.Name));
		return hyperVCollection;
	}

	public IVMTask BeginRemoveMemberById(IHyperVCollection member, string collectionId)
	{
		return PrivateBeginRemoveMemberById(member, collectionId);
	}

	public IVMTask BeginRemoveMemberById(IVMComputerSystemBase member, string collectionId)
	{
		return PrivateBeginRemoveMemberById(member, collectionId);
	}

	public void EndRemoveMemberById(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.RemoveMemberFromCollectionById);
	}

	private IVMTask PrivateBeginRemoveMemberById(IVirtualizationManagementObject member, string collectionId)
	{
		IProxy collectionManagementServiceProxy = GetCollectionManagementServiceProxy();
		object[] array = new object[3] { member, collectionId, null };
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.CurrentCulture, "Start removing a member from collection '{0}'", collectionId), member.ManagementPath.ToString());
		uint result = collectionManagementServiceProxy.InvokeMethod("RemoveMemberById", array);
		return BeginMethodTaskReturn(result, null, array[2]);
	}
}
