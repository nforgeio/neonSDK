namespace Microsoft.Virtualization.Client.Management;

internal class ExternalFcPortView : VMDeviceView, IExternalFcPort, IVMDevice, IVirtualizationManagementObject
{
	internal static class WmiPropertyNames
	{
		public const string WorldWideNodeName = "WWNN";

		public const string WorldWidePortName = "WWPN";

		public const string IsHyperVCapable = "IsHyperVCapable";

		public const string OperationalStatus = "OperationalStatus";
	}

	private const int FCPORT_OPERATIONAL_STATUS_DORMANT = 15;

	public string WorldWideNodeName => GetProperty<string>("WWNN");

	public string WorldWidePortName => GetProperty<string>("WWPN");

	public bool IsHyperVCapable => GetProperty<bool>("IsHyperVCapable");

	public int OperationalStatus
	{
		get
		{
			ushort[] property = GetProperty<ushort[]>("OperationalStatus");
			int result = 15;
			if (property != null && property.Length != 0)
			{
				result = NumberConverter.UInt16ToInt32(property[0]);
			}
			return result;
		}
	}

	public IFcEndpoint FcEndpoint => GetRelatedObject<IFcEndpoint>(base.Associations.ExternalFcPortToFcEndpointAssociation);

	public IVirtualFcSwitch GetVirtualFcSwitch()
	{
		return ((FcEndpoint.OtherEndpoint ?? throw new ObjectNotFoundException(ErrorMessages.ExternalFcPortToSwitchConnectionError)).SwitchPort ?? throw new ObjectNotFoundException(ErrorMessages.ExternalFcPortToSwitchConnectionError)).VirtualFcSwitch;
	}
}
