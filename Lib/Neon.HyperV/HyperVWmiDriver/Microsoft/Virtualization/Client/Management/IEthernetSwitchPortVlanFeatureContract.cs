using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IEthernetSwitchPortVlanFeatureContract : IEthernetSwitchPortVlanFeature, IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
	public int AccessVlan
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public int NativeVlan
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public VlanOperationMode OperationMode
	{
		get
		{
			return VlanOperationMode.Unknown;
		}
		set
		{
		}
	}

	public PrivateVlanMode PrivateMode
	{
		get
		{
			return PrivateVlanMode.Unknown;
		}
		set
		{
		}
	}

	public int PrimaryVlan
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public int SecondaryVlan
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public abstract string InstanceId { get; }

	public abstract EthernetFeatureType FeatureType { get; }

	public abstract string Name { get; }

	public abstract string ExtensionId { get; }

	public abstract string FeatureId { get; }

	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public IList<int> GetTrunkVlanList()
	{
		return null;
	}

	public void SetTrunkVlanList(IList<int> trunkList)
	{
	}

	public IList<int> GetSecondaryVlanList()
	{
		return null;
	}

	public void SetSecondaryVlanList(IList<int> secondaryVlanList)
	{
	}

	public IVMTask BeginModifySingleFeature(IEthernetSwitchFeatureService service)
	{
		return null;
	}

	public void EndModifySingleFeature(IVMTask modifyTask)
	{
	}

	public abstract void InvalidatePropertyCache();

	public abstract void UpdatePropertyCache();

	public abstract void UpdatePropertyCache(TimeSpan threshold);

	public abstract void RegisterForInstanceModificationEvents(InstanceModificationEventStrategy strategy);

	public abstract void UnregisterForInstanceModificationEvents();

	public abstract void InvalidateAssociationCache();

	public abstract void UpdateAssociationCache();

	public abstract void UpdateAssociationCache(TimeSpan threshold);

	public abstract string GetEmbeddedInstance();

	public abstract void DiscardPendingPropertyChanges();
}
