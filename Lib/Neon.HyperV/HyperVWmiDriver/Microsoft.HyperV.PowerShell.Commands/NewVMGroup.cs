using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("New", "VMGroup")]
[OutputType(new Type[] { typeof(VMGroup) })]
internal sealed class NewVMGroup : VirtualizationCreationCmdlet<VMGroup>
{
	[Parameter(Position = 0, Mandatory = true)]
	[ValidateNotNullOrEmpty]
	public string Name { get; set; }

	[Parameter(Position = 1, Mandatory = true)]
	[ValidateNotNullOrEmpty]
	public GroupType GroupType { get; set; }

	[Parameter(Position = 2)]
	[ValidateNotNullOrEmpty]
	public Guid Id { get; set; }

	internal override IList<VMGroup> CreateObjects(IOperationWatcher operationWatcher)
	{
		List<VMGroup> list = new List<VMGroup>();
		foreach (Server server in ParameterResolvers.GetServers(this, operationWatcher))
		{
			if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_NewVMGroup, Name)))
			{
				try
				{
					VMGroup item = VMGroup.Create(server, Name, GroupType, Id, operationWatcher);
					list.Add(item);
				}
				catch (Exception e)
				{
					ExceptionHelper.DisplayErrorOnException(e, operationWatcher);
				}
			}
		}
		return list;
	}
}
