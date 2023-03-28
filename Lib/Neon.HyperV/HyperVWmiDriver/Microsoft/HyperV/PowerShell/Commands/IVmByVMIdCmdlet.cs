using System;

namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IVmByVMIdCmdlet : IVirtualMachineCmdlet, IServerParameters, IParameterSet
{
	Guid[] VMId { get; set; }
}
