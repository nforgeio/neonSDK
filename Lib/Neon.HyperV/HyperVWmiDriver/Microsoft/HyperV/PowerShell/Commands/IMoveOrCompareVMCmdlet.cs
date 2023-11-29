using System.Collections;
using System.Collections.Generic;

namespace Microsoft.HyperV.PowerShell.Commands;

internal interface IMoveOrCompareVMCmdlet : IVMSingularObjectOrNameCmdlet, IVmBySingularObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmBySingularNameCmdlet
{
    string DestinationStoragePath { get; set; }

    string VirtualMachinePath { get; set; }

    string SnapshotFilePath { get; set; }

    string SmartPagingFilePath { get; set; }

    Hashtable[] Vhds { get; }

    IReadOnlyList<VhdMigrationMapping> VhdMigrationMappings { get; set; }
}
