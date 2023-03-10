using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Management.Infrastructure;

namespace Microsoft.Virtualization.Client.Management;

internal class VMTaskAssociationObject : IVMTaskAssociationObject
{
	internal static class WmiMemberNames
	{
		public const string AffectedElement = "AffectedElement";

		public const string JobPath = "AffectingElement";
	}

	private readonly Server m_Server;

	private readonly WmiObjectPath m_AffectedElementPath;

	private readonly WmiObjectPath m_JobPath;

	public bool IsActingOnVmComputerSystem => m_AffectedElementPath.ClassName == "Msvm_ComputerSystem";

	public string VmComputerSystemInstanceId
	{
		get
		{
			string result = null;
			if (IsActingOnVmComputerSystem)
			{
				result = (string)m_AffectedElementPath.KeyValues["NAME"];
			}
			return result;
		}
	}

	internal VMTaskAssociationObject(Server server, ICimInstance associationObj)
	{
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		if (associationObj == null)
		{
			throw new ArgumentNullException("associationObj");
		}
		if (!string.Equals(associationObj.CimSystemProperties.ClassName, "Msvm_AffectedJobElement", StringComparison.OrdinalIgnoreCase))
		{
			throw new ArgumentException("associationObj must be an Msvm_AffectedJobElement", "associationObj");
		}
		CimInstance instance = (CimInstance)associationObj.CimInstanceProperties["AffectedElement"].Value;
		CimInstance instance2 = (CimInstance)associationObj.CimInstanceProperties["AffectingElement"].Value;
		m_Server = server;
		m_AffectedElementPath = new WmiObjectPath(server, server.VirtualizationNamespace, instance.ToICimInstance());
		m_JobPath = new WmiObjectPath(server, server.VirtualizationNamespace, instance2.ToICimInstance());
	}

	public IVMTask GetTask()
	{
		return (IVMTask)ObjectLocator.GetVirtualizationManagementObject(m_Server, m_JobPath);
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
	private void ObjectInvariant()
	{
	}
}
