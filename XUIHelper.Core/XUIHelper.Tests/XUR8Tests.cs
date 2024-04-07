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
        //TODO: GUI app
        //TODO: XUR8 keyframe data unknown upper bits and flags

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
            XMLExtensionsManager.Initialize(logger);
            _ = XMLExtensionsManager.TryRegisterExtensionsGroupAsync("XUR8Tests", @"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V8\XuiElements.xml");
            _ = XMLExtensionsManager.TryRegisterExtensionsGroupAsync("XUR8Tests", @"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V8\17559DashElements.xml");
            _ = XMLExtensionsManager.TryRegisterExtensionsGroupAsync("XUR8Tests", @"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V8\17559HUDElements.xml");
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
            //LegendScene - Always unknown 1, vector 11
            //oobeControllerNoLanguage
            //hudbkgnd
            //BackHandle
            //KeyboardBase - No unknowns, flag 0x3 only
            //OobeNetworkSelection - Always unknown 1, vector 0xC
            //CarouselSlotScene - Always unknown 1, vector 0xF
            //VuiCommand - Always unknown 1, vector 0x2 and 0x4
            //Template1 - Unknown 2, flag 0x4 (Remove Columns, Rows and AutoId from XUI)

            Assert.True(await CheckSingleXURReadSuccessfulAsync(@"Test Data/XUR/17559/LegendScene.xur"));

            /*string xurFile = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Test Data/XUR/17559/LightweightContainerScene.xur");
            IXUR xur = GetXUR(xurFile, _Log);
            if(await xur.TryReadAsync())
            {
                XUI12 xui12 = new XUI12(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug\written.xui", null);
                xui12.TryWriteAsync(0x8, (xur.TryFindXURSectionByMagic<IDATASection>(IDATASection.ExpectedMagic)).RootObject);
            }*/
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