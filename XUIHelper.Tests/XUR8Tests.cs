using Serilog.Events;
using Serilog;
using XUIHelper.Core;
using System.IO;

namespace XUIHelper.Tests
{
    public class XUR8Tests : XURTests
    {
        //TODO: Ignore properties for XUR writing
        //TODO: Make sure all read/write property functions for XUR5 and XUR8 natively support indexed properties, rather than assuming they're animated NumStops - do a check for all uses of Indexed
        //TODO: Additional XUI extensions for dashes
        //TODO: XUR5 count header write criteria

        [SetUp]
        public void Setup()
        {
            string logPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Debug", string.Format("Tests Log {0}.log", DateTime.Now.ToString("yyyy - MM - dd HHmmss")));
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
            XMLExtensionsManager.Initialize(logger);
            _ = XMLExtensionsManager.TryRegisterExtensionsGroupAsync("XUR8Tests", Path.Combine(TestContext.CurrentContext.TestDirectory, @"Assets\Extensions\V8\17559.xhe"));
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
            Assert.True(await CheckSingleXURReadSuccessfulAsync(@"Test Data/XUR/17559/community.xur"));
        }

        [Test]
        public async Task CheckAllWritesSuccessful()
        {
            Assert.True(await CheckAllWritesSuccessfulAsync(@"Test Data/XUR/17559"));
        }

        [Test]
        public async Task CheckSingleXURWriteSuccessful()
        {
            Assert.True(await CheckSingleXURWriteSuccessfulAsync(@"Test Data/XUR/17559/LegendScene.xur"));
        }
    }
}