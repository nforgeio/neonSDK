#define TRACE
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Virtualization.Client;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Stop", "VMTrace")]
internal sealed class StopVMTrace : PSCmdlet
{
	protected override void ProcessRecord()
	{
		base.ProcessRecord();
		string traceFilePath = VMTrace.TraceFilePath;
		if (traceFilePath != null)
		{
			VMTrace.CloseLogFile();
			WriteVerbose(string.Format(CultureInfo.CurrentCulture, CmdletResources.StopVMTrace_TraceFileWrittenTo, traceFilePath));
		}
	}
}
