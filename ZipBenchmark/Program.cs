using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

#if UNIX
using Mono.Unix.Native;
#endif
using Xamarin.Tools.Zip;

namespace ZipBenchmark;

public class LargeFileCompression
{
	// Fugly...
	public static string? InputFilePath;

	[Benchmark]
	[ArgumentsSource (nameof(Paths))]
	public void Compress (string inputFile)
	{
		string outputPath = $"{Path.GetFileName (inputFile)}.out.zip";
		using (var zip = ZipArchive.Open (outputPath, FileMode.Create)) {
			zip.AddFile (inputFile);
			zip.Close ();
		}
	}

	public IEnumerable<string> Paths ()
	{
		if (String.IsNullOrEmpty (InputFilePath)) {
			throw new InvalidOperationException ("Input file path must be specified");
		}

		yield return InputFilePath;
	}
}

class App
{
	public static int Main (string [] args)
        {
		if (args.Length == 0) {
			Console.WriteLine ($"Usage: ZipBenchmark path/to/a/large/file/to/compress");
			Console.WriteLine ();
			return 1;
		}

		LargeFileCompression.InputFilePath = args[0];
		var summary = BenchmarkRunner.Run<LargeFileCompression> ();
		return 0;
	}
}
