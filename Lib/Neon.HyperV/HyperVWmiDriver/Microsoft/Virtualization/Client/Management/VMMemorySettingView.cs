using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class VMMemorySettingView : ResourcePoolAllocationSettingView, IVMMemorySetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IMetricMeasurableElement
{
	internal new static class WmiPropertyNames
	{
		public const string VirtualQuantity = "VirtualQuantity";

		public const string MinimumMemory = "Reservation";

		public const string MaximumMemory = "Limit";

		public const string PriorityLevel = "Weight";

		public const string TargetMemoryBuffer = "TargetMemoryBuffer";

		public const string DynamicMemoryEnabled = "DynamicMemoryEnabled";

		public const string MaximumMemoryPerNumaNode = "MaxMemoryBlocksPerNumaNode";
	}

	public long AllocatedRam
	{
		get
		{
			return NumberConverter.UInt64ToInt64(GetProperty<ulong>("VirtualQuantity"));
		}
		set
		{
			ulong num = NumberConverter.Int64ToUInt64(value);
			SetProperty("VirtualQuantity", num);
		}
	}

	public long MinimumMemory
	{
		get
		{
			return NumberConverter.UInt64ToInt64(GetProperty<ulong>("Reservation"));
		}
		set
		{
			ulong num = NumberConverter.Int64ToUInt64(value);
			SetProperty("Reservation", num);
		}
	}

	public long MaximumMemoryPerNumaNode
	{
		get
		{
			return NumberConverter.UInt64ToInt64(GetProperty<ulong>("MaxMemoryBlocksPerNumaNode"));
		}
		set
		{
			ulong num = NumberConverter.Int64ToUInt64(value);
			SetProperty("MaxMemoryBlocksPerNumaNode", num);
		}
	}

	public long MaximumMemory
	{
		get
		{
			return NumberConverter.UInt64ToInt64(GetProperty<ulong>("Limit"));
		}
		set
		{
			ulong num = NumberConverter.Int64ToUInt64(value);
			SetProperty("Limit", num);
		}
	}

	public int PriorityLevel
	{
		get
		{
			return NumberConverter.UInt32ToInt32(GetProperty<uint>("Weight"));
		}
		set
		{
			uint num = NumberConverter.Int32ToUInt32(value);
			SetProperty("Weight", num);
		}
	}

	public int TargetMemoryBuffer
	{
		get
		{
			return NumberConverter.UInt32ToInt32(GetProperty<uint>("TargetMemoryBuffer"));
		}
		set
		{
			uint num = NumberConverter.Int32ToUInt32(value);
			SetProperty("TargetMemoryBuffer", num);
		}
	}

	public bool IsDynamicMemoryEnabled
	{
		get
		{
			return GetProperty<bool>("DynamicMemoryEnabled");
		}
		set
		{
			SetProperty("DynamicMemoryEnabled", value);
		}
	}

	public MetricEnabledState AggregateMetricEnabledState => MetricServiceView.CalculateAggregatedMetricEnabledState(GetRelatedObjects<IMeasuredElementToMetricDefinitionAssociation>(base.Associations.MeasuredElementToMetricDefRelationship));

	public override VMDeviceSettingType VMDeviceSettingType => VMDeviceSettingType.Memory;

	public IReadOnlyCollection<IMetricValue> GetMetricValues()
	{
		return GetRelatedObjects<IMetricValue>(base.Associations.MeasuredElementToMetricValue).ToList();
	}
}
