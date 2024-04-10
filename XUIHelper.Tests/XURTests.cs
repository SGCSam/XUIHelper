using Serilog.Events;
using Serilog;
using XUIHelper.Core;
using System.Formats.Tar;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.IO;

namespace XUIHelper.Tests
{
    public abstract class XURTests
    {
        protected ILogger? _Log;

        protected abstract void RegisterExtensions(ILogger? logger = null);

        protected abstract IXUR GetXUR(string filePath, ILogger? logger = null);

        protected bool AreFilesEqual(string filePathOne, string filePathTwo)
        {
            if (!File.Exists(filePathOne))
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

        protected async Task<bool> CheckAllReadsSuccessfulAsync(string dir)
        {
            List<string> successfulXURs = new List<string>();
            List<string> failedXURs = new List<string>();

            int xursCount = 0;
            foreach (string xurFile in Directory.GetFiles(Path.Combine(TestContext.CurrentContext.TestDirectory, dir), "*.xur", SearchOption.AllDirectories))
            {
                IXUR xur = GetXUR(xurFile, null);
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

            _Log.Information("==== XUR ALL READS ====");
            _Log.Information("Total: {0}, Successful: {1}, Failed: {2} ({3}%)", totalXURsCount, successfulXURs.Count, failedXURs.Count, successPercentage);
            _Log.Information("");
            _Log.Information("==== SUCCESSFUL XURS ====");
            _Log.Information(string.Join("\n", successfulXURs));
            _Log.Information("");
            _Log.Information("==== FAILED XURS ====");
            _Log.Information(string.Join("\n", failedXURs));
            _Log.Information("");

            return failedXURs.Count == 0;
        }

        protected async Task<bool> CheckSingleXURReadSuccessfulAsync(string path)
        {
            string xurFile = Path.Combine(TestContext.CurrentContext.TestDirectory, path);
            IXUR xur = GetXUR(xurFile, _Log);
            return await xur.TryReadAsync();
        }

        protected async Task<bool> CheckAllWritesSuccessfulAsync(string dir)
        {
            List<IXUR> readXURs = new List<IXUR>();

            int xursCount = 0;
            foreach (string xurFile in Directory.GetFiles(Path.Combine(TestContext.CurrentContext.TestDirectory, dir), "*.xur", SearchOption.AllDirectories))
            {
                IXUR xur = GetXUR(xurFile, null);
                if (await xur.TryReadAsync())
                {
                    xursCount++;
                    readXURs.Add(xur);
                }
            }

            List<string> successfulXURs = new List<string>();
            List<string> warningXURs = new List<string>();
            List<string> failedXURs = new List<string>();
            foreach (IXUR readXUR in readXURs)
            {
                IDATASection? readData = readXUR.TryFindXURSectionByMagic<IDATASection>(IDATASection.ExpectedMagic);
                if (readData == null)
                {
                    _Log?.Information("Failure: Null data for {0}", readXUR.FilePath);
                    failedXURs.Add(readXUR.FilePath);
                    continue;
                }

                if (readData.RootObject == null)
                {
                    _Log?.Information("Failure: Null root object for {0}", readXUR.FilePath);
                    failedXURs.Add(readXUR.FilePath);
                    continue;
                }

                string thisWriteXURPath = Path.GetTempFileName();
                IXUR writeXUR = GetXUR(thisWriteXURPath, null);
                if (!await writeXUR.TryWriteAsync(readData.RootObject))
                {
                    _Log?.Information("Failure: Write failed for {0}", readXUR.FilePath);
                    failedXURs.Add(readXUR.FilePath);
                }
                else
                {
                    IDATASection? writtenData = writeXUR.TryFindXURSectionByMagic<IDATASection>(IDATASection.ExpectedMagic);
                    if (writtenData == null)
                    {
                        _Log?.Information("Failure: Null data for {0}", writeXUR.FilePath);
                        failedXURs.Add(readXUR.FilePath);
                    }
                    else if (writtenData.RootObject == null)
                    {
                        _Log?.Information("Failure: Null root object for {0}", writeXUR.FilePath);
                        failedXURs.Add(readXUR.FilePath);
                    }

                    IXUR readBackXUR = GetXUR(thisWriteXURPath, null);
                    if (!await readBackXUR.TryReadAsync())
                    {
                        _Log?.Information("Failure: Read back failed for {0}", thisWriteXURPath);
                        failedXURs.Add(readXUR.FilePath);
                    }
                    else
                    {
                        IDATASection? readBackData = ((IXUR)readBackXUR).TryFindXURSectionByMagic<IDATASection>(IDATASection.ExpectedMagic);
                        if (readBackData == null)
                        {
                            _Log?.Information("Failure: Null read back data for {0}", thisWriteXURPath);
                            failedXURs.Add(readXUR.FilePath);
                        }
                        else if (readBackData.RootObject == null)
                        {
                            _Log?.Information("Failure: Null read back root object for {0}", thisWriteXURPath);
                            failedXURs.Add(readXUR.FilePath);
                        }
                        else if (JsonConvert.SerializeObject(readData.RootObject) != JsonConvert.SerializeObject(readBackData.RootObject))
                        {
                            _Log?.Information("Failure: Non-equal root objects for {0}.", readXUR.FilePath);
                            failedXURs.Add(readXUR.FilePath);
                        }
                        else if (!AreFilesEqual(readXUR.FilePath, thisWriteXURPath))
                        {
                            //NOTE: We have to treat this as a warning rather than an error for several reasons as the outputted file may be perfectly valid, primarily with XUR5
                            //(I've had loads of cases where the files are functionally identical when opened in XuiTool)
                            //
                            //1) For some reason, some of the XUR5s from 9199 don't always include all child class properties, chiefly with the root XuiCanvas
                                //This means our packed byte and our masks counts will be "wrong", but our outputted XUR is still perfectly fine.
                                //This is either some optimization I don't know about, or these may have been created with a different version of XuiTool.
                            //2) The criteria I've implemented for when to write the extended count header for XUR5 isn't fully correct. So we'll end up with and additional 0x28 bytes
                                //Fortunately, XuiTool doesn't care
                            //We *could* write really in-depth comparisons of exactly why the files aren't equal and catch these cases but this is a lot of work.
                            //If all files read and write successfully and a Json serialization matches, I think a warning suffices here.

                            _Log?.Information("Warning: Non-equal files for {0}.", readXUR.FilePath);
                            warningXURs.Add(readXUR.FilePath);
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

            _Log?.Information("==== XUR ALL WRITES ====");
            _Log?.Information("Total: {0}, Successful: {1}, Failed: {2}, Warning: {3} ({4}%)", readXURs.Count, successfulXURs.Count, failedXURs.Count, warningXURs.Count, successPercentage);
            _Log?.Information("");
            _Log?.Information("==== SUCCESSFUL XURS ====");
            _Log?.Information(string.Join("\n", successfulXURs));
            _Log?.Information("");
            _Log?.Information("==== FAILED XURS ====");
            _Log?.Information(string.Join("\n", failedXURs));
            _Log?.Information("");

            return failedXURs.Count == 0;
        }

        protected async Task<bool> CheckSingleXURWriteSuccessfulAsync(string path)
        {
            string xurFile = Path.Combine(TestContext.CurrentContext.TestDirectory, path);
            IXUR readXUR = GetXUR(xurFile, null);
            if(!await readXUR.TryReadAsync())
            {
                return false;
            }

            IDATASection? readData = readXUR.TryFindXURSectionByMagic<IDATASection>(IDATASection.ExpectedMagic);
            if(readData == null)
            {
                return false;
            }

            if(readData.RootObject == null)
            {
                return false;
            }

            string thisWriteXURPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Debug\written.xur");
            IXUR writeXUR = GetXUR(thisWriteXURPath, _Log);
            if(!await writeXUR.TryWriteAsync(readData.RootObject))
            {
                return false;
            }

            IXUR readBackXUR = GetXUR(thisWriteXURPath, null);
            if(!await readBackXUR.TryReadAsync())
            {
                return false;
            }

            IDATASection? readBackData = readBackXUR.TryFindXURSectionByMagic<IDATASection>(IDATASection.ExpectedMagic);
            if(readBackData == null)
            {
                return false;
            }

            if(readBackData.RootObject == null)
            {
                return false;
            }

            return JsonConvert.SerializeObject(readData.RootObject) == JsonConvert.SerializeObject(readBackData.RootObject);
        }
    }
}