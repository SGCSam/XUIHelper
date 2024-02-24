using Serilog.Events;
using Serilog;
using XUIHelper.Core;

namespace XUIHelper.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        public async void CheckReadSuccessful(string filePath, ILogger? logger = null)
        {
            XUR5 xur = new XUR5(filePath, logger);
            Assert.True(await xur.TryReadAsync());
        }

        [Test]
        public void CheckReadsSuccessful()
        {
            string logPath = Path.Combine(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug", string.Format("Tests Log {0}.log", DateTime.Now.ToString("yyyy - MM - dd HHmmss")));
            var outputTemplate = "({Timestamp:HH:mm:ss.fff}) {Level}: [{LineNumber}]{SourceContext}::{MemberName} - {Message}{NewLine}";

            ILogger log = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.File(logPath, LogEventLevel.Verbose, outputTemplate)
            .CreateLogger();

            XMLExtensionsManager v5Extensions = new XMLExtensionsManager(log);
            _ = v5Extensions.TryRegisterXMLExtensionsAsync(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V5\XuiElements.xml");
            _ = v5Extensions.TryRegisterXMLExtensionsAsync(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V5\9199DashElements.xml");
            XUIHelperCoreConstants.VersionedExtensions[0x5] = v5Extensions;

            //CheckReadSuccessful(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug\Example XURs\9199dashSysCslSetClockTime.xur", log);
            CheckReadSuccessful(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug\Example XURs\9199GamerCard.xur", log);
        }
    }
}