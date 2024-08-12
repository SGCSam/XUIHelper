using CommandLine;
using Microsoft.Win32;
using System.Reflection;
using XUIHelper.Core;

namespace XUIHelper.CLI
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string extensionsDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Assets\Extensions");
            await XUIHelperAPI.RegisterExtensionsFromDirectoryAsync(extensionsDirPath);

            Type[] types = { typeof(AboutOptions), typeof(AboutOptions), typeof(ConvertOptions), typeof(MassConvertOptions) };
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
