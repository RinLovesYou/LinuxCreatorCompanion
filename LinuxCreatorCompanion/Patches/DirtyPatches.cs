using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using VRC.CreatorCompanion.Core;
using VRC.PackageManagement;

namespace LinuxCreatorCompanion.Patches;

[HarmonyPatch(typeof(UnityEditorList), nameof(UnityEditorList.GetUnityExePathFromEditorBase))]
public static class GetUnityExePathFromEditorBasePatch
{
    public static bool Prefix(string baseDir, ref string __result)
    {
        __result = Path.Combine(baseDir, "Editor", "Unity");
        return false;
    }
}

[HarmonyPatch(typeof(UnityEditorList), nameof(UnityEditorList.TryGetVersionFromPath))]
public static class TryGetVersionFromPathPatch
{
    public static bool Prefix(string path, out string version, ref bool __result)
    {
        var parent = Directory.GetParent(path);
        if (parent.Name != "Editor")
        {
            VRCLibLogger.LogError($"[Rin's Funnies] Parent {parent.Name} is not Editor");
            version = "";
            __result = false;
            return false;
        }
        
        var productVersion = parent.Parent!.Name;
        
        version = productVersion;
        __result = true;
        return false;
    }
}

[HarmonyPatch(typeof(UnityEditorList), nameof(UnityEditorList.GetUnityHubPath))]
public static class GetUnityHubPathPatch
{
    public static bool Prefix(ref string __result)
    {
        var process = new Process
        {
            StartInfo =
            {
                FileName = "which",
                Arguments = "unityhub",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        process.WaitForExit();
        
        var output = process.StandardOutput.ReadToEnd();
        
        if (string.IsNullOrWhiteSpace(output))
        {
            __result = "";
            return false;
        }
        
        __result = output.Trim();
        
        VRCLibLogger.LogInfo($"[Rin's Funnies] Unity Hub Path: {__result}");
        return false;
    }
}

[HarmonyPatch(typeof(SemanticVersioning.Range), nameof(SemanticVersioning.Range.TryParse),
    new Type[] { typeof(string), typeof(SemanticVersioning.Range) },
    new ArgumentType[] { ArgumentType.Normal, ArgumentType.Out })]
public static class TryParsePatch
{
    public static bool Prefix(string rangeSpec, out SemanticVersioning.Range result, out bool __result)
    {
        if (rangeSpec is null)
        {
            __result = false;
            result = null;
            return false;
        }

        __result = SemanticVersioning.Range.TryParse(rangeSpec, false, out result);
        return false;
    }
}

//this lets VCC find the UnityHub Editor install folder
[HarmonyPatch(typeof(Environment), nameof(Environment.GetFolderPath), new Type[] { typeof(Environment.SpecialFolder) })]
public static class EnvironmentPatch
{
    public static bool Prefix(Environment.SpecialFolder folder, ref string __result)
    {
        if (folder != Environment.SpecialFolder.ProgramFiles)
            return true;
        
        //invoke the original method without triggering patch (trampoline)
        object? res = MethodInvoker
            .GetHandler(typeof(Environment).GetMethod(nameof(Environment.GetFolderPath),
                new Type[] { typeof(Environment.SpecialFolder) }))
            .Invoke(null, new object[] { Environment.SpecialFolder.UserProfile });
        
        __result = (string)res!;
        return false;
    }
}