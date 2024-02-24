using System.Diagnostics;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;

namespace LinuxCreatorCompanion.Setup;

public static class VccDownloader
{
    public static async void DownloadVcc()
    {
        var latestVersion = await FetchLatestVersionInfo();

        using var client = new HttpClient();
        client.BaseAddress = new Uri(latestVersion.Url);
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
        await using (var fileStream = File.Create(vccPath))
        {
            await response.Content.CopyToAsync(fileStream);
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
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new Exception($"Could not extract VCC Files {process.ExitCode}");

        var vccFolder = Path.Combine(tempPath, "{app}");

        CreatorCompanionExtractor.Extract(vccFolder);

        Directory.Delete(tempPath, true);
    }

    private static async Task<VccVersionInfo> FetchLatestVersionInfo()
    {
        using var client = new HttpClient();
        client.BaseAddress = new Uri("https://api.vrchat.cloud/api/1/config");
        client.DefaultRequestHeaders.Host = "api.vrchat.cloud";
        client.DefaultRequestHeaders.UserAgent.Clear();
        client.DefaultRequestHeaders.Add("User-Agent", "LinuxCreatorCompanion");
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        const string exceptionMessagePrefix = "VCC online version check failed because";

        var response = client.GetAsync(client.BaseAddress).Result;

        if (!response.IsSuccessStatusCode)
            throw new Exception($"{exceptionMessagePrefix} the HTTP response code is {response.StatusCode}");

        var jsonString = await response.Content.ReadAsStringAsync();

        dynamic? jsonObject;
        try
        {
            jsonObject = JsonConvert.DeserializeObject(jsonString);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"{exceptionMessagePrefix} the response was not valid JSON", ex);
        }

        if (jsonObject == null)
        {
            throw new InvalidOperationException($"{exceptionMessagePrefix} the JSON parsed as null");
        }

        string? downloadUrlVcc;

        try
        {
            downloadUrlVcc = jsonObject.downloadUrls?.vcc;
        }
        catch (RuntimeBinderException ex)
        {
            throw new InvalidOperationException(
                $"{exceptionMessagePrefix} the JSON properties could not be accessed", ex);
        }

        if (downloadUrlVcc == null)
        {
            throw new InvalidOperationException(
                $"{exceptionMessagePrefix} the JSON did not contain the required properties");
        }

        return new VccVersionInfo(downloadUrlVcc);
    }

    private static bool WineCheck()
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

public struct VccVersionInfo
{
    public string Url { get; }

    public VccVersionInfo(string url) {
        Url = url;
    }
}
