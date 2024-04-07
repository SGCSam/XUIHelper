using CommandLine;
using XUIHelper.Core;

namespace XUIHelper.CLI
{
    [Verb("log", HelpText = "Sets the current XML extensions group to use.")]
    public class LoggerOptions : OptionsBase
    {
        [Option('f', "filepath", Required = true)]
        public string FilePath { get; set; } = string.Empty;

        [Option('l', "loglevel", Required = true)]
        public string LogLevel { get; set; } = string.Empty;

        private List<string> _ValidLogLevels = new List<string>() { "verbose", "info" };

        public LoggerOptions()
        {

        }

        public override Task HandleAsync()
        {
            if(!XUIHelperCoreUtilities.IsStringValidPath(FilePath))
            {
                Console.WriteLine("ERROR: \"{0}\" is not a valid log file path.", FilePath);
                return Task.CompletedTask;
            }

            int logLevelIndex = _ValidLogLevels.IndexOf(LogLevel.ToLower());
            if (logLevelIndex == -1)
            {
                Console.WriteLine("ERROR: \"{0}\" is not a valid log log level. Valid log levels are: \n{1}", LogLevel.ToLower(), string.Join("\n", _ValidLogLevels));
                return Task.CompletedTask;
            }

            Serilog.Events.LogEventLevel logLevel = Serilog.Events.LogEventLevel.Information;
            switch (_ValidLogLevels[logLevelIndex])
            {
                case "verbose":
                {
                    logLevel = Serilog.Events.LogEventLevel.Verbose;
                    break;
                }
                default:
                {
                    Console.WriteLine("ERROR: Unhandled log level of {0}.", _ValidLogLevels[logLevelIndex]);
                    return Task.CompletedTask;
                }
            }

            XUIHelperAPI.SetLogger(FilePath, logLevel);
            Console.WriteLine("SUCCESS: Set log file to \"{0}\" with {1} logging successfully!", FilePath, LogLevel.ToLower());
            return Task.CompletedTask;
        }
    }
}
