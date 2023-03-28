namespace Microsoft.Virtualization.Client.Management;

internal class LanEndpointView : View, ILanEndpoint, IVirtualizationManagementObject
{
	internal static class WmiPropertyNames
	{
		public const string OperationalStatus = "OperationalStatus";

		public const string StatusDescriptions = "StatusDescriptions";
	}

	public IEthernetPort EthernetPort => GetRelatedObject<IEthernetPort>(base.Associations.DeviceSAPImplementation);

	public ILanEndpoint OtherEndpoint => GetRelatedObject<ILanEndpoint>(base.Associations.ActiveConnection, throwIfNotFound: false);

	public VMLanEndpointOperationalStatus[] OperationalStatus
	{
		get
		{
			ushort[] property = GetProperty<ushort[]>("OperationalStatus");
			VMLanEndpointOperationalStatus[] array = new VMLanEndpointOperationalStatus[property.Length];
			for (int i = 0; i < property.Length; i++)
			{
				array[i] = (VMLanEndpointOperationalStatus)property[i];
			}
			return array;
		}
	}

	public string[] StatusDescriptions => GetProperty<string[]>("StatusDescriptions");
}
