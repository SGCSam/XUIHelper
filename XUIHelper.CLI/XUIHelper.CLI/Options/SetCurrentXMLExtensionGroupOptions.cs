using CommandLine;
using XUIHelper.Core;

namespace XUIHelper.CLI
{
    [Verb("setcurextgroup", HelpText = "Sets the current XML extensions group to use.")]
    public class SetCurrentXMLExtensionsGroupOptions : OptionsBase
    {
        [Option('g', "groupname", Required = true)]
        public string GroupName { get; set; } = string.Empty;

        public SetCurrentXMLExtensionsGroupOptions()
        {

        }

        public override Task HandleAsync()
        {
            XUIHelperAPI.SetCurrentExtensionsGroup(GroupName);
            Console.WriteLine("SUCCESS: Set the current XML extensions group to \"{0}\" successfully!", GroupName);
            return Task.CompletedTask;
        }
    }
}
