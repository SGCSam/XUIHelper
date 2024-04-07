﻿using Serilog;
using Serilog.Core;
using Serilog.Events;
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
            throw new NotImplementedException();
        }

        public static async Task<bool> TryConvertAsync(string filePath, XUIHelperSupportedFormats format, string outputPath)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
