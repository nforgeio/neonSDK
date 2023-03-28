using System;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal abstract class VMComponentObject : VirtualizationObject
{
	private readonly ComputeResource _parentComputeResource;

	[VirtualizationObjectIdentifier(IdentifierFlags.UniqueIdentifier)]
	public Guid VMId
	{
		get
		{
			if (_parentComputeResource is VMSnapshot vMSnapshot)
			{
				return vMSnapshot.VMId;
			}
			return _parentComputeResource.Id;
		}
	}

	[VirtualizationObjectIdentifier(IdentifierFlags.FriendlyName)]
	public string VMName
	{
		get
		{
			if (_parentComputeResource is VMSnapshot vMSnapshot)
			{
				return vMSnapshot.VMName;
			}
			return _parentComputeResource.Name;
		}
	}

	public Guid VMSnapshotId
	{
		get
		{
			if (_parentComputeResource is VMSnapshot vMSnapshot)
			{
				return vMSnapshot.Id;
			}
			return Guid.Empty;
		}
	}

	public string VMSnapshotName
	{
		get
		{
			if (_parentComputeResource is VMSnapshot vMSnapshot)
			{
				return vMSnapshot.Name;
			}
			return string.Empty;
		}
	}

	internal VMComponentObject(IVirtualizationManagementObject dataSource, ComputeResource parentComputeResource)
		: base(dataSource)
	{
		_parentComputeResource = parentComputeResource;
	}

	internal T GetParentAs<T>() where T : ComputeResource
	{
		return _parentComputeResource as T;
	}
}
