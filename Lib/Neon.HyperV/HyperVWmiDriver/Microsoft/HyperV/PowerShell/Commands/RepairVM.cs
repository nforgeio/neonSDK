using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Repair", "VM", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMCompatibilityReport) })]
internal sealed class RepairVM : VirtualizationCmdlet<VMCompatibilityReport>, ISupportsPassthrough
{
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Need this to hide the inherited parameter.")]
    [SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility", Justification = "Need this to hide the inherited parameter.")]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    private new CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Need this to hide the inherited parameter.")]
    [SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility", Justification = "Need this to hide the inherited parameter.")]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    private new string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Need this to hide the inherited parameter.")]
    [SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility", Justification = "Need this to hide the inherited parameter.")]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    private new PSCredential[] Credential { get; set; }

    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
    [ValidateNotNull]
    public VMCompatibilityReport CompatibilityReport { get; set; }

    [Parameter]
    [Alias(new string[] { "CheckpointFileLocation", "SnapshotFileLocation" })]
    public string SnapshotFilePath { get; set; }

    [Parameter]
    public string Path { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    protected override void NormalizeParameters()
    {
        if (!string.IsNullOrEmpty(Path))
        {
            Path = PathUtility.GetFullPath(Path, base.CurrentFileSystemLocation);
        }
        if (!string.IsNullOrEmpty(SnapshotFilePath))
        {
            SnapshotFilePath = PathUtility.GetFullPath(SnapshotFilePath, base.CurrentFileSystemLocation);
        }
        base.NormalizeParameters();
    }

    internal override IList<VMCompatibilityReport> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return new VMCompatibilityReport[1] { CompatibilityReport };
    }

    internal override void ProcessOneOperand(VMCompatibilityReport compatibilityReport, IOperationWatcher operationWatcher)
    {
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RepairVM)))
        {
            if (!string.IsNullOrEmpty(Path))
            {
                compatibilityReport.VM.Path = Path;
            }
            if (!string.IsNullOrEmpty(SnapshotFilePath))
            {
                compatibilityReport.VM.ImportSnapshotsFromFolder(operationWatcher, SnapshotFilePath);
            }
            if ((bool)Passthru)
            {
                operationWatcher.WriteObject(compatibilityReport);
            }
        }
    }
}
