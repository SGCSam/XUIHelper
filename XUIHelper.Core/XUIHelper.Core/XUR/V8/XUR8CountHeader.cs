using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class XUR8CountHeader : IXURCountHeader
    {

        public int TotalObjectsCount { get; private set; }
        public int TotalUnsharedObjectPropertiesCount { get; private set; }
        public int SharedPropertiesArrayCount { get; private set; }
        public int Unknown { get; private set; }
        public int SharedCompoundPropertiesArrayCount { get; private set; }
        public int TotalKeyframePropertyClassDepth { get; private set; }
        public int TotalTimelinePropertyClassDepth { get; private set; }
        public int TimelinesCount { get; private set; }
        public int KeyframePropertiesCount { get; private set; }
        public int KeyframeDataCount { get; private set; }
        public int NamedFramesCount { get; private set; }
        public int ObjectsWithChildrenCount { get; private set; }

        //TODO: Fix unknown integer

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(XUR8CountHeader));
                xur.Logger?.Here().Verbose("Reading XUR8 count header.");

                TotalObjectsCount = (int)reader.ReadPackedUInt();
                xur.Logger?.Here().Verbose("Read a total objects count of {0:X8}.", TotalObjectsCount);

                TotalUnsharedObjectPropertiesCount = (int)reader.ReadPackedUInt();
                xur.Logger?.Here().Verbose("Read total unshared properties count of {0:X8}.", TotalUnsharedObjectPropertiesCount);

                SharedPropertiesArrayCount = (int)reader.ReadPackedUInt();
                xur.Logger?.Here().Verbose("Read shared properties array count of {0:X8}.", SharedPropertiesArrayCount);

                Unknown = (int)reader.ReadPackedUInt();
                xur.Logger?.Here().Verbose("Read unknown of {0:X8}.", Unknown);

                SharedCompoundPropertiesArrayCount = (int)reader.ReadPackedUInt();
                xur.Logger?.Here().Verbose("Read shared compound properties array count of {0:X8}.", SharedCompoundPropertiesArrayCount);

                TotalKeyframePropertyClassDepth = (int)reader.ReadPackedUInt();
                xur.Logger?.Here().Verbose("Read a total keyframe property of {0:X8}.", TotalKeyframePropertyClassDepth);

                TotalTimelinePropertyClassDepth = (int)reader.ReadPackedUInt();
                xur.Logger?.Here().Verbose("Read a total timeline property of {0:X8}.", TotalTimelinePropertyClassDepth);

                TimelinesCount = (int)reader.ReadPackedUInt();
                xur.Logger?.Here().Verbose("Read timelines count of {0:X8}.", TimelinesCount);

                KeyframePropertiesCount = (int)reader.ReadPackedUInt();
                xur.Logger?.Here().Verbose("Read keyframe properties count of {0:X8}.", KeyframePropertiesCount);

                KeyframeDataCount = (int)reader.ReadPackedUInt();
                xur.Logger?.Here().Verbose("Read keyframe data count of {0:X8}.", KeyframeDataCount);

                NamedFramesCount = (int)reader.ReadPackedUInt();
                xur.Logger?.Here().Verbose("Read named frames count of {0:X8}.", NamedFramesCount);

                ObjectsWithChildrenCount = (int)reader.ReadPackedUInt();
                xur.Logger?.Here().Verbose("Read objects with children count of {0:X8}.", ObjectsWithChildrenCount);

                xur.Logger?.Here().Verbose("XUR8 count header read successful!");
                return true;

            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading XUR8 count header, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public bool TryVerify(IXUR xur)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(XUR8CountHeader));
                xur.Logger?.Here().Verbose("Verifying XUR8 count header.");

                XUR8? xur8 = xur as XUR8;
                if(xur8 == null)
                {
                    xur.Logger?.Here().Error("XUR was not XUR8, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Trying to find data section...");
                IDATASection? section = xur.TryFindXURSectionByMagic<IDATASection>(IDATASection.ExpectedMagic);
                if (section == null)
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
                if (TotalObjectsCount != objCount)
                {
                    xur.Logger?.Here().Error("Mismatch between the total objects count, returning false. Expected: {0}, Actual: {1}", TotalObjectsCount, objCount);
                    return false;
                }

                xur.Logger?.Here().Verbose("Verifying total unshared properties count.");
                int totalUnsharedObjectPropertiesCount = rootObject.GetTotalUnsharedPropertiesCount();
                if (TotalUnsharedObjectPropertiesCount != totalUnsharedObjectPropertiesCount)
                {
                    xur.Logger?.Here().Error("Mismatch between the total unshared object properties count, returning false. Expected: {0}, Actual: {1}", TotalUnsharedObjectPropertiesCount, totalUnsharedObjectPropertiesCount);
                    return false;
                }

                xur.Logger?.Here().Verbose("Verifying shared properties array count.");
                int sharedPropArrayCount = xur8.ReadPropertiesLists.Count;
                if (SharedPropertiesArrayCount != sharedPropArrayCount)
                {
                    xur.Logger?.Here().Error("Mismatch between the shared properties array count, returning false. Expected: {0}, Actual: {1}", SharedPropertiesArrayCount, sharedPropArrayCount);
                    return false;
                }

                xur.Logger?.Here().Verbose("Verifying shared compound properties array count.");
                int sharedCompoundPropArrayCount = xur8.CompoundPropertyDatas.Count;
                if (SharedCompoundPropertiesArrayCount != sharedCompoundPropArrayCount)
                {
                    xur.Logger?.Here().Error("Mismatch between the shared properties array count, returning false. Expected: {0}, Actual: {1}", SharedCompoundPropertiesArrayCount, sharedCompoundPropArrayCount);
                    return false;
                }

                xur.Logger?.Here().Verbose("Verifying total keyframe property class depth.");
                int? totalKeyframePropertyClassDepth = rootObject.TryGetTotalKeyframePropertyDefinitionsClassDepth(0x8);
                if (TotalKeyframePropertyClassDepth != totalKeyframePropertyClassDepth)
                {
                    xur.Logger?.Here().Error("Mismatch between the total keyframe property class depth, returning false. Expected: {0}, Actual: {1}", TotalKeyframePropertyClassDepth, totalKeyframePropertyClassDepth);
                    return false;
                }

                xur.Logger?.Here().Verbose("Verifying total timeline property class depth.");
                int totalTimelinePropertyClassDepth = rootObject.GetTotalTimelinePropertyDefinitionsClassDepth();
                if (TotalTimelinePropertyClassDepth != totalTimelinePropertyClassDepth)
                {
                    xur.Logger?.Here().Error("Mismatch between the total timeline property class depth, returning false. Expected: {0}, Actual: {1}", TotalTimelinePropertyClassDepth, totalTimelinePropertyClassDepth);
                    return false;
                }

                xur.Logger?.Here().Verbose("Verifying timelines count.");
                int timelinesCount = rootObject.GetTimelinesCount();
                if (TimelinesCount != timelinesCount)
                {
                    xur.Logger?.Here().Error("Mismatch between the timelines count, returning false. Expected: {0}, Actual: {1}", TimelinesCount, timelinesCount);
                    return false;
                }

                xur.Logger?.Here().Verbose("Verifying keyframe properties count.");
                IKEYPSection? keypSection = ((IXUR)xur).TryFindXURSectionByMagic<IKEYPSection>(IKEYPSection.ExpectedMagic);
                if (keypSection == null)
                {
                    xur.Logger?.Here().Error("Failed to find IKEYP section, returning false.");
                    return false;
                }

                int keyframePropertiesCount = keypSection.PropertyIndexes.Count;
                if (KeyframePropertiesCount != keyframePropertiesCount)
                {
                    xur.Logger?.Here().Error("Mismatch between the keyframe properties count, returning false. Expected: {0}, Actual: {1}", KeyframePropertiesCount, keyframePropertiesCount);
                    return false;
                }

                xur.Logger?.Here().Verbose("Verifying keyframe data count.");
                IKEYDSection? keydSection = ((IXUR)xur).TryFindXURSectionByMagic<IKEYDSection>(IKEYDSection.ExpectedMagic);
                if (keydSection == null)
                {
                    xur.Logger?.Here().Error("Failed to find IKEYD section, returning false.");
                    return false;
                }

                int keyframeDataCount = keydSection.Keyframes.Count;
                if (KeyframeDataCount != keyframeDataCount)
                {
                    xur.Logger?.Here().Error("Mismatch between the keyframe data count, returning false. Expected: {0}, Actual: {1}", KeyframeDataCount, keyframeDataCount);
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

                xur.Logger?.Here().Verbose("XUR8 count header verified successfully!");
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when verifying XUR8 count header, returning false. The exception is: {0}", ex);
                return false;
            }
        }
    }
}
