using Serilog.Events;
using Serilog;
using XUIHelper.Core;
using System.Formats.Tar;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.IO;

namespace XUIHelper.Tests
{
    public class XUR12Tests
    {
        private ILogger _Log;

        [SetUp]
        public void Setup()
        {
            string logPath = Path.Combine(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug", string.Format("Tests Log {0}.log", DateTime.Now.ToString("yyyy - MM - dd HHmmss")));
            var outputTemplate = "({Timestamp:HH:mm:ss.fff}) {Level}: [{LineNumber}]{SourceContext}::{MemberName} - {Message}{NewLine}";
            _Log = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.File(logPath, LogEventLevel.Verbose, outputTemplate)
            .CreateLogger();

            RegisterExtensions(_Log);
        }

        private void RegisterExtensions(ILogger? logger = null)
        {
            XMLExtensionsManager v5Extensions = new XMLExtensionsManager(logger);
            _ = v5Extensions.TryRegisterXMLExtensionsAsync(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V5\XuiElements.xml");
            _ = v5Extensions.TryRegisterXMLExtensionsAsync(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V5\9199DashElements.xml");
            _ = v5Extensions.TryRegisterXMLExtensionsAsync(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V5\9199HUDElements.xml");
            XUIHelperCoreConstants.VersionedExtensions[0x5] = v5Extensions;
        }

        private async Task<XUI12?> GetReadXUI(string filePath, int extensionVersion, ILogger? logger = null)
        {
            XUI12 xui = new XUI12(filePath, logger);
            if (await xui.TryReadAsync(0x5))
            {
                return xui;
            }

            return null;
        }

        private bool AreFilesEqual(string filePathOne, string filePathTwo)
        {
            if (!File.Exists(filePathOne))
            {
                return false;
            }

            if (!File.Exists(filePathTwo))
            {
                return false;
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] fileOneHash;
                byte[] fileTwoHash;

                using (FileStream fileOneStream = File.OpenRead(filePathOne))
                {
                    fileOneHash = sha256.ComputeHash(fileOneStream);
                }

                using (FileStream fileTwoStream = File.OpenRead(filePathTwo))
                {
                    fileTwoHash = sha256.ComputeHash(fileTwoStream);
                }

                return fileOneHash.SequenceEqual(fileTwoHash);
            }
        }

        [Test]
        public async Task CheckAllReadsSuccessful()
        {
            List<string> successfulXUIs = new List<string>();
            List<string> failedXUIs = new List<string>();

            int xuisCount = 0;
            foreach (string xuiFile in Directory.GetFiles(Path.Combine(TestContext.CurrentContext.TestDirectory, "Test Data/XUI/9199"), "*.xui", SearchOption.AllDirectories))
            {
                XUI12 xui = new XUI12(xuiFile, null);
                if (!await xui.TryReadAsync(0x5))
                {
                    failedXUIs.Add(xuiFile);
                }
                else
                {
                    successfulXUIs.Add(xuiFile);
                }

                xuisCount++;
            }

            int totalXUIsCount = successfulXUIs.Count + failedXUIs.Count;
            float successPercentage = (successfulXUIs.Count / (float)totalXUIsCount) * 100.0f;

            _Log.Information("==== XUI12 ALL READS ====");
            _Log.Information("Total: {0}, Successful: {1}, Failed: {2} ({3}%)", totalXUIsCount, successfulXUIs.Count, failedXUIs.Count, successPercentage);
            _Log.Information("");
            _Log.Information("==== SUCCESSFUL XUIS ====");
            _Log.Information(string.Join("\n", successfulXUIs));
            _Log.Information("");
            _Log.Information("==== FAILED XUIS ====");
            _Log.Information(string.Join("\n", failedXUIs));
            _Log.Information("");

            Assert.True(failedXUIs.Count == 0);
        }

        [Test]
        public async Task CheckSingleXUIReadSuccessful()
        {
            string xuiFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Test Data/XUI/9199/EditorSkin.xui");
            XUI12 xui = new XUI12(xuiFile, _Log);
            Assert.True(await xui.TryReadAsync(0x5));
        }
    }
}