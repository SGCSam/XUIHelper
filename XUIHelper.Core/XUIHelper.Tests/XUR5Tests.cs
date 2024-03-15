using Serilog.Events;
using Serilog;
using XUIHelper.Core;
using System.Formats.Tar;

namespace XUIHelper.Tests
{
    public class XUR5Tests
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

        public void RegisterExtensions(ILogger? logger = null)
        {
            XMLExtensionsManager v5Extensions = new XMLExtensionsManager(logger);
            _ = v5Extensions.TryRegisterXMLExtensionsAsync(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V5\XuiElements.xml");
            _ = v5Extensions.TryRegisterXMLExtensionsAsync(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V5\9199DashElements.xml");
            _ = v5Extensions.TryRegisterXMLExtensionsAsync(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V5\9199HUDElements.xml");
            XUIHelperCoreConstants.VersionedExtensions[0x5] = v5Extensions;
        }

        public async Task<XUR5?> GetReadXUR(string filePath, ILogger? logger = null)
        {
            XUR5 xur = new XUR5(filePath, logger);
            if (await xur.TryReadAsync())
            {
                return xur;
            }

            return null;
        }

        [Test]
        public async Task CheckAllReadsSuccessful()
        {
            bool anyFailed = false;
            List<string> successfulXURs = new List<string>();
            List<string> failedXURs = new List<string>();

            int xursCount = 0;
            foreach(string xurFile in Directory.GetFiles(Path.Combine(TestContext.CurrentContext.TestDirectory, "Test Data/XUR/9199"), "*.xur", SearchOption.AllDirectories)) 
            {
                XUR5 xur = new XUR5(xurFile, null);
                if (!await xur.TryReadAsync())
                {
                    failedXURs.Add(xurFile);
                    anyFailed = true;
                }
                else
                {
                    successfulXURs.Add(xurFile);
                }

                xursCount++;
            }

            int totalXURsCount = successfulXURs.Count + failedXURs.Count;
            float successPercentage = (successfulXURs.Count / (float)totalXURsCount) * 100.0f;

            _Log.Information("==== XUR5 ALL READS ====");
            _Log.Information("Total: {0}, Successful: {1}, Failed: {2} ({3}%)", totalXURsCount, successfulXURs.Count, failedXURs.Count, successPercentage);
            _Log.Information("");
            _Log.Information("==== SUCCESSFUL XURS ====");
            _Log.Information(string.Join("\n", successfulXURs));
            _Log.Information("");
            _Log.Information("==== FAILED XURS ====");
            _Log.Information(string.Join("\n", failedXURs));
            _Log.Information("");

            Assert.False(anyFailed);
        }

        [Test]
        public async Task CheckSingleXURReadSuccessful()
        {
            string xurFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Test Data/XUR/9199/MarketplaceTab.xur");
            XUR5 xur = new XUR5(xurFile, _Log);
            Assert.True(await xur.TryReadAsync());
        }

        [Test]
        public async Task CheckGamerCardWriteSuccessful()
        {
            string logPath = Path.Combine(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug", string.Format("Tests Log {0}.log", DateTime.Now.ToString("yyyy - MM - dd HHmmss")));
            var outputTemplate = "({Timestamp:HH:mm:ss.fff}) {Level}: [{LineNumber}]{SourceContext}::{MemberName} - {Message}{NewLine}";
            ILogger log = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.File(logPath, LogEventLevel.Verbose, outputTemplate)
            .CreateLogger();

            RegisterExtensions(log);

            XUR5? xur = await GetReadXUR(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug\Example XURs\9199Gamercard.xur");
            Assert.NotNull(xur);

            IDATASection? data = ((IXUR)xur).TryFindXURSectionByMagic<IDATASection>(IDATASection.ExpectedMagic);
            Assert.NotNull(data);

            ISTRNSection? strn = ((IXUR)xur).TryFindXURSectionByMagic<ISTRNSection>(ISTRNSection.ExpectedMagic);
            Assert.NotNull(strn);

            IVECTSection? vect = ((IXUR)xur).TryFindXURSectionByMagic<IVECTSection>(IVECTSection.ExpectedMagic);
            Assert.NotNull(vect);

            IQUATSection? quat = ((IXUR)xur).TryFindXURSectionByMagic<IQUATSection>(IQUATSection.ExpectedMagic);
            Assert.NotNull(quat);

            ICUSTSection? cust = ((IXUR)xur).TryFindXURSectionByMagic<ICUSTSection>(ICUSTSection.ExpectedMagic);
            Assert.NotNull(cust);

            XUR5 writeXUR = new XUR5(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug\written.xur", log);
            Assert.True(await writeXUR.TryWriteAsync(data.RootObject));

            IVECTSection? writeVects = ((IXUR)writeXUR).TryFindXURSectionByMagic<IVECTSection>(IVECTSection.ExpectedMagic);
            Assert.NotNull(writeVects);
            Assert.That(writeVects.Vectors, Is.EqualTo(vect.Vectors));

            IQUATSection? writeQuats = ((IXUR)writeXUR).TryFindXURSectionByMagic<IQUATSection>(IQUATSection.ExpectedMagic);
            Assert.NotNull(writeQuats);
            Assert.That(writeQuats.Quaternions, Is.EqualTo(quat.Quaternions));

            ICUSTSection? writeCust = ((IXUR)writeXUR).TryFindXURSectionByMagic<ICUSTSection>(ICUSTSection.ExpectedMagic);
            Assert.NotNull(writeCust);
            Assert.That(writeCust.Figures, Is.EqualTo(cust.Figures));
        }
    }
}