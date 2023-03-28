using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMMeteringReportForVirtualMachine : VMResourceReport
{
	private VirtualMachine m_VirtualMachine;

	public Guid VMId => m_VirtualMachine.Id;

	public string VMName => m_VirtualMachine.Name;

	internal VMMeteringReportForVirtualMachine(VirtualMachine vm)
		: base(vm?.Server)
	{
		if (vm == null)
		{
			throw new ArgumentNullException("vm");
		}
		m_VirtualMachine = vm;
		List<IMetricValue> values = ((IMeasurableInternal)m_VirtualMachine).GetMetricValues().ToList();
		ParseMetricValues(values);
		ParsePortAclMetricValues();
		base.HardDiskMetrics = (from drive in m_VirtualMachine.GetVirtualHardDiskDrives()
			select new VHDMetrics(drive)).ToArray();
	}

	private void ParsePortAclMetricValues()
	{
		List<VMNetworkAdapterPortAclMeteringReport> list = new List<VMNetworkAdapterPortAclMeteringReport>();
		foreach (VMNetworkAdapter adapter in m_VirtualMachine.NetworkAdapters)
		{
			foreach (VMNetworkAdapterAclSetting acl2 in adapter.AclList.Where((VMNetworkAdapterAclSetting acl) => acl.Action == VMNetworkAdapterAclAction.Meter))
			{
				IEnumerable<IMetricValue> metricValues = acl2.GetMetricValues();
				list.AddRange(metricValues.Select((IMetricValue value) => new VMNetworkAdapterPortAclMeteringReport(adapter, acl2.LocalAddress, acl2.RemoteAddress, acl2.Direction, value.IntegerValue)));
				UpdateMeteringDuration(metricValues);
			}
		}
		VMPortAclMeteringReport[] networkMeteredTrafficReport = list.ToArray();
		base.NetworkMeteredTrafficReport = networkMeteredTrafficReport;
	}
}
