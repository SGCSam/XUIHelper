using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static void SetAreIgnoredPropertiesActive(bool areActive)
        {
            AreIgnoredPropertiesActive = areActive;
        }

        #region Logging
        public static void SetLogger(string logPath, Serilog.Events.LogEventLevel level)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region XML Extensions
        public static async Task<bool> TryRegisterExtensionsGroupAsync(string extensionsGroupName, List<string> xmlExtensionFilePaths)
        {
            throw new NotImplementedException();
        }

        public static async Task<bool> TryRegisterExtensionsGroupAsync(string extensionsGroupName, string xmlExtensionsFilePath)
        {
            throw new NotImplementedException();
        }

        public static bool TrySetActiveExtensionsGroup()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Conversion
        public static async Task<bool> TryMassConvertDirectoryAsync(string directoryPath, XUIHelperSupportedFormats format, string outputDir, IXUIHelperProgressable? progressable)
        {
            throw new NotImplementedException();
        }

        public static async Task<bool> TryMassConvertFilesAsync(List<string> files, XUIHelperSupportedFormats format, string outputDir, IXUIHelperProgressable? progressable)
        {
            throw new NotImplementedException();
        }

        public static async Task<bool> TryConvertAsync(string filePath, XUIHelperSupportedFormats format, string outputPath)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
