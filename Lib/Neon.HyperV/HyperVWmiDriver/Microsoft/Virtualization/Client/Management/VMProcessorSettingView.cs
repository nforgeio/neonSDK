using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class VMProcessorSettingView : ResourcePoolAllocationSettingView, IVMProcessorSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IMetricMeasurableElement
{
	internal new static class WmiPropertyNames
	{
		public const string Reservation = "Reservation";

		public const string VirtualQuantity = "VirtualQuantity";

		public const string Weight = "Weight";

		public const string MaxProcessorsPerNumaNode = "MaxProcessorsPerNumaNode";

		public const string MaxNumaNodesPerSocket = "MaxNumaNodesPerSocket";

		public const string Limit = "Limit";

		public const string LimitCpuId = "LimitCPUID";

		public const string HwThreadsPerCore = "HwThreadsPerCore";

		public const string LimitProcessorFeatures = "LimitProcessorFeatures";

		public const string EnableHostResourceProtection = "EnableHostResourceProtection";

		public const string ExposeVirtualizationExtensions = "ExposeVirtualizationExtensions";

		public const string EnablePerfmonPmu = "EnablePerfmonPmu";

		public const string EnablePerfmonLbr = "EnablePerfmonLbr";

		public const string EnablePerfmonPebs = "EnablePerfmonPebs";

		public const string EnablePerfmonIpt = "EnablePerfmonIpt";

		public const string EnableLegacyApicMode = "EnableLegacyApicMode";

		public const string AllowACountMCount = "AllowACountMCount";
	}

	public int Reservation
	{
		get
		{
			return NumberConverter.UInt64ToInt32(GetProperty<ulong>("Reservation"));
		}
		set
		{
			SetProperty("Reservation", NumberConverter.Int64ToUInt64(value));
		}
	}

	public int VirtualQuantity
	{
		get
		{
			return NumberConverter.UInt64ToInt32(GetProperty<ulong>("VirtualQuantity"));
		}
		set
		{
			SetProperty("VirtualQuantity", NumberConverter.Int64ToUInt64(value));
		}
	}

	public int Weight
	{
		get
		{
			return NumberConverter.UInt32ToInt32(GetProperty<uint>("Weight"));
		}
		set
		{
			SetProperty("Weight", NumberConverter.Int32ToUInt32(value));
		}
	}

	public long MaxProcessorsPerNumaNode
	{
		get
		{
			return NumberConverter.UInt64ToInt64(GetProperty<ulong>("MaxProcessorsPerNumaNode"));
		}
		set
		{
			ulong num = NumberConverter.Int64ToUInt64(value);
			SetProperty("MaxProcessorsPerNumaNode", num);
		}
	}

	public long MaxNumaNodesPerSocket
	{
		get
		{
			return NumberConverter.UInt64ToInt64(GetProperty<ulong>("MaxNumaNodesPerSocket"));
		}
		set
		{
			ulong num = NumberConverter.Int64ToUInt64(value);
			SetProperty("MaxNumaNodesPerSocket", num);
		}
	}

	public int Limit
	{
		get
		{
			return NumberConverter.UInt64ToInt32(GetProperty<ulong>("Limit"));
		}
		set
		{
			SetProperty("Limit", NumberConverter.Int32ToUInt64(value));
		}
	}

	public bool LimitCpuId
	{
		get
		{
			return GetProperty<bool>("LimitCPUID");
		}
		set
		{
			SetProperty("LimitCPUID", value);
		}
	}

	public long? HwThreadsPerCore
	{
		get
		{
			ulong? propertyOrDefault = GetPropertyOrDefault<ulong?>("HwThreadsPerCore");
			if (propertyOrDefault.HasValue)
			{
				return NumberConverter.UInt64ToInt64(propertyOrDefault.Value);
			}
			return null;
		}
		set
		{
			ulong num = NumberConverter.Int64ToUInt64(value.GetValueOrDefault());
			SetProperty("HwThreadsPerCore", num);
		}
	}

	public bool LimitProcessorFeatures
	{
		get
		{
			return GetProperty<bool>("LimitProcessorFeatures");
		}
		set
		{
			SetProperty("LimitProcessorFeatures", value);
		}
	}

	public bool EnableHostResourceProtection
	{
		get
		{
			return GetProperty<bool>("EnableHostResourceProtection");
		}
		set
		{
			SetProperty("EnableHostResourceProtection", value);
		}
	}

	public bool ExposeVirtualizationExtensions
	{
		get
		{
			return GetPropertyOrDefault("ExposeVirtualizationExtensions", defaultValue: false);
		}
		set
		{
			SetProperty("ExposeVirtualizationExtensions", value);
		}
	}

	public bool EnablePerfmonPmu
	{
		get
		{
			return GetPropertyOrDefault("EnablePerfmonPmu", defaultValue: false);
		}
		set
		{
			SetProperty("EnablePerfmonPmu", value);
		}
	}

	public bool EnablePerfmonLbr
	{
		get
		{
			return GetPropertyOrDefault("EnablePerfmonLbr", defaultValue: false);
		}
		set
		{
			SetProperty("EnablePerfmonLbr", value);
		}
	}

	public bool EnablePerfmonPebs
	{
		get
		{
			return GetPropertyOrDefault("EnablePerfmonPebs", defaultValue: false);
		}
		set
		{
			SetProperty("EnablePerfmonPebs", value);
		}
	}

	public bool EnablePerfmonIpt
	{
		get
		{
			return GetPropertyOrDefault("EnablePerfmonIpt", defaultValue: false);
		}
		set
		{
			SetProperty("EnablePerfmonIpt", value);
		}
	}

	public bool EnableLegacyApicMode
	{
		get
		{
			return GetPropertyOrDefault("EnableLegacyApicMode", defaultValue: false);
		}
		set
		{
			SetProperty("EnableLegacyApicMode", value);
		}
	}

	public bool AllowACountMCount
	{
		get
		{
			return GetPropertyOrDefault("AllowACountMCount", defaultValue: false);
		}
		set
		{
			SetProperty("AllowACountMCount", value);
		}
	}

	public override VMDeviceSettingType VMDeviceSettingType => VMDeviceSettingType.Processor;

	public MetricEnabledState AggregateMetricEnabledState => MetricServiceView.CalculateAggregatedMetricEnabledState(GetRelatedObjects<IMeasuredElementToMetricDefinitionAssociation>(base.Associations.MeasuredElementToMetricDefRelationship));

	public IReadOnlyCollection<IMetricValue> GetMetricValues()
	{
		return GetRelatedObjects<IMetricValue>(base.Associations.MeasuredElementToMetricValue).ToList();
	}
}
