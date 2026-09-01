using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Xamarin.Android.BuildTools.PrepTasks
{
	public class ReplaceFileContents : Task
	{
		[Required]
		public  ITaskItem   SourceFile          { get; set; }

		[Required]
		public  ITaskItem   DestinationFile     { get; set; }

		public  string[]    Replacements        { get; set; }
		public  string      ReplacementFilePath { get; set; }

		public override bool Execute ()
		{
			Log.LogMessage (MessageImportance.Low, $"Task {nameof (ReplaceFileContents)}");
			Log.LogMessage (MessageImportance.Low, $"  {nameof (SourceFile)}: {SourceFile.ItemSpec}");
			Log.LogMessage (MessageImportance.Low, $"  {nameof (DestinationFile)}: {DestinationFile.ItemSpec}");
			Log.LogMessage (MessageImportance.Low, $"  {nameof (Replacements)}:");
			if (Replacements != null) {
				foreach (var replacement in Replacements) {
					Log.LogMessage (MessageImportance.Low, $"    {replacement}");
				}
			}

			try {
				// Validate source file exists
				if (!File.Exists (SourceFile.ItemSpec)) {
					Log.LogError ($"Source file not found: {SourceFile.ItemSpec}");
					return false;
				}

				// Delete destination if it exists
				if (File.Exists (DestinationFile.ItemSpec)) {
					try {
						File.Delete (DestinationFile.ItemSpec);
					} catch (Exception ex) {
						Log.LogError ($"Failed to delete destination file '{DestinationFile.ItemSpec}': {ex.Message}");
						return false;
					}
				}

				// Get replacement pairs
				string[] replacements;
				if (!String.IsNullOrEmpty (ReplacementFilePath)) {
					if (!File.Exists (ReplacementFilePath)) {
						Log.LogError ($"Replacement file not found: {ReplacementFilePath}");
						return false;
					}
					replacements = File.ReadAllLines (ReplacementFilePath);
				} else {
					replacements = Replacements;
				}

				var r = GetReplacementInfo (replacements);
				
				// Process file with error handling
				using (var i = File.OpenText (SourceFile.ItemSpec))
				using (var o = File.CreateText (DestinationFile.ItemSpec)) {
					string line;
					while ((line = i.ReadLine ()) != null) {
						foreach (var e in r) {
							line = line.Replace (e.Key, e.Value);
						}
						o.WriteLine (line);
					}
				}

				return !Log.HasLoggedErrors;
			} catch (Exception ex) {
				Log.LogError ($"Task {nameof (ReplaceFileContents)} failed: {ex.Message}");
				return false;
			}
		}

		static  readonly    char[]  Separator   = new [] { '=' };

		Dictionary<string, string> GetReplacementInfo (string[] replacements)
		{
			var r = new Dictionary<string, string> (replacements?.Length ?? 0);
			if (replacements == null || replacements.Length == 0)
				return r;

			foreach (var e in replacements) {
				if (string.IsNullOrEmpty (e))
					continue;

				var kvp = e.Split (Separator, 2, StringSplitOptions.RemoveEmptyEntries);
				
				// Validate we have a key
				if (kvp.Length == 0) {
					continue; // Skip malformed lines
				}

				string key = kvp;
				string value = kvp.Length > 1 ? kvp [1] : "";
				
				// Warn if key already exists (duplicate replacement)
				if (r.ContainsKey (key)) {
					Log.LogWarning ($"Duplicate replacement key: '{key}'");
					r [key] = value; // Override with latest
				} else {
					r.Add (key, value);
				}
			}
			return r;
		}
	}
}
