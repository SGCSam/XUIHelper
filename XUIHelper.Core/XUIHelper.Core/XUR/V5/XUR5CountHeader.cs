using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class XUR5CountHeader : IXURHeader
    {
        public int ObjectsCount { get; private set; }
        public int TotalPropertiesCount { get; private set; }
        public int TotalPropertiesArrayCount { get; private set; }
        public int KeyframePropertiesCount { get; private set; }
        public int TotalKeyframePropertyClassDepth { get; private set; }
        public int KeyframePropertyDefinitionsCount { get; private set; }
        public int KeyframesCount { get; private set; }
        public int TimelinesCount { get; private set; }
        public int NamedFramesCount { get; private set; }
        public int ObjectsWithChildrenCount { get; private set; }

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(XUR5CountHeader));

                xur.Logger?.Here().Verbose("Reading XUR5 count header.");

                ObjectsCount = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Read an objects count of {0:X8}.", ObjectsCount);

                TotalPropertiesCount = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Read total properties count of {0:X8}.", TotalPropertiesCount);

                TotalPropertiesArrayCount = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Read total properties array count of {0:X8}.", TotalPropertiesArrayCount);

                KeyframePropertiesCount = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Read keyframe properties count of {0:X8}.", KeyframePropertiesCount);

                TotalKeyframePropertyClassDepth = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Read a total keyframe property of {0:X8}.", TotalKeyframePropertyClassDepth);

                KeyframePropertyDefinitionsCount = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Read a keyframe property definitions count of {0:X8}.", KeyframePropertyDefinitionsCount);

                KeyframesCount = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Read keyframes count of {0:X8}.", KeyframesCount);

                TimelinesCount = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Read timelines count of {0:X8}.", TimelinesCount);

                NamedFramesCount = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Read named frames count of {0:X8}.", NamedFramesCount);

                ObjectsWithChildrenCount = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Read objects with children count of {0:X8}.", ObjectsWithChildrenCount);

                xur.Logger?.Here().Verbose("XUR5 count header read successful!");
                return true;

            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading XUR5 count header, returning false. The exception is: {0}", ex);
                return false;
            }
        }
    }
}
