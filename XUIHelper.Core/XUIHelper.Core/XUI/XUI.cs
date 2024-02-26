using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public abstract class XUI : IXUI
    {
        public ILogger? Logger { get; set; }

        public string FilePath { get; private set; }

        protected BinaryReader Reader { get; private set; }

        public XUI(string filePath, ILogger? logger = null)
        {
            FilePath = filePath;
            Logger = logger?.ForContext(typeof(XUI));
        }

        public virtual async Task<bool> TryReadAsync()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    Logger?.Here().Error("The XUI file at {0} doesn't exist, returning false.", FilePath);
                    return false;
                }

                Logger?.Here().Information("Reading XUI file at {0}", FilePath);
                using (Reader = new BinaryReader(File.OpenRead(FilePath)))
                {
                    
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger?.Here().Error("Caught an exception when trying to read XUI file at {0}, returning false. The exception is: {1}", FilePath, ex);
                return false;
            }
        }
    }
}
