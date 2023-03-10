#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Virtualization.Client.Common;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class ServerNames
{
	private static class Win32ComputerSystemWmiPropertyNames
	{
		public const string DnsHostName = "DNSHostName";
	}

	internal static class IPAddressExtensions
	{
		internal static bool TryParseExact(string ipString, out IPAddress address)
		{
			if (IPAddress.TryParse(ipString, out address) && (address.AddressFamily == AddressFamily.InterNetworkV6 || Regex.IsMatch(ipString, "^([0-9]{1,3}\\.){3}[0-9]{1,3}$")))
			{
				return true;
			}
			address = null;
			return false;
		}
	}

	private const string LocalhostDot = ".";

	public const string LocalhostName = "localhost";

	public string UserSpecifiedName { get; private set; }

	public bool IsLocalhost { get; private set; }

	public string NetBiosName { get; private set; }

	public string FullName { get; private set; }

	public bool HasFullName { get; private set; }

	public IPAddress IPAddress { get; set; }

	public static bool IsLocalhostName(string serverName)
	{
		bool flag = string.Equals(serverName, "localhost", StringComparison.OrdinalIgnoreCase) || string.Equals(serverName, Environment.GetEnvironmentVariable("COMPUTERNAME"), StringComparison.OrdinalIgnoreCase) || string.Equals(serverName, ".", StringComparison.OrdinalIgnoreCase);
		if (!flag)
		{
			try
			{
				IPAddress address = null;
				if (!IPAddressExtensions.TryParseExact(serverName, out address))
				{
					flag = string.Equals(serverName, GetHostEntry("localhost").HostName, StringComparison.OrdinalIgnoreCase);
					return flag;
				}
				flag = IPAddress.IsLoopback(address);
				if (flag)
				{
					return flag;
				}
				flag = GetHostEntry(Environment.GetEnvironmentVariable("COMPUTERNAME")).AddressList.Contains(address);
				return flag;
			}
			catch (SocketException ex)
			{
				VMTrace.TraceWarning("Failed to resolve \"localhost\" via DNS.", ex);
				return flag;
			}
		}
		return flag;
	}

	private static T GetTaskResultOrThrowFirstException<T>(Task<T> task)
	{
		try
		{
			return task.Result;
		}
		catch (AggregateException ex)
		{
			ExceptionDispatchInfo.Capture(ex.Flatten().InnerExceptions.First()).Throw();
			throw;
		}
	}

	private static IPHostEntry GetHostEntry(string hostNameOrAddress)
	{
		return GetTaskResultOrThrowFirstException(GetHostEntrySafeAsync(hostNameOrAddress));
	}

	private static async Task<IPHostEntry> GetHostEntrySafeAsync(string hostNameOrAddress)
	{
		return await Dns.GetHostEntryAsync(hostNameOrAddress).ConfigureAwait(continueOnCapturedContext: false);
	}

	private static IPHostEntry GetHostEntry(IPAddress address)
	{
		return GetTaskResultOrThrowFirstException(GetHostEntrySafeAsync(address));
	}

	private static async Task<IPHostEntry> GetHostEntrySafeAsync(IPAddress address)
	{
		return await Dns.GetHostEntryAsync(address).ConfigureAwait(continueOnCapturedContext: false);
	}

	public static ServerNames Resolve(string nameToResolve, IUserPassCredential credential)
	{
		ServerNames serverNames = ResolveInternal(nameToResolve);
		if (serverNames.FullName != null)
		{
			serverNames.NetBiosName = GetNetBiosNameFromFullName(serverNames.FullName);
		}
		else
		{
			if (serverNames.IPAddress == null)
			{
				throw ThrowHelper.CreateServerConnectionException(nameToResolve, ServerConnectionIssue.ServerResolution, callback: false, null);
			}
			serverNames.NetBiosName = GetNetBiosNameFromWin32ComputerSystemData((!serverNames.IsLocalhost) ? nameToResolve : null, credential);
			serverNames.FullName = serverNames.NetBiosName;
		}
		return serverNames;
	}

	public static ServerNames Resolve(ICimSession cimSession)
	{
		ServerNames serverNames = ResolveInternal(cimSession.ComputerName ?? Environment.GetEnvironmentVariable("COMPUTERNAME"));
		if (serverNames.FullName != null)
		{
			serverNames.NetBiosName = GetNetBiosNameFromFullName(serverNames.FullName);
		}
		else
		{
			if (serverNames.IPAddress == null)
			{
				throw ThrowHelper.CreateServerConnectionException(cimSession.ComputerName ?? Environment.GetEnvironmentVariable("COMPUTERNAME"), ServerConnectionIssue.ServerResolution, callback: false, null);
			}
			serverNames.NetBiosName = GetNetBiosNameFromWin32ComputerSystemData(cimSession);
			serverNames.FullName = serverNames.NetBiosName;
		}
		return serverNames;
	}

	private static ServerNames ResolveInternal(string nameToResolve)
	{
		if (string.Equals(".", nameToResolve, StringComparison.OrdinalIgnoreCase))
		{
			nameToResolve = Environment.GetEnvironmentVariable("COMPUTERNAME");
		}
		ServerNames serverNames = new ServerNames
		{
			UserSpecifiedName = nameToResolve,
			IsLocalhost = IsLocalhostName(nameToResolve),
			HasFullName = false
		};
		IPAddress address = null;
		if (IPAddressExtensions.TryParseExact(nameToResolve, out address))
		{
			serverNames.IPAddress = address;
		}
		try
		{
			IPHostEntry iPHostEntry = ((serverNames.IPAddress != null) ? GetHostEntry(address) : GetHostEntry(nameToResolve));
			serverNames.FullName = iPHostEntry.HostName;
			serverNames.HasFullName = true;
			return serverNames;
		}
		catch (SocketException ex)
		{
			VMTrace.TraceWarning(string.Format(CultureInfo.InvariantCulture, "Unable to resolve server '{0}'.", nameToResolve), ex);
			return serverNames;
		}
	}

	private static string GetNetBiosNameFromWin32ComputerSystemData(string serverName, IUserPassCredential credential)
	{
		try
		{
			using ICimSession cimSession = Server.CreateSession(serverName, credential);
			return GetNetBiosNameFromWin32ComputerSystemDataInternal(cimSession);
		}
		catch (Exception inner)
		{
			throw ThrowHelper.CreateServerConnectionException(serverName ?? "localhost", ServerConnectionIssue.ServerResolution, callback: false, inner);
		}
	}

	private static string GetNetBiosNameFromWin32ComputerSystemData(ICimSession cimSession)
	{
		try
		{
			return GetNetBiosNameFromWin32ComputerSystemDataInternal(cimSession);
		}
		catch (Exception inner)
		{
			throw ThrowHelper.CreateServerConnectionException(cimSession.ComputerName ?? Environment.GetEnvironmentVariable("COMPUTERNAME"), ServerConnectionIssue.ServerResolution, callback: false, inner);
		}
	}

	private static string GetNetBiosNameFromWin32ComputerSystemDataInternal(ICimSession cimSession)
	{
		using (IEnumerator<ICimInstance> enumerator = cimSession.EnumerateInstances(Server.CimV2Namespace, "Win32_ComputerSystem").GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				ICimInstance current = enumerator.Current;
				using (current)
				{
					return (string)current.CimInstanceProperties["DNSHostName"].Value;
				}
			}
		}
		return null;
	}

	internal static string GetNetBiosNameFromFullName(string fullName)
	{
		string text = fullName;
		int num = fullName.IndexOf('.');
		if (num != -1)
		{
			text = fullName.Substring(0, num);
		}
		if (text.Length > 15)
		{
			text = text.Substring(0, 15);
		}
		return text.ToUpperInvariant();
	}
}
