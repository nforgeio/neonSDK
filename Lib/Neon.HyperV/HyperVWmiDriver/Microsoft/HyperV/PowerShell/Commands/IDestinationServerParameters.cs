using System.Management.Automation;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IDestinationServerParameters
{
    CimSession DestinationCimSession { get; }

    string DestinationHost { get; }

    PSCredential DestinationCredential { get; }
}
