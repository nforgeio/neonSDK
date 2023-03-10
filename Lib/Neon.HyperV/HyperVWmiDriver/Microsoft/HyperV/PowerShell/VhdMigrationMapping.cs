using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.HyperV.PowerShell.Common;

namespace Microsoft.HyperV.PowerShell;

internal class VhdMigrationMapping
{
	private const string SourceKey = "SourceFilePath";

	private const string DestinationKey = "DestinationFilePath";

	internal string SourcePath { get; private set; }

	internal string DestinationPath { get; private set; }

	internal VhdMigrationMapping(string sourcePath, string destinationPath)
	{
		SourcePath = sourcePath;
		DestinationPath = destinationPath;
	}

	internal static IReadOnlyList<VhdMigrationMapping> CreateMappingsFromHashtable(Hashtable[] hashtables)
	{
		List<VhdMigrationMapping> list = new List<VhdMigrationMapping>();
		foreach (Hashtable obj in hashtables)
		{
			string text = null;
			string text2 = null;
			foreach (DictionaryEntry item in obj)
			{
				if (!(item.Key is string value))
				{
					throw ExceptionHelper.CreateInvalidArgumentException(ErrorMessages.InvalidParameter_HashtableKeyIsNotString);
				}
				if ("SourceFilePath".StartsWith(value, StringComparison.OrdinalIgnoreCase))
				{
					text = item.Value as string;
					continue;
				}
				if ("DestinationFilePath".StartsWith(value, StringComparison.OrdinalIgnoreCase))
				{
					text2 = item.Value as string;
					continue;
				}
				throw ExceptionHelper.CreateInvalidArgumentException(ErrorMessages.InvalidParameter_HashContainsInvalidKeys);
			}
			if (text == null)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(ErrorMessages.InvalidParameter_HashDoesNotContainSource);
			}
			if (text2 == null)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(ErrorMessages.InvalidParameter_HashDoesNotContainDestination);
			}
			list.Add(new VhdMigrationMapping(text, text2));
		}
		return list.AsReadOnly();
	}
}
