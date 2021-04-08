# LibZipSharp
A managed wrapper (and then some) around libzip (https://libzip.org/)

[![Build Status](https://devdiv.visualstudio.com/DevDiv/_apis/build/status/xamarin.LibZipSharp?repoName=xamarin%2FLibZipSharp&branchName=main)](https://devdiv.visualstudio.com/DevDiv/_build/latest?definitionId=11678&repoName=xamarin%2FLibZipSharp&branchName=main)


The core of `LibZipSharp` is the `ZipArchive` class. You can use this class
to create/extract/update zip file. 

## Create a new Zip

To create a new archive use the `FileMode.CreateNew`, this will behave
exactly as it does with normal `File` operations. An exception will be
thrown if the file already exists. 

```
using (var zip = ZipArchive.Open ("test.zip", FileMode.CreateNew)) {
}
```

## Open am existing Zip

To open an existing zip file use `FileMode.Open`.

```
using (var zip = ZipArchive.Open ("test.zip", FileMode.Open)) {
}
```

## Add files to Zip

There are a number of methods which can be used to add items to 
the zip file. The simplest is `AddFile`. This takes a file path.
If filename is an absolute path, it will be converted to a relative
one by removing the root of the path (i.e. the leading `/` part on 
Unix systems and the `x:\\` part on Windows). You can also pass an 
`archivePath` parameter where you can specify the name/path which file
will have within the archive. 

```
using (var zip = ZipArchive.Open ("test.zip", FileMode.CreateNew)) {
    zip.AddFile ("somefile.txt");
}
```

You can also add data directly from a `MemoryStream` via the `AddEntry`
method. This takes an `entryName` and a `MemoryStream`. Note the `MemoryStream`
will be disposed of when the zip file is finally written to disk. 

```
var ms = new MemoryStream ();
// write data to stream.
using (var zip = ZipArchive.Open ("test.zip", FileMode.CreateNew)) {
    zip.AddEntry ("foo", ms);
}
```

You can also add text directly via the `AddEntry` method. This method 
takes an `entryName` and `text` parameters. The `entryName` defines 
what the item in the zip file will be called. The `text` defines the contents
on the entry. There is also an `encoding` parameter where you can define
the `Encoding` of the file. 

```
using (var zip = ZipArchive.Open ("test.zip", FileMode.CreateNew)) {
    zip.AddEntry ("foo", "contents of the file", Encoding.UTF8);
}
```

# Shipping the native libraries.

By default the native libraries will NOT be copied into the output directory
of your app. `.Net Core` apps will pick these files up automatically. However
for Mono you will need the `libzip.*` files in the same directory as the 
final app for this library to work. Setting the `LibZipSharpBundleAllNativeLibraries`
MSBuild property to `true` will make sure the native libraries for 
ALL supported platforms are copied to the output directory.

You can do this via the command line

```
/p:LibZipSharpBundleAllNativeLibraries=True
```

or by adding the following to you `csproj`.

```
<LibZipSharpBundleAllNativeLibraries>true</LibZipSharpBundleAllNativeLibraries>
```
