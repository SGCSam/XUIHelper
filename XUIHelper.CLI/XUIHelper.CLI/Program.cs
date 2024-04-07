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
            await RegisterExtensionsAsync();

            Type[] types = { typeof(AboutOptions), typeof(AboutOptions), typeof(ConvertOptions) };
            await Parser.Default.ParseArguments(args, types).WithParsedAsync(HandleArguments);
        }

        private static async Task RegisterExtensionsAsync()
        {
            string extensionsDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Extensions");

            foreach (string subDir in Directory.GetDirectories(extensionsDirPath, "*", SearchOption.TopDirectoryOnly))
            {
                string groupName = new DirectoryInfo(subDir).Name;

                foreach (string extensionXML in Directory.GetFiles(subDir, "*.xml", SearchOption.TopDirectoryOnly))
                {
                    if (!await XUIHelperAPI.TryRegisterExtensionsGroupAsync(groupName, extensionXML))
                    {
                        Console.WriteLine("WARNING: Failed to register XML extension at {0}.", extensionXML);
                    }
                    else
                    {
                        Console.WriteLine("INFO: Registered XML extensions from {0}.", extensionXML);
                    }
                }
            }
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
