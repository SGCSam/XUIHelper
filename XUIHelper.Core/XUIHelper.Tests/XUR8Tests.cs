using Serilog.Events;
using Serilog;
using XUIHelper.Core;

namespace XUIHelper.Tests
{
    public class XUR8Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        public async Task<bool> CheckReadSuccessful(string filePath, ILogger? logger = null)
        {
            XUR8 xur = new XUR8(filePath, logger);
            bool successful = await xur.TryReadAsync();
            return successful;
        }

        public void RegisterExtensions(ILogger? logger = null)
        {
            XMLExtensionsManager v8Extensions = new XMLExtensionsManager(logger);
            _ = v8Extensions.TryRegisterXMLExtensionsAsync(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V8\XuiElements.xml");
            _ = v8Extensions.TryRegisterXMLExtensionsAsync(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V8\17559DashElements.xml");
            XUIHelperCoreConstants.VersionedExtensions[0x8] = v8Extensions;
        }

        [Test]
        public async Task CheckGamerCardReadSuccessful()
        {
            string logPath = Path.Combine(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug", string.Format("Tests Log {0}.log", DateTime.Now.ToString("yyyy - MM - dd HHmmss")));
            var outputTemplate = "({Timestamp:HH:mm:ss.fff}) {Level}: [{LineNumber}]{SourceContext}::{MemberName} - {Message}{NewLine}";
            ILogger log = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.File(logPath, LogEventLevel.Verbose, outputTemplate)
            .CreateLogger();

            RegisterExtensions(log);

            Assert.True(await CheckReadSuccessful(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug\Example XURs\17559GamerCard.xur", log));
        }

        [Test]
        public async Task CheckSetClockTimeSuccessful()
        {
            string logPath = Path.Combine(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug", string.Format("Tests Log {0}.log", DateTime.Now.ToString("yyyy - MM - dd HHmmss")));
            var outputTemplate = "({Timestamp:HH:mm:ss.fff}) {Level}: [{LineNumber}]{SourceContext}::{MemberName} - {Message}{NewLine}";
            ILogger log = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.File(logPath, LogEventLevel.Verbose, outputTemplate)
            .CreateLogger();

            RegisterExtensions(log);

            Assert.True(await CheckReadSuccessful(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug\Example XURs\17559dashSysCslSetClockTime.xur", log));
        }

        [Test]
        public async Task CheckAllReadsSuccessful()
        {
            string logPath = Path.Combine(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug", string.Format("Tests Log {0}.log", DateTime.Now.ToString("yyyy - MM - dd HHmmss")));
            var outputTemplate = "({Timestamp:HH:mm:ss.fff}) {Level}: [{LineNumber}]{SourceContext}::{MemberName} - {Message}{NewLine}";
            ILogger log = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.File(logPath, LogEventLevel.Verbose, outputTemplate)
            .CreateLogger();

            RegisterExtensions(log);

            string workingPath = @"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug\Example XURs\17559 All\Working";
            string notWorkingPath = @"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug\Example XURs\17559 All\Not Working";

            foreach (string filePath in Directory.GetFiles(workingPath, "*.xur", SearchOption.TopDirectoryOnly))
            {
                File.Delete(filePath);
            }

            foreach (string filePath in Directory.GetFiles(notWorkingPath, "*.xur", SearchOption.TopDirectoryOnly))
            {
                File.Delete(filePath);
            }

            bool anyFailed = false;
            foreach (string xurFilePath in Directory.GetFiles(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug\Example XURs\17559 All", "*.xur"))
            {
                string fileName = Path.GetFileName(xurFilePath);
                if (!await CheckReadSuccessful(xurFilePath, log))
                {
                    anyFailed = true;
                    string destPath = Path.Combine(notWorkingPath, fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                    File.Copy(xurFilePath, destPath);
                }
                else
                {
                    string destPath = Path.Combine(workingPath, fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                    File.Copy(xurFilePath, destPath);
                }
            }

            Assert.False(anyFailed);
        }

        [Test]
        public async Task CheckNonWorkingSuccessful()
        {
            string logPath = Path.Combine(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug", string.Format("Tests Log {0}.log", DateTime.Now.ToString("yyyy - MM - dd HHmmss")));
            var outputTemplate = "({Timestamp:HH:mm:ss.fff}) {Level}: [{LineNumber}]{SourceContext}::{MemberName} - {Message}{NewLine}";
            ILogger log = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.File(logPath, LogEventLevel.Verbose, outputTemplate)
            .CreateLogger();

            RegisterExtensions(log);

            string rootPath = @"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug\Example XURs\17559 All\Not Working";
            string filePath = Path.Combine(rootPath, "ThermalPostScene.xur");

            Assert.True(await CheckReadSuccessful(filePath, log));
        }
    }
}