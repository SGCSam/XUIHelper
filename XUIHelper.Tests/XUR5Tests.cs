using Serilog.Events;
using Serilog;
using XUIHelper.Core;
using System.Formats.Tar;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.IO;

namespace XUIHelper.Tests
{
    public class XUR5Tests : XURTests
    {
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
            _ = XMLExtensionsManager.TryRegisterExtensionsGroupAsync("XUR5Tests", Path.Combine(TestContext.CurrentContext.TestDirectory, @"Extensions\V5\9199.xhe"));
        }

        protected override IXUR GetXUR(string filePath, ILogger? logger = null)
        {
            return new XUR5(filePath, logger);
        }

        [Test]
        public async Task CheckAllReadsSuccessful()
        {
            Assert.True(await CheckAllReadsSuccessfulAsync(@"Test Data/XUR/9199"));
        }

        [Test]
        public async Task CheckSingleXURReadSuccessful()
        {
            Assert.True(await CheckSingleXURReadSuccessfulAsync(@"Test Data/XUR/9199/GamerCard.xur"));
        }

        [Test]
        public async Task CheckAllWritesSuccessful()
        {
            Assert.True(await CheckAllWritesSuccessfulAsync(@"Test Data/XUR/9199"));
        }

        [Test]
        public async Task CheckSingleXURWriteSuccessful()
        {
            Assert.True(await CheckSingleXURWriteSuccessfulAsync(@"Test Data/XUR/9199/GamerCard.xur"));
        }
    }
}