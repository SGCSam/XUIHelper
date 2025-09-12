using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class XUR5CountHeader : IXURCountHeader
    {
        public int TotalObjectsCount { get; private set; }
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

                TotalObjectsCount = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Read a total objects count of {0:X8}.", TotalObjectsCount);

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

        public bool TryVerify(IXUR xur)
        {
            try
            {
                return true;

                xur.Logger = xur.Logger?.ForContext(typeof(XUR5CountHeader));
                xur.Logger?.Here().Verbose("Verifying XUR5 count header.");

                xur.Logger?.Here().Verbose("Trying to find data section...");
                IDATASection? section = xur.TryFindXURSectionByMagic<IDATASection>(IDATASection.ExpectedMagic);
                if(section == null)
                {
                    xur.Logger?.Here().Error("Failed to find DATA section, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Found data section, trying to grab root object...");
                XUObject? rootObject = section.RootObject;
                if (rootObject == null)
                {
                    xur.Logger?.Here().Error("The root object was null, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Verifying total objects count.");
                int objCount = rootObject.GetTotalObjectsCount();
                if(TotalObjectsCount != objCount)
                {
                    xur.Logger?.Here().Error("Mismatch between the total objects count, returning false. Expected: {0}, Actual: {1}", TotalObjectsCount, objCount);
                    return false;
                }

                xur.Logger?.Here().Verbose("Verifying total properties count.");
                int totalPropertiesCount = rootObject.GetTotalPropertiesCount();
                if (TotalPropertiesCount != totalPropertiesCount)
                {
                    xur.Logger?.Here().Error("Mismatch between the total properties count, returning false. Expected: {0}, Actual: {1}", TotalPropertiesCount, totalPropertiesCount);
                    return false;
                }

                xur.Logger?.Here().Verbose("Verifying properties array count.");
                int propArrayCount = rootObject.GetPropertiesArrayCount();
                if (TotalPropertiesArrayCount != propArrayCount)
                {
                    xur.Logger?.Here().Error("Mismatch between the properties array count, returning false. Expected: {0}, Actual: {1}", TotalPropertiesArrayCount, propArrayCount);
                    return false;
                }

                xur.Logger?.Here().Verbose("Verifying keyframe properties count.");
                int keyframePropertiesCount = rootObject.GetTotalKeyframePropertiesCount();
                if (KeyframePropertiesCount != keyframePropertiesCount)
                {
                    xur.Logger?.Here().Error("Mismatch between the keyframe properties count, returning false. Expected: {0}, Actual: {1}", KeyframePropertiesCount, keyframePropertiesCount);
                    return false;
                }

                xur.Logger?.Here().Verbose("Verifying total keyframe properties class depth.");
                int? totalKeyframePropertiesClassDepth = rootObject.TryGetTotalKeyframePropertyDefinitionsClassDepth();
                if(totalKeyframePropertiesClassDepth == null)
                {
                    xur.Logger?.Here().Error("The acquired total keyframe properties class depth was null, an error must have occurred, returning false.");
                    return false;
                }

                if (TotalKeyframePropertyClassDepth != totalKeyframePropertiesClassDepth)
                {
                    xur.Logger?.Here().Error("Mismatch between the total keyframe properties class depth, returning false. Expected: {0}, Actual: {1}", TotalKeyframePropertyClassDepth, totalKeyframePropertiesClassDepth);
                    return false;
                }

                xur.Logger?.Here().Verbose("Verifying keyframe property definitions count.");
                int keyframePropertyDefinitionsCount = rootObject.GetKeyframePropertyDefinitionsCount();
                if (KeyframePropertyDefinitionsCount != keyframePropertyDefinitionsCount)
                {
                    xur.Logger?.Here().Error("Mismatch between the keyframe property definitions count, returning false. Expected: {0}, Actual: {1}", KeyframePropertyDefinitionsCount, keyframePropertyDefinitionsCount);
                    return false;
                }

                xur.Logger?.Here().Verbose("Verifying keyframes count.");
                int keyframesCount = rootObject.GetKeyframesCount();
                if (KeyframesCount != keyframesCount)
                {
                    xur.Logger?.Here().Error("Mismatch between the keyframes count, returning false. Expected: {0}, Actual: {1}", KeyframesCount, keyframesCount);
                    return false;
                }

                xur.Logger?.Here().Verbose("Verifying timelines count.");
                int timelinesCount = rootObject.GetTimelinesCount();
                if (TimelinesCount != timelinesCount)
                {
                    xur.Logger?.Here().Error("Mismatch between the timelines count, returning false. Expected: {0}, Actual: {1}", TimelinesCount, timelinesCount);
                    return false;
                }

                xur.Logger?.Here().Verbose("Verifying named frames count.");
                int namedFramesCount = rootObject.GetNamedFramesCount();
                if (NamedFramesCount != namedFramesCount)
                {
                    xur.Logger?.Here().Error("Mismatch between the named frames count, returning false. Expected: {0}, Actual: {1}", NamedFramesCount, namedFramesCount);
                    return false;
                }

                xur.Logger?.Here().Verbose("Verifying objects with children count.");
                int objWithChildrenCount = rootObject.GetObjectsWithChildrenCount();
                if (ObjectsWithChildrenCount != objWithChildrenCount)
                {
                    xur.Logger?.Here().Error("Mismatch between the objects with children count, returning false. Expected: {0}, Actual: {1}", ObjectsWithChildrenCount, objWithChildrenCount);
                    return false;
                }

                xur.Logger?.Here().Verbose("XUR5 count header verified successfully!");
                return true;
            }
            catch(Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when verifying XUR5 count header, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public async Task<int?> TryWriteAsync(IXUR xur, BinaryWriter writer, XUObject rootObject)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(XUR5CountHeader));
                xur.Logger?.Here().Verbose("Writing XUR5 count header.");

                int bytesWritten = 0;

                int objCount = rootObject.GetTotalObjectsCount();
                xur.Logger?.Here().Verbose("Writing total objects count of {0:X8}.", objCount);
                writer.WriteInt32BE(objCount);
                bytesWritten += 4;

                int totalPropertiesCount = rootObject.GetTotalPropertiesCount();
                xur.Logger?.Here().Verbose("Writing total properties count of {0:X8}.", totalPropertiesCount);
                writer.WriteInt32BE(totalPropertiesCount);
                bytesWritten += 4;

                int propArrayCount = rootObject.GetPropertiesArrayCount();
                xur.Logger?.Here().Verbose("Writing properties array count of {0:X8}.", propArrayCount);
                writer.WriteInt32BE(propArrayCount);
                bytesWritten += 4;

                int keyframePropertiesCount = rootObject.GetTotalKeyframePropertiesCount();
                xur.Logger?.Here().Verbose("Writing keyframe properties count of {0:X8}.", keyframePropertiesCount);
                writer.WriteInt32BE(keyframePropertiesCount);
                bytesWritten += 4;

                int? totalKeyframePropertiesClassDepth = rootObject.TryGetTotalKeyframePropertyDefinitionsClassDepth();
                if(totalKeyframePropertiesClassDepth == null)
                {
                    xur.Logger?.Here().Error("Failed to get total keyframe property definitions class depth, returning null.");
                    return null;
                }

                xur.Logger?.Here().Verbose("Writing total keyframe properties class depth of {0:X8}.", totalKeyframePropertiesClassDepth);
                writer.WriteInt32BE(totalKeyframePropertiesClassDepth.Value);
                bytesWritten += 4;

                int keyframePropertyDefinitionsCount = rootObject.GetKeyframePropertyDefinitionsCount();
                xur.Logger?.Here().Verbose("Writing keyframe property definitions count of {0:X8}.", keyframePropertyDefinitionsCount);
                writer.WriteInt32BE(keyframePropertyDefinitionsCount);
                bytesWritten += 4;

                int keyframesCount = rootObject.GetKeyframesCount();
                xur.Logger?.Here().Verbose("Writing keyframes count of {0:X8}.", keyframesCount);
                writer.WriteInt32BE(keyframesCount);
                bytesWritten += 4;

                int timelinesCount = rootObject.GetTimelinesCount();
                xur.Logger?.Here().Verbose("Writing timelines count of {0:X8}.", timelinesCount);
                writer.WriteInt32BE(timelinesCount);
                bytesWritten += 4;

                int namedFramesCount = rootObject.GetNamedFramesCount();
                xur.Logger?.Here().Verbose("Writing named frames count of {0:X8}.", namedFramesCount);
                writer.WriteInt32BE(namedFramesCount);
                bytesWritten += 4;

                int objWithChildrenCount = rootObject.GetObjectsWithChildrenCount();
                xur.Logger?.Here().Verbose("Writing objects with children count of {0:X8}.", objWithChildrenCount);
                writer.WriteInt32BE(objWithChildrenCount);
                bytesWritten += 4;

                xur.Logger?.Here().Verbose("Wrote XUR5 count header with a total of {0:X8} bytes successfully!", bytesWritten);
                return bytesWritten;
            }
            catch(Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing XUR5 count header, returning null. The exception is: {0}", ex);
                return null;
            }
        }
    }
}
