#define TRACE
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Virtualization.Client;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Start", "VMTrace")]
internal sealed class StartVMTrace : PSCmdlet
{
    private static readonly VMTraceLevels[] gm_CmdletToVirtManLevels = new VMTraceLevels[5]
    {
        VMTraceLevels.None,
        VMTraceLevels.Error,
        VMTraceLevels.Error | VMTraceLevels.Warning,
        VMTraceLevels.Error | VMTraceLevels.Warning | VMTraceLevels.UserActions | VMTraceLevels.Information,
        VMTraceLevels.Error | VMTraceLevels.Warning | VMTraceLevels.UserActions | VMTraceLevels.Information | VMTraceLevels.WmiCalls | VMTraceLevels.WmiEvents | VMTraceLevels.Verbose
    };

    [Parameter(Position = 0, Mandatory = true)]
    [ValidateSet(new string[] { "Error", "Warning", "Info", "Verbose" }, IgnoreCase = true)]
    public TraceLevel Level { get; set; }

    [Parameter]
    public SwitchParameter TraceVerboseObjects { get; set; }

    [Parameter]
    public string Path { get; set; }

    protected override void ProcessRecord()
    {
        base.ProcessRecord();
        if (!string.IsNullOrEmpty(Path) && !string.IsNullOrEmpty(VMTrace.TraceFilePath))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.StartVMTrace_CannotSetTraceFile);
        }
        VMTrace.Initialize(GetVmTraceLevels(), VMTraceTagFormatLevels.Timestamp, (!string.IsNullOrEmpty(Path)) ? global::System.IO.Path.GetFullPath(Path) : null);
    }

    private VMTraceLevels GetVmTraceLevels()
    {
        int level = (int)Level;
        if (level <= 0 || level >= gm_CmdletToVirtManLevels.Length)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_ParameterOutOfRange, "Level"));
        }
        VMTraceLevels vMTraceLevels = gm_CmdletToVirtManLevels[level];
        if (TraceVerboseObjects.IsPresent)
        {
            if (Level < TraceLevel.Verbose)
            {
                throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.StartVMTrace_DumpObjectsRequiresVerboseLevel);
            }
            vMTraceLevels |= VMTraceLevels.VerboseWmiGetProperties;
            vMTraceLevels |= VMTraceLevels.VerboseWmiEventProperties;
        }
        return vMTraceLevels;
    }
}
