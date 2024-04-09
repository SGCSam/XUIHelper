using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public interface IXUIHelperProgressable
    {
        float Progress { get; set; }
        bool IsIndeterminate { get; set; }
        string Description { get; set; }
    }
}
