using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using ICSharpCode.Decompiler;

namespace LinuxCreatorCompanion;
public static class CreatorCompanionExtractor
{
    public static void Extract(string path)
    {
        if (!Directory.Exists(path))
        {
            Console.WriteLine($"{path} does not exist!");
            Environment.Exit(1);
        }

        var packageFileName = Path.Combine(path, "CreatorCompanion.exe");
        if (!File.Exists(packageFileName))
        {
            Console.WriteLine($"{packageFileName} does not exist!");
            Environment.Exit(1);
        }

        // https://github.com/icsharpcode/ILSpy/blob/master/ICSharpCode.ILSpyCmd/IlspyCmdProgram.cs#L360C14-L360C15
        using (var memoryMappedPackage =
               MemoryMappedFile.CreateFromFile(packageFileName, FileMode.Open, null, 0, MemoryMappedFileAccess.Read))
        {
            using (MemoryMappedViewAccessor packageView = memoryMappedPackage.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read))
            {
                if (!SingleFileBundle.IsBundle(packageView, out long bundleHeaderOffset))
                {
                    Console.WriteLine(
                        $"Cannot dump assemblies for {packageFileName}, because it is not a single file bundle.");
                    Environment.Exit(-1);
                }

                var manifest = SingleFileBundle.ReadManifest(packageView, bundleHeaderOffset);
                foreach (var entry in manifest.Entries)
                {
                    //get file name
                    var name = entry.RelativePath;
                    if (name == null)
                        continue;

                    if (name != "CreatorCompanion.dll" && name != "vcc-lib.dll" && name != "vpm-core-lib.dll")
                        continue;

                    Stream contents;

                    if (entry.RelativePath.Replace('\\', '/').Contains("../", StringComparison.Ordinal) ||
                        Path.IsPathRooted(entry.RelativePath))
                    {
                        Console.WriteLine(
                            $"Skipping single-file entry '{entry.RelativePath}' because it might refer to a location outside of the bundle output directory.");
                        continue;
                    }

                    if (entry.CompressedSize == 0)
                    {
                        contents = new UnmanagedMemoryStream(packageView.SafeMemoryMappedViewHandle, entry.Offset,
                            entry.Size);
                    }
                    else
                    {
                        Stream compressedStream = new UnmanagedMemoryStream(packageView.SafeMemoryMappedViewHandle,
                            entry.Offset, entry.CompressedSize);
                        Stream decompressedStream = new MemoryStream((int)entry.Size);
                        using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                        {
                            deflateStream.CopyTo(decompressedStream);
                        }

                        if (decompressedStream.Length != entry.Size)
                        {
                            Console.WriteLine(
                                $"Corrupted single-file entry '{entry.RelativePath}'. Declared decompressed size '{entry.Size}' is not the same as actual decompressed size '{decompressedStream.Length}'.");
                            Environment.Exit(-1);
                        }

                        decompressedStream.Seek(0, SeekOrigin.Begin);
                        contents = decompressedStream;
                    }

                    string target = Path.Combine(Directory.GetCurrentDirectory(), entry.RelativePath);
                    using FileStream fileStream = File.Create(target);
                    contents.CopyTo(fileStream);
                }
            }
        }

        var templates = Path.Combine(path, "Templates");
        var tools = Path.Combine(path, "Tools");
        var webapp = Path.Combine(path, "WebApp");

        if (!Directory.Exists(templates) || !Directory.Exists(tools) || !Directory.Exists(webapp))
        {
            Console.WriteLine("Templates, Tools, or WebApp does not exist!");
            Environment.Exit(1);
        }

        //copy those folders recursively
        CopyFolder(templates, Path.Combine(Directory.GetCurrentDirectory(), "Templates"));
        CopyFolder(tools, Path.Combine(Directory.GetCurrentDirectory(), "Tools"));
        CopyFolder(webapp, Path.Combine(Directory.GetCurrentDirectory(), "WebApp"));

        Environment.Exit(0);
    }
    
    private static void CopyFolder(string input, string output)
    {
        if (!Directory.Exists(output))
            Directory.CreateDirectory(output);

        foreach (var file in Directory.GetFiles(input))
            File.Copy(file, Path.Combine(output, Path.GetFileName(file)));

        foreach (var folder in Directory.GetDirectories(input))
            CopyFolder(folder, Path.Combine(output, Path.GetFileName(folder)));
    }
}