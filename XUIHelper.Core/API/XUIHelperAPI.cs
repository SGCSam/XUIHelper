using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public static class XUIHelperAPI
    {
        public enum XUIHelperSupportedFormats
        {
            XUR5,
            XUR8,
            XUI12
        }

        public static bool AreIgnoredPropertiesActive { get; private set; } = true;
        public static ILogger? Logger { get; private set; } = null;

        public static void SetAreIgnoredPropertiesActive(bool areActive)
        {
            AreIgnoredPropertiesActive = areActive;
        }

        #region Logging
        public static void SetLogger(string logPath, Serilog.Events.LogEventLevel level)
        {
            string outputTemplate = "({Timestamp:HH:mm:ss.fff}) {Level}: [{LineNumber}]{SourceContext}::{MemberName} - {Message}{NewLine}";

            Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.File(logPath, level, outputTemplate)
            .CreateLogger();
        }
        #endregion

        #region XML Extensions
        public static async Task RegisterExtensionsFromDirectoryAsync(string extensionsDirPath, ILogger? logger = null)
        {
            foreach (string subDir in Directory.GetDirectories(extensionsDirPath, "*", SearchOption.TopDirectoryOnly))
            {
                string groupName = new DirectoryInfo(subDir).Name;

                foreach (string extensionXML in Directory.GetFiles(subDir, "*.xml", SearchOption.TopDirectoryOnly))
                {
                    if (!await TryRegisterExtensionsGroupAsync(groupName, extensionXML))
                    {
                        logger?.Here().Error("Failed to register XML extension at {0}.", extensionXML);
                    }
                    else
                    {
                        logger?.Here().Information("Registered XML extensions from {0}.", extensionXML);
                    }
                }
            }
        }

        public static async Task<bool> TryRegisterExtensionsGroupAsync(string extensionsGroupName, string xmlExtensionsFilePath)
        {
            return await XMLExtensionsManager.TryRegisterExtensionsGroupAsync(extensionsGroupName, xmlExtensionsFilePath);
        }

        public static void SetCurrentExtensionsGroup(string groupName)
        {
            XMLExtensionsManager.SetCurrentGroup(groupName);
        }
        #endregion

        #region Conversion
        public static async Task<bool> TryMassConvertDirectoryAsync(string directoryPath, XUIHelperSupportedFormats format, string outputDir, IXUIHelperProgressable? progressable)
        {
            try
            {
                if(!XUIHelperCoreUtilities.IsStringValidPath(directoryPath))
                {
                    Logger?.Error("The directory path {0} is invalid, returning false.", directoryPath);
                    return false;
                }

                if (!XUIHelperCoreUtilities.IsStringValidPath(outputDir))
                {
                    Logger?.Error("The output directory path {0} is invalid, returning false.", outputDir);
                    return false;
                }

                if (progressable != null)
                {
                    progressable.SuccessfulWorkCount = 0;
                    progressable.FailedWorkCount = 0;
                    progressable.IsIndeterminate = true;
                    progressable.Description = "Searching for convertible files, please wait...";
                }

                List<string> convertibleFilePaths = new List<string>();
                foreach(string filePath in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
                {
                    if(XUI12.IsFileXUI12(filePath))
                    {
                        Logger?.Verbose("{0} is an XUI12.", filePath);
                        convertibleFilePaths.Add(filePath);
                    }
                    else if(XUR5.IsFileXUR5(filePath))
                    {
                        Logger?.Verbose("{0} is an XUR5.", filePath);
                        convertibleFilePaths.Add(filePath);
                    }
                    else if (XUR8.IsFileXUR8(filePath))
                    {
                        Logger?.Verbose("{0} is an XUR5.", filePath);
                        convertibleFilePaths.Add(filePath);
                    }
                }

                if(progressable != null)
                {
                    progressable.TotalWorkCount = convertibleFilePaths.Count;
                }

                Directory.CreateDirectory(outputDir);

                foreach(string filePath in convertibleFilePaths)
                {
                    string thisOutputPath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(filePath));
                    if(format == XUIHelperSupportedFormats.XUI12)
                    {
                        thisOutputPath += ".xui";
                    }
                    else
                    {
                        thisOutputPath += ".xur";
                    }

                    bool successful = await TryConvertAsync(filePath, format, thisOutputPath);

                    if(progressable != null)
                    {
                        if(successful)
                        {
                            progressable.SuccessfulWorkCount++;
                        }
                        else
                        {
                            progressable.FailedWorkCount++;
                        }

                        progressable.Description = string.Format("Converting files, converted {0} of {1}...", progressable.CompletedWorkCount, progressable.TotalWorkCount);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger?.Error("Caught an exception when trying to mass convert files from {0} to {1}, returning false. The exception is: {2}", directoryPath, format, ex);
                return false;
            }
        }

        public static async Task<bool> TryConvertAsync(string filePath, XUIHelperSupportedFormats format, string outputPath)
        {
            try
            {
                Logger?.Here().Information("Converting {0} to {1}.", filePath, format);

                if (!XUIHelperCoreUtilities.IsStringValidPath(outputPath))
                {
                    Logger?.Error("The output directory path {0} is invalid, returning false.", outputPath);
                    return false;
                }

                XUObject? rootObject = null;
                if(XUI12.IsFileXUI12(filePath))
                {
                    if(format == XUIHelperSupportedFormats.XUI12)
                    {
                        Logger?.Information("{0} is already an XUI12, no need to convert, copying to {1}", filePath, outputPath);
                        File.Copy(filePath, outputPath, true);
                        return true;
                    }

                    XUI12 xui12 = new XUI12(filePath, Logger);
                    if(!await xui12.TryReadAsync())
                    {
                        Logger?.Error("Failed to read XUI12, returning false.");
                        return false;
                    }

                    rootObject = xui12.RootObject;
                }
                else if(XUR5.IsFileXUR5(filePath))
                {
                    if (format == XUIHelperSupportedFormats.XUR5)
                    {
                        Logger?.Information("{0} is already an XUR5, no need to convert, copying to {1}", filePath, outputPath);
                        File.Copy(filePath, outputPath, true);
                        return true;
                    }

                    XUR5 xur5 = new XUR5(filePath, Logger);
                    if (!await xur5.TryReadAsync())
                    {
                        Logger?.Error("Failed to read XUR5, returning false.");
                        return false;
                    }

                    IDATASection? data = ((IXUR)xur5).TryFindXURSectionByMagic<IDATASection>(IDATASection.ExpectedMagic);
                    if(data == null)
                    {
                        Logger?.Error("XUR5 DATA section was null, returning false.");
                        return false;
                    }

                    rootObject = data.RootObject;
                }
                else if (XUR8.IsFileXUR8(filePath))
                {
                    if (format == XUIHelperSupportedFormats.XUR8)
                    {
                        Logger?.Information("{0} is already an XUR8, no need to convert, copying to {1}", filePath, outputPath);
                        File.Copy(filePath, outputPath, true);
                        return true;
                    }

                    XUR8 xur8 = new XUR8(filePath, Logger);
                    if (!await xur8.TryReadAsync())
                    {
                        Logger?.Error("Failed to read XUR8, returning false.");
                        return false;
                    }

                    IDATASection? data = ((IXUR)xur8).TryFindXURSectionByMagic<IDATASection>(IDATASection.ExpectedMagic);
                    if (data == null)
                    {
                        Logger?.Error("XUR5 DATA section was null, returning false.");
                        return false;
                    }

                    rootObject = data.RootObject;
                }

                if(rootObject == null) 
                {
                    Logger?.Error("Root object was null, the file must be of an invalid type, returning false.");
                    return false;
                }

                switch(format)
                {
                    case XUIHelperSupportedFormats.XUI12:
                    {
                        XUI12 xui12 = new XUI12(outputPath, Logger);
                        if(!await xui12.TryWriteAsync(rootObject))
                        {
                            Logger?.Error("Failed to write XUI12 to {0}, returning false.", outputPath);
                            return false;
                        }
                        else
                        {
                            Logger?.Information("Converted {0} to XUI12 successfully!", filePath);
                            return true;
                        }
                    }
                    case XUIHelperSupportedFormats.XUR5:
                    {
                        XUR5 xur5 = new XUR5(outputPath, Logger);
                        if (!await xur5.TryWriteAsync(rootObject))
                        {
                            Logger?.Error("Failed to write XUR5 to {0}, returning false.", outputPath);
                            return false;
                        }
                        else
                        {
                            Logger?.Information("Converted {0} to XUR5 successfully!", filePath);
                            return true;
                        }
                    }
                    case XUIHelperSupportedFormats.XUR8:
                    {
                        XUR8 xur8 = new XUR8(outputPath, Logger);
                        if (!await xur8.TryWriteAsync(rootObject))
                        {
                            Logger?.Error("Failed to write XUR8 to {0}, returning false.", outputPath);
                            return false;
                        }
                        else
                        {
                            Logger?.Information("Converted {0} to XUR8 successfully!", filePath);
                            return true;
                        }
                    }
                    default:
                    {
                        Logger?.Error("Unhandled format case of {0}, returning false.", format);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger?.Error("Caught an exception when trying to convert {0} to {1}, returning false. The exception is: {2}", filePath, format, ex);
                return false;
            }
        }
        #endregion
    }
}
