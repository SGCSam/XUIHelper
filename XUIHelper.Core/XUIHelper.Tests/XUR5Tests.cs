using Serilog.Events;
using Serilog;
using XUIHelper.Core;

namespace XUIHelper.Tests
{
    public class XUR5Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        public async Task<bool> CheckReadSuccessful(string filePath, ILogger? logger = null)
        {
            XUR5 xur = new XUR5(filePath, logger);
            return await xur.TryReadAsync();
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
        public async Task CheckFigureReadSuccessful()
        {
            string logPath = Path.Combine(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug", string.Format("Tests Log {0}.log", DateTime.Now.ToString("yyyy - MM - dd HHmmss")));
            var outputTemplate = "({Timestamp:HH:mm:ss.fff}) {Level}: [{LineNumber}]{SourceContext}::{MemberName} - {Message}{NewLine}";
            ILogger log = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.File(logPath, LogEventLevel.Verbose, outputTemplate)
            .CreateLogger();

            RegisterExtensions(log);

            Assert.True(await CheckReadSuccessful(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug\Example XURs\figure.xur", log));
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

            XUR5 writeXUR = new XUR5(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug\written.xur", log);
            //Assert.True(await writeXUR.TryWriteAsync(data.RootObject));
            await writeXUR.TryWriteAsync(data.RootObject);

            IVECTSection? writeVects = ((IXUR)writeXUR).TryFindXURSectionByMagic<IVECTSection>(IVECTSection.ExpectedMagic);
            Assert.NotNull(writeVects);

            Assert.That(writeVects.Vectors, Is.EqualTo(vect.Vectors));

        }
    }
}