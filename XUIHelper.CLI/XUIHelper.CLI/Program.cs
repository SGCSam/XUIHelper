using CommandLine;
using Microsoft.Win32;
using System.Reflection;

namespace XUIHelper.CLI
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Type[] types = { typeof(AboutOptions), typeof(RegisterXMLExtensionOptions), typeof(SetCurrentXMLExtensionsGroupOptions) };
            await Parser.Default.ParseArguments(args, types).WithParsedAsync(HandleArguments);
        }

        private static async Task HandleArguments(object obj)
        {
            if(obj is OptionsBase o)
            {
                await o.HandleAsync();
            }
        }
    }
}
