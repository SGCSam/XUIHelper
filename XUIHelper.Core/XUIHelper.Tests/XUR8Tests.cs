using Serilog.Events;
using Serilog;
using XUIHelper.Core;

namespace XUIHelper.Tests
{
    public class XUR8Tests : XURTests
    {
        //TODO: XUR8 write support
        //TODO: XUR8 unit tests
        //TODO: Decouple XUI version from extensions, make it more of a group IDed by a string, as we should be able to write a XUR8 as a XUR5
        //TODO: Support for ignore properties (ones that aren't supported in XuiTool)
        //TODO: Function library API
        //TODO: Console app
        //TODO: GUI app

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

        protected override void RegisterExtensions(ILogger? logger = null)
        {
            XMLExtensionsManager v8Extensions = new XMLExtensionsManager(logger);
            _ = v8Extensions.TryRegisterXMLExtensionsAsync(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V8\XuiElements.xml");
            _ = v8Extensions.TryRegisterXMLExtensionsAsync(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V8\17559DashElements.xml");
            XUIHelperCoreConstants.VersionedExtensions[0x8] = v8Extensions;
        }

        protected override IXUR GetXUR(string filePath, ILogger? logger = null)
        {
            return new XUR8(filePath, logger);
        }

        [Test]
        public async Task CheckAllReadsSuccessful()
        {
            Assert.True(await CheckAllReadsSuccessfulAsync(@"Test Data/XUR/17559"));
        }

        [Test]
        public async Task CheckSingleXURReadSuccessful()
        {
            Assert.True(await CheckSingleXURReadSuccessfulAsync(@"Test Data/XUR/17559/gamercard.xur"));
        }

        [Test]
        public async Task CheckAllWritesSuccessful()
        {
            Assert.True(await CheckAllWritesSuccessfulAsync(@"Test Data/XUR/17559"));
        }

        [Test]
        public async Task CheckSingleXURWriteSuccessful()
        {
            Assert.True(await CheckSingleXURWriteSuccessfulAsync(@"Test Data/XUR/17559/gamercard.xur"));
        }
    }
}