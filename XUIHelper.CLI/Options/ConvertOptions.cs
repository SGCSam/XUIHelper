using CommandLine;
using XUIHelper.Core;

namespace XUIHelper.CLI
{
    [Verb("conv", HelpText = "Converts a single file to the specified XU format and outputs the result to a new file.")]
    public class ConvertOptions : OptionsBase
    {
        [Option('s', "sourcefilepath", Required = true)]
        public string SourceFilePath { get; set; } = string.Empty;

        [Option('f', "filetype", Required = true)]
        public string FileFormat { get; set; } = string.Empty;

        [Option('o', "outputfilepath", Required = true)]
        public string OutputFilePath { get; set; } = string.Empty;

        [Option('g', "groupname", Required = true)]
        public string ExtensionsGroupName { get; set; } = string.Empty;

        [Option('i', "ignoreproperties", Required = false)]
        public bool IgnoreProperties { get; set; } = false;

        [Option('l', "logfilepath", Required = false)]
        public string LogFilePath { get; set; } = string.Empty;

        [Option('v', "logverbositylevel", Required = false)]
        public string LogLevel { get; set; } = string.Empty;

        private List<string> _ValidFormats = new List<string>() { "xurv5", "xurv8", "xuiv12" };
        private List<string> _ValidLogLevels = new List<string>() { "verbose", "info" };

        public ConvertOptions()
        {

        }

        public override async Task HandleAsync()
        {
            if (!File.Exists(SourceFilePath))
            {
                Console.WriteLine("ERROR: The source file at \"{0}\" does not exist.", SourceFilePath);
                return;
            }

            int formatIndex = _ValidFormats.IndexOf(FileFormat.ToLower());
            if (formatIndex == -1)
            {
                Console.WriteLine("ERROR: \"{0}\" is not a valid file format. Valid formats are: \n{1}", FileFormat.ToLower(), string.Join("\n", _ValidFormats));
                return;
            }

            XUIHelperAPI.XUIHelperSupportedFormats format = XUIHelperAPI.XUIHelperSupportedFormats.XUR5;
            switch (_ValidFormats[formatIndex])
            {
                case "xurv5":
                {
                    format = XUIHelperAPI.XUIHelperSupportedFormats.XUR5;
                    break;
                }
                case "xurv8":
                {
                    format = XUIHelperAPI.XUIHelperSupportedFormats.XUR8;
                    break;
                }
                case "xuiv12":
                {
                    format = XUIHelperAPI.XUIHelperSupportedFormats.XUI12;
                    break;
                }
                default:
                {
                    Console.WriteLine("ERROR: Unhandled format of {0}.", _ValidFormats[formatIndex]);
                    return;
                }
            }

            if (!XUIHelperCoreUtilities.IsStringValidPath(OutputFilePath))
            {
                Console.WriteLine("ERROR: The output file path \"{0}\" is invalid.", OutputFilePath);
                return;
            }

            string? directoryName = Path.GetDirectoryName(OutputFilePath);
            if(string.IsNullOrEmpty(directoryName))
            {
                Console.WriteLine("ERROR: The directory name for output file path \"{0}\" is invalid.", OutputFilePath);
                return;
            }

            Directory.CreateDirectory(directoryName);

            XUIHelperAPI.SetCurrentExtensionsGroup(ExtensionsGroupName);
            XUIHelperAPI.SetAreIgnoredPropertiesActive(!IgnoreProperties);

            if (!string.IsNullOrEmpty(LogFilePath))
            {
                if (!XUIHelperCoreUtilities.IsStringValidPath(LogFilePath))
                {
                    Console.WriteLine("ERROR: \"{0}\" is not a valid log file path.", LogFilePath);
                    return;
                }

                int logLevelIndex = _ValidLogLevels.IndexOf(LogLevel.ToLower());
                if (logLevelIndex == -1)
                {
                    Console.WriteLine("ERROR: \"{0}\" is not a valid log level. Valid log levels are: \n{1}", LogLevel.ToLower(), string.Join("\n", _ValidLogLevels));
                    return;
                }

                Serilog.Events.LogEventLevel logLevel = Serilog.Events.LogEventLevel.Information;
                switch (_ValidLogLevels[logLevelIndex])
                {
                    case "verbose":
                    {
                        logLevel = Serilog.Events.LogEventLevel.Verbose;
                        break;
                    }
                    case "info":
                    {
                        logLevel = Serilog.Events.LogEventLevel.Information;
                        break;
                    }
                    default:
                    {
                        Console.WriteLine("ERROR: Unhandled log level of {0}.", _ValidLogLevels[logLevelIndex]);
                        return;
                    }
                }

                XUIHelperAPI.SetLogger(LogFilePath, logLevel);
            }
            
            if (!await XUIHelperAPI.TryConvertAsync(SourceFilePath, format, OutputFilePath))
            {
                Console.WriteLine("ERROR: Failed to convert \"{0}\" to {1}. Consider using a log file and checking it for more information.", SourceFilePath, format);
            }
            else
            {
                Console.WriteLine("SUCCESS: Converted \"{0}\" to {1} successfully!", SourceFilePath, format);
            }
        }
    }
}
