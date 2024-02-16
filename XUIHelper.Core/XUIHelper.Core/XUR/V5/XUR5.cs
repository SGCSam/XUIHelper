using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public class XUR5 : IXUR
    {
        private static Serilog.ILogger Log => Serilog.Log.ForContext(typeof(XUR5));
    }
}
