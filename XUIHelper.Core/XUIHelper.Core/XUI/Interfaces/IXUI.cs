using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public interface IXUI
    {
        ILogger? Logger { get; set; }
        string FilePath { get; }

        public Task<bool> TryReadAsync();
        public Task<bool> TryWriteAsync(XUObject rootObject);
    }
}
