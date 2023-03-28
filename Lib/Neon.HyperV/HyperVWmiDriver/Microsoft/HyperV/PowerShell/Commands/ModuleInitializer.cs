using System.Management.Automation;

namespace Microsoft.HyperV.PowerShell.Commands;

internal sealed class ModuleInitializer : IModuleAssemblyInitializer
{
	public void OnImport()
	{
		global::System.Management.Automation.PowerShell powerShell = global::System.Management.Automation.PowerShell.Create(RunspaceMode.CurrentRunspace);
		AddAlias(powerShell, "gvm", "Get-VM");
		AddAlias(powerShell, "savm", "Start-VM");
		AddAlias(powerShell, "spvm", "Stop-VM");
		AddAlias(powerShell, "gvmr", "Get-VMReplication");
		AddAlias(powerShell, "mvmr", "Measure-VMReplication");
		AddAlias(powerShell, "gvmrs", "Get-VMReplicationServer");
		AddAlias(powerShell, "Export-VMCheckpoint", "Export-VMSnapshot");
		AddAlias(powerShell, "Get-VMCheckpoint", "Get-VMSnapshot");
		AddAlias(powerShell, "Remove-VMCheckpoint", "Remove-VMSnapshot");
		AddAlias(powerShell, "Rename-VMCheckpoint", "Rename-VMSnapshot");
		AddAlias(powerShell, "Restore-VMCheckpoint", "Restore-VMSnapshot");
		powerShell.Invoke();
	}

	private static void AddAlias(global::System.Management.Automation.PowerShell invoker, string alias, string originalCmdletName)
	{
		invoker.AddCommand("New-Alias").AddParameter("Name", alias).AddParameter("Value", originalCmdletName);
		invoker.AddStatement();
	}
}
