using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Import", "VM", DefaultParameterSetName = "Register", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VirtualMachine) })]
internal sealed class ImportVM : VirtualizationCreationCmdlet<VirtualMachine>, IImportOrCompareVMCmdlet, IServerParameters, IParameterSet, ISupportsAsJob
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "Register")]
    [Parameter(ParameterSetName = "Copy")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "Register")]
    [Parameter(ParameterSetName = "Copy")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "Register")]
    [Parameter(ParameterSetName = "Copy")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = "CompatibilityReport", Position = 0, ValueFromPipeline = true)]
    public VMCompatibilityReport CompatibilityReport { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = "Register", Position = 0, ValueFromPipeline = true)]
    [Parameter(Mandatory = true, ParameterSetName = "Copy", Position = 0, ValueFromPipeline = true)]
    [ValidateNotNullOrEmpty]
    public string Path { get; set; }

    [Parameter(Mandatory = false, ParameterSetName = "Copy", Position = 1)]
    public string VhdDestinationPath { get; set; }

    [Parameter(ParameterSetName = "Register")]
    public SwitchParameter Register { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = "Copy")]
    public SwitchParameter Copy { get; set; }

    [Parameter(ParameterSetName = "Copy")]
    public string VirtualMachinePath { get; set; }

    [Parameter(ParameterSetName = "Copy")]
    [Alias(new string[] { "CheckpointFileLocation", "SnapshotFileLocation" })]
    public string SnapshotFilePath { get; set; }

    [Parameter(ParameterSetName = "Copy")]
    public string SmartPagingFilePath { get; set; }

    [Parameter(ParameterSetName = "Copy")]
    public string VhdSourcePath { get; set; }

    [Parameter(ParameterSetName = "Copy")]
    public SwitchParameter GenerateNewId { get; set; }

    [Parameter]
    public SwitchParameter AsJob { get; set; }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();
        if (!CurrentParameterSetIs("CompatibilityReport"))
        {
            ParameterValidator.ValidateImportOrCompareVMParameters(this);
        }
        else if (CompatibilityReport.OperationType != OperationType.ImportVirtualMachine)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_CompatibilityReportMustBeForImport);
        }
    }

    protected override void NormalizeParameters()
    {
        ParameterNormalizers.NormalizeImportOrCompareVMParameters(this, base.CurrentFileSystemLocation);
        base.NormalizeParameters();
    }

    internal override IList<VirtualMachine> CreateObjects(IOperationWatcher operationWatcher)
    {
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_ImportVM)))
        {
            List<VMCompatibilityReport> inputs = ((!CurrentParameterSetIs("CompatibilityReport")) ? ParameterResolvers.GenerateCompatibilityReports(this, operationWatcher).ToList() : new List<VMCompatibilityReport> { VMCompatibilityReport.RegenerateReportForImport(CompatibilityReport, operationWatcher) });
            return inputs.SelectWithLogging((VMCompatibilityReport report) => RealizeFromCompatibilityReport(operationWatcher, report), operationWatcher).ToList();
        }
        return new VirtualMachine[0];
    }

    private VirtualMachine RealizeFromCompatibilityReport(IOperationWatcher operationWatcher, VMCompatibilityReport report)
    {
        try
        {
            if (report.Incompatibilities.Length != 0)
            {
                throw ExceptionHelper.CreateOperationFailedException(CmdletErrorMessages.ImportVM_FailedValidate);
            }
            if (!report.VhdSourcePath.IsNullOrEmpty() && !report.VhdDestinationPath.IsNullOrEmpty())
            {
                report.VM.PerformStorageCopyForImport(report.VhdSourcePath, report.VhdDestinationPath);
            }
            return VirtualMachine.Realize(report.VM, operationWatcher);
        }
        catch (Exception)
        {
            if (!CurrentParameterSetIs("CompatibilityReport"))
            {
                try
                {
                    if (!report.VM.IsDeleted)
                    {
                        ((IRemovable)report.VM).Remove(operationWatcher);
                    }
                }
                catch (Exception e)
                {
                    ExceptionHelper.DisplayErrorOnException(e, operationWatcher);
                }
            }
            throw;
        }
    }
}
