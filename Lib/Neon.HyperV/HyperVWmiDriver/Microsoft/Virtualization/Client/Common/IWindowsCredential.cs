using System;

namespace Microsoft.Virtualization.Client.Common;

internal interface IWindowsCredential : IUserPassCredential, IEquatable<IUserPassCredential>, IDisposable, IEquatable<IWindowsCredential>
{
    string EncryptedPassword { get; }

    bool IsPersistent { get; }

    string LogonName { get; }

    string TargetName { get; }
}
