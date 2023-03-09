namespace Microsoft.Virtualization.Client.Management;

internal class VMSecurityInformationView : View, IVMSecurityInformation, IVirtualizationManagementObject
{
	internal static class WmiPropertyNames
	{
		public const string EncryptStateAndVmMigrationTrafficEnabled = "EncryptStateAndVmMigrationTrafficEnabled";

		public const string Shielded = "Shielded";
	}

	public bool Shielded => GetProperty<bool>("Shielded");

	public bool EncryptStateAndVmMigrationTrafficEnabled => GetPropertyOrDefault("EncryptStateAndVmMigrationTrafficEnabled", defaultValue: false);
}
