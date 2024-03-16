using Serilog.Events;
using Serilog;
using XUIHelper.Core;
using System.Formats.Tar;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.IO;

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

        private void RegisterExtensions(ILogger? logger = null)
        {
            XMLExtensionsManager v5Extensions = new XMLExtensionsManager(logger);
            _ = v5Extensions.TryRegisterXMLExtensionsAsync(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V5\XuiElements.xml");
            _ = v5Extensions.TryRegisterXMLExtensionsAsync(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V5\9199DashElements.xml");
            _ = v5Extensions.TryRegisterXMLExtensionsAsync(@"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Assets\V5\9199HUDElements.xml");
            XUIHelperCoreConstants.VersionedExtensions[0x5] = v5Extensions;
        }

        private async Task<XUR5?> GetReadXUR(string filePath, ILogger? logger = null)
        {
            XUR5 xur = new XUR5(filePath, logger);
            if (await xur.TryReadAsync())
            {
                return xur;
            }

            return null;
        }

        private bool AreFilesEqual(string filePathOne, string filePathTwo)
        {
            if(!File.Exists(filePathOne))
            {
                return false;
            }

            if (!File.Exists(filePathTwo))
            {
                return false;
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] fileOneHash;
                byte[] fileTwoHash;

                using (FileStream fileOneStream = File.OpenRead(filePathOne))
                {
                    fileOneHash = sha256.ComputeHash(fileOneStream);
                }

                using (FileStream fileTwoStream = File.OpenRead(filePathTwo))
                {
                    fileTwoHash = sha256.ComputeHash(fileTwoStream);
                }

                return fileOneHash.SequenceEqual(fileTwoHash);
            }
        }


        [Test]
        public async Task CheckAllReadsSuccessful()
        {
            List<string> successfulXURs = new List<string>();
            List<string> failedXURs = new List<string>();

            int xursCount = 0;
            foreach(string xurFile in Directory.GetFiles(Path.Combine(TestContext.CurrentContext.TestDirectory, "Test Data/XUR/9199"), "*.xur", SearchOption.AllDirectories)) 
            {
                XUR5 xur = new XUR5(xurFile, null);
                if (!await xur.TryReadAsync())
                {
                    failedXURs.Add(xurFile);
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

            Assert.True(failedXURs.Count == 0);
        }

        [Test]
        public async Task CheckSingleXURReadSuccessful()
        {
            string xurFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Test Data/XUR/9199/PhotoCapture.xur");
            XUR5 xur = new XUR5(xurFile, _Log);
            Assert.True(await xur.TryReadAsync());
        }

        [Test]
        public async Task CheckAllWritesSuccessful()
        {
            List<XUR5> readXURs = new List<XUR5>();

            int xursCount = 0;
            foreach (string xurFile in Directory.GetFiles(Path.Combine(TestContext.CurrentContext.TestDirectory, "Test Data/XUR/9199"), "*.xur", SearchOption.AllDirectories))
            {
                XUR5 xur = new XUR5(xurFile, null);
                if (await xur.TryReadAsync())
                {
                    xursCount++;
                    readXURs.Add(xur);
                }
            }

            List<string> successfulXURs = new List<string>();
            List<string> warningXURs = new List<string>();
            List<string> failedXURs = new List<string>();
            foreach (XUR5 readXUR in readXURs)
            {
                IDATASection? readData = ((IXUR)readXUR).TryFindXURSectionByMagic<IDATASection>(IDATASection.ExpectedMagic);
                if(readData == null)
                {
                    _Log.Information("Failure: Null data for {0}", readXUR.FilePath);
                    failedXURs.Add(readXUR.FilePath);
                    continue;
                }

                if (readData.RootObject == null)
                {
                    _Log.Information("Failure: Null root object for {0}", readXUR.FilePath);
                    failedXURs.Add(readXUR.FilePath);
                    continue;
                }

                string thisWriteXURPath = Path.GetTempFileName();
                XUR5 writeXUR = new XUR5(thisWriteXURPath, null);
                if (!await writeXUR.TryWriteAsync(readData.RootObject))
                {
                    _Log.Information("Failure: Write failed for {0}", readXUR.FilePath);
                    failedXURs.Add(readXUR.FilePath);
                }
                else
                {
                    IDATASection? writtenData = ((IXUR)writeXUR).TryFindXURSectionByMagic<IDATASection>(IDATASection.ExpectedMagic);
                    if (writtenData == null)
                    {
                        _Log.Information("Failure: Null data for {0}", writeXUR.FilePath);
                        failedXURs.Add(readXUR.FilePath);
                    }
                    else if (writtenData.RootObject == null)
                    {
                        _Log.Information("Failure: Null root object for {0}", writeXUR.FilePath);
                        failedXURs.Add(readXUR.FilePath);
                    }

                    XUR5 readBackXUR = new XUR5(thisWriteXURPath, null);
                    if(!await readBackXUR.TryReadAsync())
                    {
                        _Log.Information("Failure: Read back failed for {0}", thisWriteXURPath);
                        failedXURs.Add(readXUR.FilePath);
                    }
                    else
                    {
                        IDATASection? readBackData = ((IXUR)readBackXUR).TryFindXURSectionByMagic<IDATASection>(IDATASection.ExpectedMagic);
                        if (readBackData == null)
                        {
                            _Log.Information("Failure: Null read back data for {0}", thisWriteXURPath);
                            failedXURs.Add(readXUR.FilePath);
                        }
                        else if (readBackData.RootObject == null)
                        {
                            _Log.Information("Failure: Null read back root object for {0}", thisWriteXURPath);
                            failedXURs.Add(readXUR.FilePath);
                        }
                        else if (JsonConvert.SerializeObject(readData.RootObject) != JsonConvert.SerializeObject(readBackData.RootObject))
                        {
                            _Log.Information("Failure: Non-equal root objects for {0}.", readXUR.FilePath);
                            failedXURs.Add(readXUR.FilePath);
                        }
                        else if (!AreFilesEqual(readXUR.FilePath, thisWriteXURPath))
                        {
                            _Log.Information("Warning: Non-equal files for {0}.", readXUR.FilePath);
                            warningXURs.Add(readXUR.FilePath);

                            //string filePath = Path.Combine(@"C:\Users\sgcsa\Desktop\Warning XURs", Path.GetFileName(readXUR.FilePath));
                            //File.Copy(thisWriteXURPath, filePath, true);
                        }
                        else
                        {
                            successfulXURs.Add(readXUR.FilePath);
                        }
                    }
                }

                File.Delete(thisWriteXURPath);
            }

            float successPercentage = ((successfulXURs.Count + warningXURs.Count) / (float)readXURs.Count) * 100.0f;

            _Log.Information("==== XUR5 ALL WRITES ====");
            _Log.Information("Total: {0}, Successful: {1}, Failed: {2}, Warning: {3} ({4}%)", readXURs.Count, successfulXURs.Count, failedXURs.Count, warningXURs.Count, successPercentage);
            _Log.Information("");
            _Log.Information("==== SUCCESSFUL XURS ====");
            _Log.Information(string.Join("\n", successfulXURs));
            _Log.Information("");
            _Log.Information("==== FAILED XURS ====");
            _Log.Information(string.Join("\n", failedXURs));
            _Log.Information("");

            Assert.True(failedXURs.Count == 0);
        }

        [Test]
        public async Task CheckSingleXURWriteSuccessful()
        {
            string xurFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "Test Data/XUR/9199/PhotoCapture.xur");
            XUR5 readXUR = new XUR5(xurFile, null);
            Assert.True(await readXUR.TryReadAsync());

            IDATASection? readData = ((IXUR)readXUR).TryFindXURSectionByMagic<IDATASection>(IDATASection.ExpectedMagic);
            Assert.NotNull(readData);
            Assert.NotNull(readData.RootObject);

            string thisWriteXURPath = @"F:\Code Repos\XUIHelper\XUIHelper.Core\XUIHelper.Core\Debug\written.xur";
            XUR5 writeXUR = new XUR5(thisWriteXURPath, _Log);
            Assert.True(await writeXUR.TryWriteAsync(readData.RootObject));

            XUR5 readBackXUR = new XUR5(thisWriteXURPath, null);
            Assert.True(await readBackXUR.TryReadAsync());

            IDATASection? readBackData = ((IXUR)readBackXUR).TryFindXURSectionByMagic<IDATASection>(IDATASection.ExpectedMagic);
            Assert.NotNull(readBackData);
            Assert.NotNull(readBackData.RootObject);
            Assert.That(JsonConvert.SerializeObject(readData.RootObject), Is.EqualTo(JsonConvert.SerializeObject(readBackData.RootObject)));
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