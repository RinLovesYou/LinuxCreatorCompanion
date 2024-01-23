using System.Diagnostics;
using System.Net;

namespace LinuxCreatorCompanion.Setup;

public static class VCCDownloader
{
    public static void DownloadVCC()
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri("https://vrcpm.vrchat.cloud/vcc/Builds/2.2.3/VRChat_CreatorCompanion_Setup_2.2.3.exe");
        client.DefaultRequestHeaders.Host = "vrcpm.vrchat.cloud";
        client.DefaultRequestHeaders.UserAgent.Clear();
        client.DefaultRequestHeaders.Add("User-Agent", "LinuxCreatorCompanion");
        client.DefaultRequestHeaders.Add("Accept", "application/octet-stream");
        
        var response = client.GetAsync(client.BaseAddress).Result;
        
        if (!response.IsSuccessStatusCode)
            throw new Exception($"VCC download failed with code {response.StatusCode}");

        var tempPath = Path.Combine(Directory.GetCurrentDirectory(), "tmp");
        Directory.CreateDirectory(tempPath);
        
        var vccPath = Path.Combine(tempPath, "vcc.exe");
        using (var fileStream = File.Create(vccPath))
        {
            response.Content.CopyToAsync(fileStream).Wait();
        }
        
        if (!WineCheck())
            throw new Exception("Wine is not installed!");
        
        var innounp = Path.Combine(Directory.GetCurrentDirectory(), "BaseLibs", "innounp.exe");
        
        var process = new Process
        {
            StartInfo =
            {
                FileName = "wine",
                Arguments = $"\"{innounp}\" -x \"{vccPath}\" -d\"{tempPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        process.WaitForExit();
        
        if (process.ExitCode != 0)
            throw new Exception($"Could not extract VCC Files {process.ExitCode}");
        
        var vccFolder = Path.Combine(tempPath, "{app}");
        
        CreatorCompanionExtractor.Extract(vccFolder);
        
        Directory.Delete(tempPath, true);
    }

    public static bool WineCheck()
    {
        var process = new Process
        {
            StartInfo =
            {
                FileName = "which",
                Arguments = "wine",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        process.WaitForExit();
        
        return process.ExitCode == 0;
    }
}