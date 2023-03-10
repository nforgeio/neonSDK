#define TRACE
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class MetricServiceView : View, IMetricService, IVirtualizationManagementObject
{
	internal static class WmiMemberNames
	{
		public const string ControlMetrics = "ControlMetrics";
	}

	public IMetricServiceSetting Setting => GetRelatedObject<IMetricServiceSetting>(base.Associations.ElementSettingData);

	public void ControlMetrics(IMetricMeasurableElement targetObject, IMetricDefinition metricDefinition, MetricEnabledState targetState)
	{
		object[] args = new object[3]
		{
			targetObject,
			metricDefinition,
			(ushort)targetState
		};
		uint num = InvokeMethod("ControlMetrics", args);
		if (num != View.ErrorCodeSuccess)
		{
			string errorMsg = ((metricDefinition == null) ? string.Format(CultureInfo.CurrentCulture, ErrorMessages.ControlMetricsFailed, targetObject.ManagementPath.ToString(), targetState) : string.Format(CultureInfo.CurrentCulture, ErrorMessages.ControlMetricsByDefinitionFailed, metricDefinition, targetObject.ManagementPath.ToString(), targetState));
			throw ThrowHelper.CreateVirtualizationOperationFailedException(errorMsg, VirtualizationOperation.ControlMetrics, num, GetErrorCodeMapper(), null);
		}
		VMTrace.TraceUserActionCompleted("Controlling metric state completed successfully.");
	}

	internal static MetricEnabledState CalculateAggregatedMetricEnabledState(IEnumerable<IMeasuredElementToMetricDefinitionAssociation> associationInstances)
	{
		ISet<MetricEnabledState> set = new HashSet<MetricEnabledState>(associationInstances.Select((IMeasuredElementToMetricDefinitionAssociation association) => association.EnabledState));
		MetricEnabledState result = MetricEnabledState.Disabled;
		if (set.Contains(MetricEnabledState.PartiallyEnabled))
		{
			result = MetricEnabledState.PartiallyEnabled;
		}
		else
		{
			bool flag = set.Contains(MetricEnabledState.Enabled);
			bool flag2 = set.Contains(MetricEnabledState.Disabled);
			if (flag && flag2)
			{
				result = MetricEnabledState.PartiallyEnabled;
			}
			else if (flag)
			{
				result = MetricEnabledState.Enabled;
			}
			else if (flag2)
			{
				result = MetricEnabledState.Disabled;
			}
		}
		return result;
	}
}
