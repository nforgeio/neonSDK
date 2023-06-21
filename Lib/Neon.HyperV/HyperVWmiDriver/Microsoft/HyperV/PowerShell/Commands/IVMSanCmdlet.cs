using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IVMSanCmdlet : IParameterSet
{
    CimInstance[] HostBusAdapter { get; }

    string[] WorldWideNodeName { get; }

    string[] WorldWidePortName { get; }
}
