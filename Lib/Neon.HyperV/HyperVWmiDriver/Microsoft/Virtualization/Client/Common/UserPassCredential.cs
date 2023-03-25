using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security;
using System.Text.RegularExpressions;
using Microsoft.Virtualization.Client.Common.Types.Resources;

namespace Microsoft.Virtualization.Client.Common;

internal class UserPassCredential : IUserPassCredential, IEquatable<IUserPassCredential>, IDisposable
{
	private readonly int _hashCode;

	private bool _disposed;

	public string DomainName { get; private set; }

	public SecureString Password { get; private set; }

	public string UserName { get; private set; }

	protected UserPassCredential(string domain, string userName, SecureString password)
	{
		DomainName = domain;
		UserName = userName;
		Password = password;
		_hashCode = DomainName.ToUpperInvariant().GetHashCode();
		_hashCode ^= UserName.ToUpperInvariant().GetHashCode();
	}

	public static IUserPassCredential Create(string logonName, SecureString password)
	{
		ParseLogonName(logonName, out var user, out var domain);
		return new UserPassCredential(domain, user, password);
	}

	public static void ParseLogonName(string logonName, out string user, out string domain)
	{
		if (string.IsNullOrEmpty(logonName))
		{
			throw new ArgumentNullException("logonName");
		}
		string[] array = logonName.Split('\\');
		if (array.Length == 1)
		{
			string[] array2 = logonName.Split('@');
			if (array2.Length < 2)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.WindowsCredentials_InvalidUserName, logonName), "logonName");
			}
			domain = string.Empty;
			user = array[0];
			if (!IsValidDomainName(array2[array2.Length - 1]) || !IsValidUpn(user))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.WindowsCredentials_InvalidUserName, logonName), "logonName");
			}
		}
		else
		{
			if (array.Length != 2)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.WindowsCredentials_InvalidUserName, logonName), "logonName");
			}
			domain = array[0];
			user = array[1];
			if ((!IsValidNetBiosName(domain) && !IsValidDomainName(domain)) || !IsValidUserName(user))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.WindowsCredentials_InvalidUserName, logonName), "logonName");
			}
		}
	}

	private static bool IsValidNetBiosName(string netBiosName)
	{
		return netBiosName.Length <= 15;
	}

	private static bool IsValidDomainName(string domainName)
	{
		Regex regex = new Regex("^([\\w-]{1," + 63 + "}(\\.|$))+$", RegexOptions.IgnoreCase);
		if (domainName.Length <= 253)
		{
			return regex.IsMatch(domainName);
		}
		return false;
	}

	private static bool IsValidUpn(string userName)
	{
		return userName.Length <= 513;
	}

	private static bool IsValidUserName(string userName)
	{
		return userName.Length <= 256;
	}

	public bool Equals(IUserPassCredential other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		if (string.Equals(DomainName, other.DomainName, StringComparison.OrdinalIgnoreCase))
		{
			return string.Equals(UserName, other.UserName, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public override bool Equals(object other)
	{
		if (other is IUserPassCredential other2)
		{
			return Equals(other2);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _hashCode;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && !_disposed)
		{
			Password.Dispose();
			_disposed = true;
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
	private void ObjectInvariant()
	{
	}
}
