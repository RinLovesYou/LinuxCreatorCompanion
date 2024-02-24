using System.Reflection;
using HarmonyLib;

namespace LinuxCreatorCompanion;

public static class Entrypoint
{
    public static Harmony HarmonyInstance = new("LinuxCreatorCompanion");

    public static void Run(string[] args)
    {
        Console.WriteLine("Harmony patching VCC");
        HarmonyInstance.PatchAll();
        Console.WriteLine("Invoking Main");
        var mainMethodInfo = typeof(VCCApp.Program).GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic);
        // this errors out with:
        //    terminate called after throwing an instance of 'PAL_SEHException'
        // seemingly right as soon as the VCC API server class is instantiated.
        mainMethodInfo?.Invoke(null, new object[] { args });
    }
}
