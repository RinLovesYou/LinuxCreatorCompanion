using CommandLine;
using LinuxCreatorCompanion.Setup;

namespace LinuxCreatorCompanion;

public class Program
{
    public class Options
    {
        [Option('s', "setup", Required = false, HelpText = "Setup the LinuxCreatorCompanion, give it the path to the Windows Version of VRChat's Creator Companion")]
        public string? SetupPath { get; set; }
    }

    [STAThread]
    private static async Task Main(string[] args)
    {
        // AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
        // {
        //     var exception = (Exception) eventArgs.ExceptionObject;
        //     Console.WriteLine(exception);
        //     Console.WriteLine(exception.StackTrace);
        // };
        // Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
        // {
        //     if (o.SetupPath != null)
        //         CreatorCompanionExtractor.Extract(o.SetupPath!);
        // });

        await FileChecks.VccFileChecks();

        Entrypoint.Run(args);
    }
}
