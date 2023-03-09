using System;
using System.Security;

namespace Microsoft.Virtualization.Client.Common;

internal abstract class IUserPassCredentialContract : IUserPassCredential, IEquatable<IUserPassCredential>, IDisposable
{
	string IUserPassCredential.DomainName => null;

	SecureString IUserPassCredential.Password => null;

	string IUserPassCredential.UserName => null;

	public abstract bool Equals(IUserPassCredential other);

	public abstract void Dispose();
}
