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
            int foundBaseIndex = -1;

            while(true)
            {
                int baseIndex = NamedFrames.GetSequenceIndex(frames, indexToCheck);
                if (baseIndex == -1)
                {
                    if(foundBaseIndex != -1)
                    {
                        //NOTE: Seems required for 17559 GuideMain?
                        logger?.Here().Error("Failed to find a sequence index but found a previous base index, falling back to {0}.", foundBaseIndex);
                        return foundBaseIndex;
                    }
                    else
                    {
                        logger?.Here().Error("Failed to get base index for named frames, returning null.");
                        return null;
                    }
                }
                else if(!HandledBaseIndexes.Contains(baseIndex))
                {
                    HandledBaseIndexes.Add(baseIndex);
                    return baseIndex;
                }
                else
                {
                    foundBaseIndex = baseIndex;
                    indexToCheck = baseIndex + 1;
                }
            }
        }
    }
}
