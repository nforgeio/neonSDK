namespace Microsoft.Virtualization.Client.Management;

internal enum ServerConnectionIssue
{
	Unknown,
	ServerResolution,
	AccessDenied,
	InvalidClass,
	InvalidNamespace,
	RpcServerUnavailable,
	ConnectedWithDifferentCredentials,
	CredentialsNotSupportedOnLocalHost,
	IncompatibleVersion,
	CredSspNotEnabledOnClient,
	ClientCannotDelegateCredentials
}
