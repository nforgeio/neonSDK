#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.Virtualization.Client;

internal static class VMTrace
{
	private const string gm_TimeStampFormat = "yyyy-MM-dd hh:mm:ss.fff";

	private static VMTraceTagFormatLevels gm_TagFormatLevel = VMTraceTagFormatLevels.None;

	private static VMTraceLevels gm_TraceLevel = VMTraceLevels.None;

	private static TextWriterTraceListener gm_TraceTextWriterTraceListener;

	private static string gm_TraceFilePath;

	private static StreamWriter gm_TraceOutputStream;

	private static Timer gm_FlushingTimer;

	private static readonly TimeSpan gm_FlushingInterval = TimeSpan.FromSeconds(10.0);

	private static readonly object gm_LockObject = new object();

	public static string TraceFilePath
	{
		get
		{
			lock (gm_LockObject)
			{
				return gm_TraceFilePath;
			}
		}
	}

	[Conditional("TRACE")]
	public static void Initialize(VMTraceLevels level, VMTraceTagFormatLevels formatLevel)
	{
		Initialize(level, formatLevel, null);
	}

	[Conditional("TRACE")]
	public static void Initialize(VMTraceLevels level, VMTraceTagFormatLevels formatLevel, string filePath)
	{
		lock (gm_LockObject)
		{
			if (level != 0)
			{
				if (gm_TraceOutputStream == null)
				{
					try
					{
						if (filePath == null)
						{
							filePath = GetTraceOutputFileName();
						}
						gm_TraceOutputStream = new StreamWriter(filePath, append: true);
						gm_TraceTextWriterTraceListener = new TextWriterTraceListener(gm_TraceOutputStream);
						Trace.Listeners.Add(gm_TraceTextWriterTraceListener);
						gm_TraceFilePath = filePath;
					}
					catch (Exception ex)
					{
						TraceError("Failed in opening trace log file!", ex);
					}
				}
				if (gm_FlushingTimer == null)
				{
					gm_FlushingTimer = new Timer(FlushTraceFileCallback, null, TimeSpan.Zero, gm_FlushingInterval);
				}
			}
			gm_TraceLevel = level;
			gm_TagFormatLevel = formatLevel;
		}
		TraceVersion();
	}

	[Conditional("TRACE")]
	public static void CloseLogFile()
	{
		lock (gm_LockObject)
		{
			if (gm_FlushingTimer != null)
			{
				gm_FlushingTimer.Dispose();
			}
			if (gm_TraceOutputStream != null)
			{
				try
				{
					gm_TraceOutputStream.Flush();
				}
				catch (Exception)
				{
				}
				gm_TraceOutputStream.Dispose();
				gm_TraceOutputStream = null;
				gm_TraceTextWriterTraceListener.Close();
				Trace.Listeners.Remove(gm_TraceTextWriterTraceListener);
				gm_TraceFilePath = null;
			}
		}
	}

	[Conditional("TRACE")]
	private static void TraceVersion()
	{
		Assembly assembly = typeof(VMTrace).GetTypeInfo().Assembly;
		string arg = assembly.GetName().Version.ToString();
		DateTime lastWriteTime = File.GetLastWriteTime(assembly.Location);
		Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "Tracing Hyper-V Client version: {0}. Rough build date (virtman file written): {1}", arg, lastWriteTime));
		Trace.WriteLine(string.Empty);
	}

	[Conditional("TRACE")]
	public static void TraceWarning(string information, Exception ex)
	{
		if (ex != null && (gm_TraceLevel & VMTraceLevels.Warning) == VMTraceLevels.Warning)
		{
			TraceMessage(GetTaggingString("WARNING"), information, ex.Message, new string[1] { ex.StackTrace });
		}
	}

	[Conditional("TRACE")]
	public static void TraceWarning(string information, params string[] verboseInformation)
	{
		if ((gm_TraceLevel & VMTraceLevels.Warning) == VMTraceLevels.Warning)
		{
			TraceMessage(GetTaggingString("WARNING"), information, verboseInformation);
		}
	}

	[Conditional("TRACE")]
	public static void TraceError(string information, Exception ex)
	{
		if (ex != null && (gm_TraceLevel & VMTraceLevels.Error) == VMTraceLevels.Error)
		{
			TraceMessage(GetTaggingString("ERROR"), information, ex.Message, new string[1] { ex.StackTrace });
		}
	}

	[Conditional("TRACE")]
	public static void TraceError(string information, params string[] verboseInformation)
	{
		if ((gm_TraceLevel & VMTraceLevels.Error) == VMTraceLevels.Error)
		{
			TraceMessage(GetTaggingString("ERROR"), information, verboseInformation);
		}
	}

	[Conditional("TRACE")]
	public static void TraceWmiGetProperties(string classPath, CimKeyedCollection<CimProperty> properties)
	{
		if ((gm_TraceLevel & VMTraceLevels.WmiCalls) == VMTraceLevels.WmiCalls)
		{
			string taggingString = GetTaggingString("WMI_CALL_GET_PROPS");
			string message = string.Format(CultureInfo.InvariantCulture, "Get properties of '{0}'", classPath);
			string[] verboseMessages = null;
			VMTraceLevels verboseLevel = VMTraceLevels.VerboseWmiGetProperties;
			if (!string.Equals(classPath, "Win32_Service", StringComparison.OrdinalIgnoreCase))
			{
				verboseMessages = GetVerboseProperties(properties, verboseLevel);
			}
			TraceMessage(taggingString, message, null, verboseMessages, verboseLevel);
		}
	}

	[Conditional("TRACE")]
	public static void TraceWmiQueryComplete(string message)
	{
		if ((gm_TraceLevel & VMTraceLevels.WmiCalls) == VMTraceLevels.WmiCalls)
		{
			TraceMessage(GetTaggingString("WMI_CALL_QUERY_COMPLETE"), message, null);
		}
	}

	[Conditional("TRACE")]
	public static void TraceWmiAssociation(string message, string path, string assocClass, string resultClass)
	{
		if ((gm_TraceLevel & VMTraceLevels.WmiCalls) != VMTraceLevels.WmiCalls)
		{
			return;
		}
		string taggingString = GetTaggingString("WMI_CALL_QUERY");
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "ASSOCIATORS OF '{0}'", path);
		if (!string.IsNullOrEmpty(assocClass) || !string.IsNullOrEmpty(resultClass))
		{
			stringBuilder.Append(" WHERE");
			if (!string.IsNullOrEmpty(assocClass))
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " AssocClass = '{0}'", assocClass);
			}
			if (!string.IsNullOrEmpty(resultClass))
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " ResultClass = '{0}'", resultClass);
			}
		}
		TraceMessage(taggingString, message, stringBuilder.ToString(), null);
	}

	[Conditional("TRACE")]
	public static void TraceWmiRelationship(string message, string path, string assocClass, string sourceRole)
	{
		if ((gm_TraceLevel & VMTraceLevels.WmiCalls) != VMTraceLevels.WmiCalls)
		{
			return;
		}
		string taggingString = GetTaggingString("WMI_CALL_QUERY");
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "REFERENCES OF '{0}'", path);
		if (!string.IsNullOrEmpty(assocClass) || !string.IsNullOrEmpty(sourceRole))
		{
			stringBuilder.Append(" WHERE");
			if (!string.IsNullOrEmpty(assocClass))
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " ResultClass = '{0}'", assocClass);
			}
			if (!string.IsNullOrEmpty(sourceRole))
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " Role = '{0}'", sourceRole);
			}
		}
		TraceMessage(taggingString, message, stringBuilder.ToString(), null);
	}

	[Conditional("TRACE")]
	public static void TraceWmiQueryAssociation(string message, string query)
	{
		if ((gm_TraceLevel & VMTraceLevels.WmiCalls) == VMTraceLevels.WmiCalls)
		{
			TraceMessage(GetTaggingString("WMI_CALL_QUERY"), message, query, null);
		}
	}

	[Conditional("TRACE")]
	public static void TraceWmiObjectGet(WmiObjectPath path)
	{
		if (path != null && (gm_TraceLevel & VMTraceLevels.WmiCalls) == VMTraceLevels.WmiCalls)
		{
			string taggingString = GetTaggingString("WMI_CALL_GET_OBJECT");
			string message = string.Format(CultureInfo.InvariantCulture, "Get WMI object with path: {0}", path.ToString());
			TraceMessage(taggingString, message, null);
		}
	}

	[Conditional("TRACE")]
	public static void TraceWmiWatcher(string watcherInformation)
	{
		if ((gm_TraceLevel & VMTraceLevels.WmiCalls) == VMTraceLevels.WmiCalls)
		{
			TraceMessage(GetTaggingString("WMI_CALL_WATCHER"), watcherInformation, null);
		}
	}

	[Conditional("TRACE")]
	public static void TraceUserActionInitiated(string actionInformation, params string[] verboseInformation)
	{
		if ((gm_TraceLevel & VMTraceLevels.UserActions) == VMTraceLevels.UserActions)
		{
			TraceMessage(GetTaggingString("USER_ACTION_INITIATED"), actionInformation, verboseInformation);
		}
	}

	[Conditional("TRACE")]
	public static void TraceUserActionInitiatedWithProperties(string actionInformation, IDictionary<string, object> properties)
	{
		if ((gm_TraceLevel & VMTraceLevels.UserActions) == VMTraceLevels.UserActions)
		{
			string taggingString = GetTaggingString("USER_ACTION_INITIATED");
			string[] verboseProperties = GetVerboseProperties(properties, VMTraceLevels.Verbose);
			TraceMessage(taggingString, actionInformation, verboseProperties);
		}
	}

	[Conditional("TRACE")]
	public static void TraceUserActionCompleted(string actionInformation, params string[] verboseInformation)
	{
		if ((gm_TraceLevel & VMTraceLevels.UserActions) == VMTraceLevels.UserActions)
		{
			TraceMessage(GetTaggingString("USER_ACTION_COMPLETED"), actionInformation, verboseInformation);
		}
	}

	[Conditional("TRACE")]
	public static void TraceInformation(string informationMessage, params string[] verboseInformation)
	{
		if ((gm_TraceLevel & VMTraceLevels.Information) == VMTraceLevels.Information)
		{
			TraceMessage(GetTaggingString("INFO"), informationMessage, verboseInformation);
		}
	}

	[Conditional("TRACE")]
	public static void TraceWmiEvent(string eventInfo)
	{
		if ((gm_TraceLevel & VMTraceLevels.WmiEvents) == VMTraceLevels.WmiEvents)
		{
			TraceMessage(GetTaggingString("WMI_EVENT"), eventInfo, null);
		}
	}

	[Conditional("TRACE")]
	public static void TraceWmiEventModifiedProperties(string eventInfo, CimKeyedCollection<CimProperty> properties)
	{
		if ((gm_TraceLevel & VMTraceLevels.WmiEvents) == VMTraceLevels.WmiEvents)
		{
			string taggingString = GetTaggingString("WMI_EVENT");
			VMTraceLevels verboseLevel = VMTraceLevels.VerboseWmiEventProperties;
			string[] verboseProperties = GetVerboseProperties(properties, verboseLevel);
			TraceMessage(taggingString, eventInfo, null, verboseProperties, verboseLevel);
		}
	}

	[Conditional("TRACE")]
	public static void TraceWmiGetSummaryInformation(Server server, IList<IVMComputerSystem> virtualMachineList, SummaryInformationRequest requestedInformation)
	{
		if ((gm_TraceLevel & VMTraceLevels.WmiCalls) != VMTraceLevels.WmiCalls)
		{
			return;
		}
		string taggingString = GetTaggingString("WMI_CALL_GET_SUMMARY");
		string message;
		if (virtualMachineList == null || virtualMachineList.Count == 0)
		{
			message = string.Format(CultureInfo.InvariantCulture, "Get {0} summary information for all vms on server {1}.", requestedInformation, server);
		}
		else if (virtualMachineList.Count > 1)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < virtualMachineList.Count; i++)
			{
				stringBuilder.Append(virtualMachineList[i].Name);
				if (i != virtualMachineList.Count - 1)
				{
					stringBuilder.Append(", ");
				}
			}
			message = string.Format(CultureInfo.InvariantCulture, "Get {0} summary information for vms: {2} on server {1}.", requestedInformation, server, stringBuilder);
		}
		else
		{
			message = string.Format(CultureInfo.InvariantCulture, "Get {0} summary information for vm {2} on server {1}.", requestedInformation, server, virtualMachineList[0].Name);
		}
		TraceMessage(taggingString, message, null);
	}

	private static void FlushTraceFileCallback(object state)
	{
		lock (gm_LockObject)
		{
			try
			{
				if (gm_TraceOutputStream != null)
				{
					gm_TraceOutputStream.Flush();
				}
			}
			catch (Exception)
			{
			}
		}
	}

	private static string[] GetVerboseProperties(CimKeyedCollection<CimProperty> properties, VMTraceLevels verboseLevel)
	{
		List<string> list = new List<string>();
		if ((gm_TraceLevel & verboseLevel) == verboseLevel && properties != null)
		{
			foreach (CimProperty property in properties)
			{
				string propertyValueToString = GetPropertyValueToString(property.Value);
				list.Add(string.Format(CultureInfo.InvariantCulture, "{0}={1}", property.Name, propertyValueToString));
			}
		}
		return list.ToArray();
	}

	private static string[] GetVerboseProperties(IDictionary<string, object> properties, VMTraceLevels verboseLevel)
	{
		List<string> list = new List<string>();
		if ((gm_TraceLevel & verboseLevel) == verboseLevel && properties != null)
		{
			foreach (KeyValuePair<string, object> property in properties)
			{
				string propertyValueToString = GetPropertyValueToString(property.Value);
				list.Add(string.Format(CultureInfo.InvariantCulture, "{0}={1}", property.Key, propertyValueToString));
			}
		}
		return list.ToArray();
	}

	private static string GetPropertyValueToString(object propValue)
	{
		if (propValue is object[] array)
		{
			StringBuilder stringBuilder = new StringBuilder();
			object[] array2 = array;
			foreach (object propValue2 in array2)
			{
				stringBuilder.Append(GetPropertyValueToString(propValue2));
				stringBuilder.Append(", ");
			}
			if (array.Length != 0)
			{
				stringBuilder.Remove(stringBuilder.Length - 2, 2);
			}
			return stringBuilder.ToString();
		}
		if (propValue != null)
		{
			return propValue.ToString();
		}
		return "null";
	}

	private static string GetTaggingString(string tagMessage)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if ((gm_TagFormatLevel & VMTraceTagFormatLevels.Timestamp) == VMTraceTagFormatLevels.Timestamp)
		{
			stringBuilder.Append(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff", CultureInfo.InvariantCulture));
			stringBuilder.Append(" ");
		}
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "[{0:0#}] {1}", Thread.CurrentThread.ManagedThreadId, tagMessage);
		if ((gm_TagFormatLevel & VMTraceTagFormatLevels.SourceInformation) == VMTraceTagFormatLevels.SourceInformation)
		{
			StackFrame stackFrame = new StackFrame(2, needFileInfo: true);
			MethodBase method = stackFrame.GetMethod();
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " {0} {1}:{2}()", GetModule(method.DeclaringType.GetTypeInfo().Assembly.GetName().Name), method.DeclaringType.Name, method.Name);
			string fileName = stackFrame.GetFileName();
			if (fileName != null)
			{
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " in {0}:{1}", Path.GetFileName(fileName), stackFrame.GetFileLineNumber());
			}
		}
		stringBuilder.Append(" ");
		return stringBuilder.ToString();
	}

	private static string GetModule(string assemblyName)
	{
		if (!(assemblyName == "Microsoft.Virtualization.Client.Management"))
		{
			if (assemblyName == "vmconnect")
			{
				return "VmConnect";
			}
			int num = assemblyName.LastIndexOf('.');
			if (num != -1 && num < assemblyName.Length - 1)
			{
				return assemblyName.Substring(num + 1);
			}
			return assemblyName;
		}
		return "VirtMan";
	}

	private static void TraceMessage(string tag, string message, string[] verboseMessages)
	{
		TraceMessage(tag, message, null, verboseMessages, VMTraceLevels.Verbose);
	}

	private static void TraceMessage(string tag, string message, string additionalMessage, string[] verboseMessages)
	{
		TraceMessage(tag, message, additionalMessage, verboseMessages, VMTraceLevels.Verbose);
	}

	private static void TraceMessage(string tag, string message, string additionalMessage, string[] verboseMessages, VMTraceLevels verboseLevel)
	{
		lock (gm_LockObject)
		{
			Trace.WriteLine(tag + message);
			if (!string.IsNullOrEmpty(additionalMessage))
			{
				Trace.Indent();
				Trace.WriteLine(additionalMessage);
				Trace.Unindent();
			}
			if (verboseMessages == null || (gm_TraceLevel & verboseLevel) != verboseLevel)
			{
				return;
			}
			Trace.Indent();
			foreach (string text in verboseMessages)
			{
				if (!string.IsNullOrEmpty(text))
				{
					Trace.WriteLine(text);
				}
			}
			Trace.Unindent();
		}
	}

	[SuppressMessage("Microsoft.Security", "CA2122")]
	private static string GetTraceOutputFileName()
	{
		string tempPath = Path.GetTempPath();
		string processName = Process.GetCurrentProcess().ProcessName;
		StringBuilder stringBuilder = new StringBuilder();
		if (string.Equals(processName, "mmc", StringComparison.OrdinalIgnoreCase))
		{
			stringBuilder.Append("VMBrowser");
		}
		else if (string.Equals(processName, "vmconnect", StringComparison.OrdinalIgnoreCase))
		{
			stringBuilder.Append("VMConnect");
		}
		else if (string.Equals(processName, "InspectVHDDialog", StringComparison.OrdinalIgnoreCase))
		{
			stringBuilder.Append("VHDInspect");
		}
		else if (string.Equals(processName, "powershell", StringComparison.OrdinalIgnoreCase))
		{
			stringBuilder.Append("HyperVCmdlet");
		}
		else
		{
			stringBuilder.Append("HyperV_");
			stringBuilder.Append(processName);
		}
		stringBuilder.Append("_Trace_");
		stringBuilder.Append(DateTime.Now.ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture));
		stringBuilder.Append(".log");
		return Path.Combine(tempPath, stringBuilder.ToString());
	}
}
