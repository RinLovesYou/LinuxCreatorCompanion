namespace LinuxCreatorCompanion.Setup;

public static class FileChecks
{
    public static List<string> VCCFiles = new()
    {
        "CreatorCompanion.dll",
        "vcc-lib.dll",
        "vpm-core-lib.dll"
    };

    public static List<string> VCCFolders = new()
    {
        "WebApp",
        "Tools",
        "Templates"
    };
    
    public static void VCCFileChecks()
    {
        bool foldersExist = VCCFolders.Select((s) => Path.Combine(Directory.GetCurrentDirectory(), s)).All(Directory.Exists);
        bool filesExist = VCCFiles.Select((s) => Path.Combine(Directory.GetCurrentDirectory(), s)).All(File.Exists);

        if (foldersExist && filesExist)
        {
            Console.WriteLine("VCC Files and Folders exist, skipping download.");
            return;
        }
        
        VccDownloader.DownloadVcc();
    }
}
