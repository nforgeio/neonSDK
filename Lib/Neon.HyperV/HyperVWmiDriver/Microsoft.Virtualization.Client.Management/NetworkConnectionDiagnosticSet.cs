namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_NetworkConnectionDiagnosticSettingData")]
internal class NetworkConnectionDiagnosticSettingData : EmbeddedInstance
{
	internal static class WmiPropertyNames
	{
		public const string IsSender = "IsSender";

		public const string SenderIP = "SenderIP";

		public const string ReceiverIP = "ReceiverIP";

		public const string ReceiverMac = "ReceiverMac";

		public const string IsolationId = "IsolationId";

		public const string SequenceNumber = "SequenceNumber";

		public const string PayloadSize = "PayloadSize";
	}

	public bool IsSender => GetProperty("IsSender", defaultValue: false);

	public string SenderIP => GetProperty<string>("SenderIP");

	public string ReceiverIP => GetProperty<string>("ReceiverIP");

	public string ReceiverMac => GetProperty<string>("ReceiverMac");

	public int IsolationId => GetProperty("IsolationId", 0);

	public int SequenceNumber => GetProperty("SequenceNumber", 0);

	public int PayloadSize => GetProperty("PayloadSize", 0);

	public NetworkConnectionDiagnosticSettingData()
	{
	}

	public NetworkConnectionDiagnosticSettingData(Server server, bool isSender, string senderIP, string receiverIP, string receiverMac, int isolationId, int sequenceNumber, int payloadSize)
		: base(server, server.VirtualizationNamespace, "Msvm_NetworkConnectionDiagnosticSettingData")
	{
		AddProperty("IsSender", isSender);
		AddProperty("SenderIP", senderIP);
		AddProperty("ReceiverIP", receiverIP);
		AddProperty("ReceiverMac", receiverMac);
		AddProperty("IsolationId", isolationId);
		AddProperty("SequenceNumber", sequenceNumber);
		AddProperty("PayloadSize", payloadSize);
	}
}
