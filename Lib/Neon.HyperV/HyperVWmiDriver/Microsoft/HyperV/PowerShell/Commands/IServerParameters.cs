using System.Management.Automation;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IServerParameters
{
    CimSession[] CimSession { get; }

    string[] ComputerName { get; }

    PSCredential[] Credential { get; }
}
