using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public interface IXUIHelperProgressable
    {
        int TotalWorkCount { get; set; }
        int SuccessfulWorkCount { get; set; }
        int FailedWorkCount { get; set; }
        int CompletedWorkCount { get { return SuccessfulWorkCount + FailedWorkCount; } }
        float Progress { get { return (CompletedWorkCount / (float)TotalWorkCount) * 100.0f; } }
        bool IsIndeterminate { get; set; }
        string Description { get; set; }
    }
}
