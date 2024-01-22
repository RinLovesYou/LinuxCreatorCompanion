using System.Reflection;
using HarmonyLib;

namespace LinuxCreatorCompanion;

public static class Entrypoint
{
    public static Harmony HarmonyInstance = new Harmony("LinuxCreatorCompanion");

    public static void Run(string[] args)
    {
        HarmonyInstance.PatchAll();
        typeof(VCCApp.Program).GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic)
            ?.Invoke(null, new object[] { args });
    }
}