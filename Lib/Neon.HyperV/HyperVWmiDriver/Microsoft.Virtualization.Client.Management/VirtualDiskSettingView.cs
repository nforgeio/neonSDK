using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class VirtualDiskSettingView : VirtualDiskPoolAllocationSettingView, IVirtualDiskSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable, IMetricMeasurableElement, IVirtualDiskPoolAllocationSetting, IResourcePoolAllocationSetting
{
	internal static class WmiMemberNames
	{
		public const string Path = "HostResource";

		public const string Drive = "Parent";

		public const string MinimumIOPS = "IOPSReservation";

		public const string MaximumIOPS = "IOPSLimit";

		public const string StorageQoSPolicyID = "StorageQoSPolicyID";

		public const string PrSupported = "PersistentReservationsSupported";

		public const string WriteHardeningMethod = "WriteHardeningMethod";
	}

	public string Path
	{
		get
		{
			object[] property = GetProperty<object[]>("HostResource");
			if (property != null && property.Length != 0)
			{
				return (string)property[0];
			}
			return null;
		}
		set
		{
			SetProperty("HostResource", new string[1] { value ?? string.Empty });
		}
	}

	public IVMDriveSetting DriveSetting
	{
		get
		{
			IVMDriveSetting result = null;
			string property = GetProperty<string>("Parent");
			if (!string.IsNullOrEmpty(property))
			{
				result = (IVMDriveSetting)GetViewFromPath(property);
			}
			return result;
		}
		set
		{
			SetProperty("Parent", (value != null) ? value.ManagementPath.ToString() : string.Empty);
		}
	}

	public IVirtualDiskResourcePool ResourcePool
	{
		get
		{
			string query = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM {0} WHERE ResourceType = {1} AND PoolId = \"{2}\"", "Msvm_ResourcePool", 31, base.PoolId);
			QueryAssociation association = QueryAssociation.CreateFromQuery(base.Server.VirtualizationNamespace, query);
			return GetRelatedObject<IVirtualDiskResourcePool>(association, throwIfNotFound: false);
		}
	}

	public ulong MinimumIOPS
	{
		get
		{
			ulong result = 0uL;
			object property = GetProperty<object>("IOPSReservation");
			if (property != null)
			{
				result = (ulong)property;
			}
			return result;
		}
		set
		{
			SetProperty("IOPSReservation", value);
		}
	}

	public ulong MaximumIOPS
	{
		get
		{
			ulong result = 0uL;
			object property = GetProperty<object>("IOPSLimit");
			if (property != null)
			{
				result = (ulong)property;
			}
			return result;
		}
		set
		{
			SetProperty("IOPSLimit", value);
		}
	}

	public Guid StorageQoSPolicyID
	{
		get
		{
			Guid result = Guid.Empty;
			string property = GetProperty<string>("StorageQoSPolicyID");
			if (!string.IsNullOrEmpty(property))
			{
				result = new Guid(property);
			}
			return result;
		}
		set
		{
			SetProperty("StorageQoSPolicyID", value.ToString());
		}
	}

	public bool PersistentReservationsSupported
	{
		get
		{
			bool result = false;
			object property = GetProperty<object>("PersistentReservationsSupported");
			if (property != null)
			{
				result = (bool)property;
			}
			return result;
		}
		set
		{
			SetProperty("PersistentReservationsSupported", value);
		}
	}

	public ushort WriteHardeningMethod
	{
		get
		{
			return GetPropertyOrDefault("WriteHardeningMethod", (ushort)0);
		}
		set
		{
			SetProperty("WriteHardeningMethod", value);
		}
	}

	public MetricEnabledState AggregateMetricEnabledState => MetricServiceView.CalculateAggregatedMetricEnabledState(GetRelatedObjects<IMeasuredElementToMetricDefinitionAssociation>(base.Associations.MeasuredElementToMetricDefRelationship));

	public IReadOnlyCollection<IMetricValue> GetMetricValues()
	{
		return GetRelatedObjects<IMetricValue>(base.Associations.MeasuredElementToMetricValue).ToList();
	}
}
