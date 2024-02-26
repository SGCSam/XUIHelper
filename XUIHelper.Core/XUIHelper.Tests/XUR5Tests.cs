using Serilog.Events;
using Serilog;
using XUIHelper.Core;

namespace XUIHelper.Tests
{
    public class Tests
    {
        //TODO: Proper addition of IXURCountHeader so we can handle this for XUR8 as well
        //TODO: Override Read in XUR5, if it succeeds, verify the count header with our own count functions on the root object
        //TODO: Add a check to ensure count header is not null if 0x1 flag is set in the base header

        [SetUp]
        public void Setup()
        {
        }

        public async Task<bool> CheckReadSuccessful(string filePath, ILogger? logger = null)
        {
            XUR5 xur = new XUR5(filePath, logger);
            bool successful = await xur.TryReadAsync();
            IDATASection? data = ((IXUR)xur).TryFindXURSectionByMagic<IDATASection>(IDATASection.ExpectedMagic);
            if (data != null && data.RootObject != null)
            {
                int objCount = data.RootObject.GetTotalObjectsCount();
                int totalPropertiesCount = data.RootObject.GetTotalPropertiesCount();
                int propArrayCount = data.RootObject.GetPropertiesArrayCount();
                int keyframePropertiesCount = data.RootObject.GetTotalKeyframePropertiesCount();
                int totalKeyframePropertiesClassDepth = data.RootObject.TryGetTotalKeyframePropertyDefinitionsClassDepth(0x5).Value;
                int keyframePropertyDefinitionsCount = data.RootObject.GetKeyframePropertyDefinitionsCount();
                int keyframesCount = data.RootObject.GetKeyframesCount();
                int timelinesCount = data.RootObject.GetTimelinesCount();
                int namedFramesCount = data.RootObject.GetNamedFramesCount();
                int objWithChildrenCount = data.RootObject.GetObjectsWithChildrenCount();


                XUR5CountHeader? countHeader = ((XUR5Header)xur.Header).CountHeader;
                int debug = 0;
            }
            return successful;
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

            Assert.True(await CheckReadSuccessful(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug\Example XURs\9199GamerCard.xur", log));
        }

        [Test]
        public async Task CheckGamerCardEditReadSuccessful()
        {
            string logPath = Path.Combine(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug", string.Format("Tests Log {0}.log", DateTime.Now.ToString("yyyy - MM - dd HHmmss")));
            var outputTemplate = "({Timestamp:HH:mm:ss.fff}) {Level}: [{LineNumber}]{SourceContext}::{MemberName} - {Message}{NewLine}";
            ILogger log = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.File(logPath, LogEventLevel.Verbose, outputTemplate)
            .CreateLogger();

            RegisterExtensions(log);

            Assert.True(await CheckReadSuccessful(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug\9199GamerCardEdit.xur", log));
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

            bool anyFailed = false;
            foreach(string xurFilePath in Directory.GetFiles(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug\Example XURs\9199 All\Working", "*.xur"))
            {
                string fileName = Path.GetFileName(xurFilePath);
                if(!await CheckReadSuccessful(xurFilePath, log))
                {
                    anyFailed = true;
                    File.Move(xurFilePath, Path.Combine(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug\Example XURs\9199 All\Not Working", fileName));
                }
            }

            Assert.False(anyFailed);
        }
    }
}