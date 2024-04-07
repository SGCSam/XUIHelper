using CommandLine;
using System.Reflection;

namespace XUIHelper.CLI
{
    [Verb("about", HelpText = "Displays information about XUIHelper.CLI.")]
    public class AboutOptions : OptionsBase
    {
        public AboutOptions()
        {

        }

        public override Task HandleAsync()
        {
            Console.WriteLine("========================");
            Console.WriteLine("XUIHelper.CLI {0} by SGCSam", Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("XUIHelper.CLI is a command line interface to the XUIHelper.Core library.");
            Console.WriteLine("The library provides an entire suite of functions for interfacing with XUI and XUR files, the file formats used for the Xbox 360's UI implementation.");
            Console.WriteLine("All associated assets and copyrights belong to Microsoft. No ownership of any content is claimed.");
            Console.WriteLine("========================");
            Console.WriteLine();
            return Task.CompletedTask;
        }
    }
}
