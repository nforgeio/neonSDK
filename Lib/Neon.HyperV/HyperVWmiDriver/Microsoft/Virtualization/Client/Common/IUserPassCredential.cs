using System;
using System.Security;

namespace Microsoft.Virtualization.Client.Common;

internal interface IUserPassCredential : IEquatable<IUserPassCredential>, IDisposable
{
    string DomainName { get; }

    SecureString Password { get; }

    string UserName { get; }
}
