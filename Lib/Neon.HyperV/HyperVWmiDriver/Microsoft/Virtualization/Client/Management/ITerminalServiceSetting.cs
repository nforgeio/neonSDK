namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_TerminalServiceSettingData")]
internal interface ITerminalServiceSetting : IVirtualizationManagementObject
{
	int ListenerPort { get; }
}
