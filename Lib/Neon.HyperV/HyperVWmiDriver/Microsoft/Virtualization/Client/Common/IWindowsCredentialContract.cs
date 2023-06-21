using System;
using System.Security;

namespace Microsoft.Virtualization.Client.Common;

internal abstract class IWindowsCredentialContract : IWindowsCredential, IUserPassCredential, IEquatable<IUserPassCredential>, IDisposable, IEquatable<IWindowsCredential>
{
    string IWindowsCredential.EncryptedPassword => null;

    bool IWindowsCredential.IsPersistent => false;

    string IWindowsCredential.LogonName => null;

    string IWindowsCredential.TargetName => null;

    public abstract string DomainName { get; }

    public abstract SecureString Password { get; }

    public abstract string UserName { get; }

    public abstract bool Equals(IUserPassCredential other);

    public abstract void Dispose();

    public abstract bool Equals(IWindowsCredential other);
}
