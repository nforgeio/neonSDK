using System.Collections.Generic;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal interface IMeasurableInternal
{
	IMetricMeasurableElement GetMeasurableElement(UpdatePolicy policy);

	IEnumerable<IMetricValue> GetMetricValues();
}
