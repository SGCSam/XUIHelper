using Serilog.Events;
using Serilog;
using XUIHelper.Core;

namespace XUIHelper.Tests
{
    public class XUI12Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        public async Task<bool> CheckReadSuccessful(string filePath, ILogger? logger = null)
        {
            XUI12 xui = new XUI12(filePath, logger);
            return await xui.TryReadAsync(0x5);
        }

        public void RegisterExtensions(ILogger? logger = null)
        {
            XMLExtensionsManager v5Extensions = new XMLExtensionsManager(logger);
            _ = v5Extensions.TryRegisterXMLExtensionsAsync(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V5\XuiElements.xml");
            _ = v5Extensions.TryRegisterXMLExtensionsAsync(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V5\9199DashElements.xml");
            XUIHelperCoreConstants.VersionedExtensions[0x5] = v5Extensions;
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

            Assert.True(await CheckReadSuccessful(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug\Example XUIs\9199GamerCard.xui", log));
        }

        [Test]
        public async Task CheckSimpleSceneReadSuccessful()
        {
            string logPath = Path.Combine(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug", string.Format("Tests Log {0}.log", DateTime.Now.ToString("yyyy - MM - dd HHmmss")));
            var outputTemplate = "({Timestamp:HH:mm:ss.fff}) {Level}: [{LineNumber}]{SourceContext}::{MemberName} - {Message}{NewLine}";
            ILogger log = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.File(logPath, LogEventLevel.Verbose, outputTemplate)
            .CreateLogger();

            RegisterExtensions(log);

            Assert.True(await CheckReadSuccessful(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug\Example XUIs\SimpleScene - Formatted.xui", log));
        }
    }
}