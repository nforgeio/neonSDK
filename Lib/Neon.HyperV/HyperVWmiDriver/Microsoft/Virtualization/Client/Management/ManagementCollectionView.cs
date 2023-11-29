#define TRACE
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class ManagementCollectionView : HyperVCollectionView, IManagementCollection, IHyperVCollection, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
    public IEnumerable<IHyperVCollection> CollectedCollections => GetRelatedObjects<IHyperVCollection>(base.Associations.CollectedCollections);

    public ManagementCollectionView()
        : base(CollectionType.ManagementCollectionType)
    {
    }

    public IVMTask BeginAddCollection(IHyperVCollection collection)
    {
        IProxy collectionManagementServiceProxy = GetCollectionManagementServiceProxy();
        object[] array = new object[3] { collection, this, null };
        VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.CurrentCulture, "Start adding a collection to collection '{0}' ({1})", base.InstanceId.ToString("D"), base.Name), collection.ManagementPath.ToString());
        uint result = collectionManagementServiceProxy.InvokeMethod("AddMember", array);
        return BeginMethodTaskReturn(result, null, array[2]);
    }

    public void EndAddCollection(IVMTask task)
    {
        EndMethod(task, VirtualizationOperation.AddMemberToCollection);
        VMTrace.TraceUserActionCompleted(string.Format(CultureInfo.CurrentCulture, "Complete adding a collection to '{0}' ({1})", base.InstanceId.ToString("D"), base.Name));
    }
}
