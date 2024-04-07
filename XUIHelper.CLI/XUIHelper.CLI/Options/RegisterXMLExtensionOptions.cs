using CommandLine;
using XUIHelper.Core;

namespace XUIHelper.CLI
{
    [Verb("regext", HelpText = "Registers XML extensions from the specified file path.")]
    public class RegisterXMLExtensionOptions : OptionsBase
    {
        [Option('g', "groupname", Required = true)]
        public string GroupName { get; set; } = string.Empty;

        [Option('f', "filepath", Required = true)]
        public string ExtensionsFilePath { get; set; } = string.Empty;

        public RegisterXMLExtensionOptions() 
        { 
        
        }

        public override async Task HandleAsync()
        {
            if (!await XUIHelperAPI.TryRegisterExtensionsGroupAsync(GroupName, ExtensionsFilePath))
            {
                Console.WriteLine("ERROR: Failed to register XML extensions. Consider using a log file and checking it for more information.");
            }
            else
            {
                Console.WriteLine("SUCCESS: XML extensions registered successfully!");
            }
        }
    }
}
