using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public interface INAMESection : IXURSection
    {
        const int ExpectedMagic = 0x4E414D45;

        List<XUNamedFrame> NamedFrames { get; }

        HashSet<int> HandledBaseIndexes { get; }

        public int? TryGetBaseIndex(List<XUNamedFrame> frames, ILogger? logger = null)
        {
            int indexToCheck = 0;
            while(true)
            {
                int baseIndex = NamedFrames.GetSequenceIndex(frames, indexToCheck);
                if (baseIndex == -1)
                {
                    logger?.Here().Error("Failed to get base index for named frames, returning null.");
                    return null;
                }
                else if(!HandledBaseIndexes.Contains(baseIndex))
                {
                    HandledBaseIndexes.Add(baseIndex);
                    return baseIndex;
                }
                else
                {
                    indexToCheck = baseIndex + 1;
                }
            }
        }
    }
}
