using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class XUI12 : XUI
    {
        public XUI12(string filePath, ILogger? logger = null) : base(filePath, logger)
        {

        }

        public static bool IsFileXUI12(string filePath, ILogger? logger = null)
        {
            try
            {
                logger = logger?.ForContext(typeof(XUI12));

                if (!File.Exists(filePath))
                {
                    logger?.Here().Verbose("The file at {0} doesn't exist, returning false.", filePath);
                    return false;
                }

                List<string> lines = File.ReadLines(filePath).ToList();
                if(lines.Count <= 0) 
                {
                    logger?.Here().Verbose("The file at {0} had no lines, returning false.", filePath);
                    return false;
                }

                if (!lines[0].Contains("<XuiCanvas version="))
                {
                    logger?.Here().Verbose("The file at {0} doesn't contain an XuiCanvas, returning false.", filePath);
                    return false;
                }

                logger?.Here().Verbose("The file at {0} is an XUI12, returning true.", filePath);
                return true;
            }
            catch(Exception ex)
            {
                logger?.Here().Error("Caught an exception when checking if file {0} was XUI12, returning false. The exception is: {1}", filePath, ex);
                return false;
            }
        }
    }
}
