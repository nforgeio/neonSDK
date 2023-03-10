using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal abstract class VirtualizationObject
{
	[Flags]
	private enum StringFormatFlags
	{
		None = 0,
		IncludeTypeName = 1,
		IncludePropertyNames = 2
	}

	public CimSession CimSession => Server.Session.Session;

	public string ComputerName => Server.Name;

	public bool IsDeleted { get; internal set; }

	internal Server Server { get; private set; }

	internal VirtualizationObject(IVirtualizationManagementObject dataSource)
		: this(dataSource.Server)
	{
	}

	internal VirtualizationObject(Server server)
	{
		Server = server;
	}

	internal void OnDeleted(object sender, EventArgs e)
	{
		IsDeleted = true;
	}

	internal DataUpdater<TVirtManObject> InitializePrimaryDataUpdater<TVirtManObject>(TVirtManObject dataSource) where TVirtManObject : class, IVirtualizationManagementObject
	{
		DataUpdater<TVirtManObject> dataUpdater = new DataUpdater<TVirtManObject>(dataSource);
		dataUpdater.Deleted += OnDeleted;
		return dataUpdater;
	}

	public override string ToString()
	{
		return ToString(IdentifierFlags.UniqueIdentifier | IdentifierFlags.FriendlyName, StringFormatFlags.IncludeTypeName | StringFormatFlags.IncludePropertyNames);
	}

	internal string ToString(string format)
	{
		IdentifierFlags identifierFlags = IdentifierFlags.None;
		StringFormatFlags stringFormatFlags = StringFormatFlags.None;
		switch (format)
		{
		case "C":
			identifierFlags = IdentifierFlags.UniqueIdentifier | IdentifierFlags.FriendlyName;
			stringFormatFlags = StringFormatFlags.IncludeTypeName | StringFormatFlags.IncludePropertyNames;
			break;
		case "c":
			identifierFlags = IdentifierFlags.UniqueIdentifier | IdentifierFlags.FriendlyName;
			stringFormatFlags = StringFormatFlags.IncludeTypeName;
			break;
		case "F":
			identifierFlags = IdentifierFlags.FriendlyName;
			stringFormatFlags = StringFormatFlags.IncludeTypeName | StringFormatFlags.IncludePropertyNames;
			break;
		case "f":
			identifierFlags = IdentifierFlags.FriendlyName;
			stringFormatFlags = StringFormatFlags.IncludeTypeName;
			break;
		case "I":
			identifierFlags = IdentifierFlags.UniqueIdentifier;
			stringFormatFlags = StringFormatFlags.IncludeTypeName | StringFormatFlags.IncludePropertyNames;
			break;
		case "i":
			identifierFlags = IdentifierFlags.UniqueIdentifier;
			stringFormatFlags = StringFormatFlags.IncludeTypeName;
			break;
		case "N":
			identifierFlags = IdentifierFlags.FriendlyName;
			stringFormatFlags = StringFormatFlags.IncludePropertyNames;
			break;
		case "n":
			identifierFlags = IdentifierFlags.FriendlyName;
			break;
		case "U":
			identifierFlags = IdentifierFlags.UniqueIdentifier;
			stringFormatFlags = StringFormatFlags.IncludePropertyNames;
			break;
		case "u":
			identifierFlags = IdentifierFlags.UniqueIdentifier;
			break;
		default:
			throw new FormatException(ErrorMessages.InvalidParameter_UnknownFormat);
		}
		return ToString(identifierFlags, stringFormatFlags);
	}

	private string ToString(IdentifierFlags selectionFlags, StringFormatFlags formatFlags)
	{
		Type type = GetType();
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		foreach (PropertyInfo item3 in from property in type.GetProperties()
			where property.IsDefined(typeof(VirtualizationObjectIdentifierAttribute), inherit: true)
			select property)
		{
			MethodInfo getMethod = item3.GetMethod;
			if (getMethod != null)
			{
				object arg = getMethod.Invoke(this, null);
				IdentifierFlags identifierFlags = selectionFlags & item3.GetCustomAttribute<VirtualizationObjectIdentifierAttribute>().Flags;
				if (identifierFlags.HasFlag(IdentifierFlags.FriendlyName))
				{
					list.Add(string.Format(CultureInfo.InvariantCulture, formatFlags.HasFlag(StringFormatFlags.IncludePropertyNames) ? "{0} = '{1}'" : "{1}", item3.Name, arg));
				}
				if (identifierFlags.HasFlag(IdentifierFlags.UniqueIdentifier) && !identifierFlags.HasFlag(IdentifierFlags.FriendlyName))
				{
					list2.Add(string.Format(CultureInfo.InvariantCulture, formatFlags.HasFlag(StringFormatFlags.IncludePropertyNames) ? "{0} = '{1}'" : "{1}", item3.Name, arg));
				}
			}
		}
		List<string> list3 = new List<string>();
		if (formatFlags.HasFlag(StringFormatFlags.IncludeTypeName))
		{
			list3.Add(type.Name);
		}
		if (list.Count != 0)
		{
			string item = ((formatFlags.HasFlag(StringFormatFlags.IncludeTypeName) || list.Count != 1) ? string.Format(CultureInfo.InvariantCulture, (formatFlags.HasFlag(StringFormatFlags.IncludeTypeName) || list.Count != 1) ? "({0})" : "{0}", string.Join(", ", list)) : list.First());
			list3.Add(item);
		}
		if (list2.Count != 0)
		{
			string item2 = ((formatFlags.HasFlag(formatFlags & StringFormatFlags.IncludeTypeName) || list2.Count != 1 || list.Count != 0) ? string.Format(CultureInfo.InvariantCulture, "[{0}]", string.Join(", ", list2)) : list2.First());
			list3.Add(item2);
		}
		return string.Join(" ", list3);
	}
}
