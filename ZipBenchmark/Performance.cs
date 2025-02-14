using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;
using Xamarin.Tools.Zip;
using SIOC = System.IO.Compression;

[MemoryDiagnoser]
public class Performance
{

    static byte [] data = [];
    static Dictionary<string, byte []> files = new Dictionary<string, byte []> ();

    static Stream? sicZip = null;
    static Stream? libZip = null;

    [GlobalSetup]
    public void Setup ()
    {
        Random r = new Random (234234);
        for (int j = 0; j < 100; j++) {
            data = new byte [r.Next (100, 1024 * 1000)];
            for (int i = 0; i < data.Length; i++)
                data [i] = (byte)r.Next (0, 128);// Add a bunch of ascii characters.
            files.Add ($"file-{j}.dat", data);
        }
        sicZip = new MemoryStream ();
        using (var zip = new SIOC.ZipArchive (sicZip, SIOC.ZipArchiveMode.Create, leaveOpen: true)) {
            foreach (var file in files) {
                var entry = zip.CreateEntry (file.Key);
                using (var writer = new StreamWriter (entry.Open ())) {
                    writer.Write (file.Value);
                }
            }
        }
        libZip = new MemoryStream ();
        using (var zip = ZipArchive.Create (libZip, strictConsistencyChecks: false)) {
            foreach (var file in files) {
                zip.AddEntry (file.Value, file.Key);
            }
            zip.Close ();
        }
    }

    [Benchmark]
    public void SystemIOCompressionCreateZip ()
    {
        using var stream = new MemoryStream ();
        using (var zip = new SIOC.ZipArchive (stream, SIOC.ZipArchiveMode.Create)) {
            foreach (var file in files) {
                var entry = zip.CreateEntry (file.Key, SIOC.CompressionLevel.SmallestSize);
                using (var writer = new StreamWriter (entry.Open ())) {
                    writer.Write (file.Value);
                }
            }
        }
    }

    [Benchmark]
    public void LibZipSharpCreateZip ()
    {
        using var stream = new MemoryStream ();
        using (var zip = ZipArchive.Create (stream, strictConsistencyChecks: false, useTempFile: false)) {
            foreach (var file in files) {
                zip.AddEntry (file.Value, file.Key);
            }
            zip.Close ();
        }
    }

    [Benchmark]
    public void SystemIOCompressionExtractToMemory ()
    {
        using var ms = new MemoryStream ();
        sicZip!.Position = 0;
        using (var zi = new SIOC.ZipArchive (sicZip, SIOC.ZipArchiveMode.Read, leaveOpen: true)) {
            foreach (var entry in zi.Entries) {
                ms.Position= 0;
                entry.Open ().CopyTo (ms);
            }
        }
    }

    [Benchmark]
    public void LibZipSharpExtractToMemory ()
    {
        using var ms = new MemoryStream ();
        libZip!.Position = 0;
        using (var zi = ZipArchive.Open (libZip, useTempFile: false)) {
            foreach (var entry in zi) {
                ms.Position= 0;
                entry.Extract (ms);
            }
        }
    }
}