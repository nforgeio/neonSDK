using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Vhd.PowerShell;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("New", "VFD", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(FileInfo) })]
internal sealed class NewVfd : VirtualizationCreationCmdlet<FileInfo>
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec. Also arrary is more user friendly.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    public string[] Path { get; set; }

    protected override void NormalizeParameters()
    {
        base.NormalizeParameters();
        Path = Path.Select((string path) => PathUtility.GetFullPath(path, base.CurrentFileSystemLocation)).ToArray();
    }

    internal override IList<FileInfo> CreateObjects(IOperationWatcher operationWatcher)
    {
        return ParameterResolvers.GetServers(this, operationWatcher).SelectMany((Server server) => CreateFloppyDisks(server, Path, operationWatcher)).ToList();
    }

    private IEnumerable<FileInfo> CreateFloppyDisks(Server server, IEnumerable<string> paths, IOperationWatcher operationWatcher)
    {
        return paths.SelectWithLogging((string path) => CreateFloppyDisk(server, path, operationWatcher), operationWatcher);
    }

    private FileInfo CreateFloppyDisk(Server server, string path, IOperationWatcher operationWatcher)
    {
        FileInfo result = null;
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_NewVFD, path)))
        {
            VhdUtilities.CreateVirtualFloppyDisk(server, path, operationWatcher);
            result = new FileInfo(ProcessFullVfdPath(server, path));
        }
        return result;
    }

    private string GetUncPathFromComputerLocalPath(string computerName, string path)
    {
        string text = path.Trim();
        if (text.IndexOf(':') != 1)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_InvalidFullPath, path));
        }
        text = text.Substring(0, 1) + "$" + text.Substring(2, text.Length - 2);
        return "\\\\" + computerName + "\\" + text;
    }

    private string ProcessFullVfdPath(Server server, string path)
    {
        string result = path;
        bool num = !server.IsLocalhost;
        bool flag = !path.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase);
        if (num && flag)
        {
            result = GetUncPathFromComputerLocalPath(server.FullName, path);
        }
        return result;
    }
}
