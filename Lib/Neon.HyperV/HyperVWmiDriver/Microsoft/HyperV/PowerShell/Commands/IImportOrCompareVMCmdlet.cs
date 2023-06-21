using System.Management.Automation;

namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IImportOrCompareVMCmdlet : IServerParameters, IParameterSet
{
    string Path { get; set; }

    SwitchParameter Register { get; }

    SwitchParameter Copy { get; }

    SwitchParameter GenerateNewId { get; }

    string VirtualMachinePath { get; set; }

    string SnapshotFilePath { get; set; }

    string SmartPagingFilePath { get; set; }

    string VhdDestinationPath { get; set; }

    string VhdSourcePath { get; set; }
}
