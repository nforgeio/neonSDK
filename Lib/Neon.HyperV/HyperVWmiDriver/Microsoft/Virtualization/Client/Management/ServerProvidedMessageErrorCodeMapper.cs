namespace Microsoft.Virtualization.Client.Management;

internal class ServerProvidedMessageErrorCodeMapper : ErrorCodeMapper
{
	public override string MapError(VirtualizationOperation operation, long errorCode, string operationFailedMsg)
	{
		return operationFailedMsg;
	}
}
