﻿using Serilog;
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
        public int SharedCompoundPropertiesCount { get; private set; }
        public int SharedCompoundPropertiesArrayCount { get; private set; }
        public int TotalKeyframePropertyClassDepth { get; private set; }
        public int TotalTimelinePropertyClassDepth { get; private set; }
        public int TimelinesCount { get; private set; }
        public int KeyframePropertiesCount { get; private set; }
        public int KeyframeDataCount { get; private set; }
        public int NamedFramesCount { get; private set; }
        public int ObjectsWithChildrenCount { get; private set; }

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

                SharedCompoundPropertiesCount = (int)reader.ReadPackedUInt();
                xur.Logger?.Here().Verbose("Read shared compound properties count of {0:X8}.", SharedCompoundPropertiesCount);

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

                xur.Logger?.Here().Verbose("Verifying shared compound properties count.");
                int sharedCompoundPropCount = xur8.GetSharedCompoundPropertiesCount();
                if (SharedCompoundPropertiesCount != sharedCompoundPropCount && false)
                {
                    xur.Logger?.Here().Error("Mismatch between the shared properties count, returning false. Expected: {0}, Actual: {1}", SharedCompoundPropertiesCount, sharedCompoundPropCount);
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
                int? totalKeyframePropertyClassDepth = rootObject.TryGetTotalKeyframePropertyDefinitionsClassDepth();
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
                int keyframePropertiesCount = 0;
                if (keypSection != null)
                {
                    keyframePropertiesCount = keypSection.PropertyIndexes.Count;
                }
                
                if (KeyframePropertiesCount != keyframePropertiesCount)
                {
                    xur.Logger?.Here().Error("Mismatch between the keyframe properties count, returning false. Expected: {0}, Actual: {1}", KeyframePropertiesCount, keyframePropertiesCount);
                    return false;
                }

                xur.Logger?.Here().Verbose("Verifying keyframe data count.");
                IKEYDSection? keydSection = ((IXUR)xur).TryFindXURSectionByMagic<IKEYDSection>(IKEYDSection.ExpectedMagic);
                int keyframeDataCount = 0;
                if (keydSection != null)
                {
                    keyframeDataCount = keydSection.Keyframes.Count;
                }

                if (KeyframeDataCount != keyframeDataCount)
                {
                    xur.Logger?.Here().Error("Mismatch between the keyframe data count, returning false. Expected: {0}, Actual: {1}", KeyframeDataCount, keyframeDataCount);
                    return false;
                }

                xur.Logger?.Here().Verbose("Verifying named frames count.");
                INAMESection? nameSection = ((IXUR)xur).TryFindXURSectionByMagic<INAMESection>(INAMESection.ExpectedMagic);
                int namedFramesCount = 0;
                if (nameSection != null)
                {
                    namedFramesCount = nameSection.NamedFrames.Count;
                }

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

        public async Task<int?> TryWriteAsync(IXUR xur, BinaryWriter writer, XUObject rootObject)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(XUR8CountHeader));
                xur.Logger?.Here().Verbose("Writing XUR8 count header.");

                if (xur is not XUR8 xur8)
                {
                    xur.Logger?.Here().Error("XUR was not XUR8, returning null.");
                    return null;
                }

                int bytesWritten = 0;

                int objCount = rootObject.GetTotalObjectsCount();
                int objCountBytesWritten = 0;
                writer.WritePackedUInt((uint)objCount, out objCountBytesWritten);
                xur.Logger?.Here().Verbose("Wrote total objects count of {0:X8}, {1} bytes.", objCount, objCountBytesWritten);
                bytesWritten += objCountBytesWritten;

                int totalUnsharedPropertiesCount = rootObject.GetTotalUnsharedPropertiesCount();
                int totalUnsharedPropertiesCountBytesWritten = 0;
                writer.WritePackedUInt((uint)totalUnsharedPropertiesCount, out totalUnsharedPropertiesCountBytesWritten);
                xur.Logger?.Here().Verbose("Wrote total unshared properties count of {0:X8}, {1} bytes.", totalUnsharedPropertiesCount, totalUnsharedPropertiesCountBytesWritten);
                bytesWritten += totalUnsharedPropertiesCountBytesWritten;

                int sharedPropertiesArrayCount = xur8.ReadPropertiesLists.Count;
                int sharedPropertiesArrayCountBytesWritten = 0;
                writer.WritePackedUInt((uint)sharedPropertiesArrayCount, out sharedPropertiesArrayCountBytesWritten);
                xur.Logger?.Here().Verbose("Wrote shared properties array count of {0:X8}, {1} bytes.", sharedPropertiesArrayCount, sharedPropertiesArrayCountBytesWritten);
                bytesWritten += sharedPropertiesArrayCountBytesWritten;

                int sharedCompoundPropertiesCount = xur8.GetSharedCompoundPropertiesCount();
                int sharedCompoundPropertiesCountBytesWritten = 0;
                writer.WritePackedUInt((uint)sharedCompoundPropertiesCount, out sharedCompoundPropertiesCountBytesWritten);
                xur.Logger?.Here().Verbose("Wrote shared compound properties count of {0:X8}, {1} bytes.", sharedCompoundPropertiesCount, sharedCompoundPropertiesCountBytesWritten);
                bytesWritten += sharedCompoundPropertiesCountBytesWritten;

                int sharedCompoundPropertiesArrayCount = xur8.CompoundPropertyDatas.Count;
                int sharedCompoundPropertiesArrayCountBytesWritten = 0;
                writer.WritePackedUInt((uint)sharedCompoundPropertiesArrayCount, out sharedCompoundPropertiesArrayCountBytesWritten);
                xur.Logger?.Here().Verbose("Wrote shared compound properties array count of {0:X8}, {1} bytes.", sharedCompoundPropertiesArrayCount, sharedCompoundPropertiesArrayCountBytesWritten);
                bytesWritten += sharedCompoundPropertiesArrayCountBytesWritten;

                int? totalKeyframePropertyClassDepth = rootObject.TryGetTotalKeyframePropertyDefinitionsClassDepth();
                if (totalKeyframePropertyClassDepth == null)
                {
                    xur.Logger?.Here().Error("Failed to get total keyframe property class depth, returning null.");
                    return null;
                }

                int totalKeyframePropertyClassDepthBytesWritten = 0;
                writer.WritePackedUInt((uint)totalKeyframePropertyClassDepth, out totalKeyframePropertyClassDepthBytesWritten);
                xur.Logger?.Here().Verbose("Wrote total keyframe property class depth of {0:X8}, {1} bytes.", totalKeyframePropertyClassDepth, totalKeyframePropertyClassDepthBytesWritten);
                bytesWritten += totalKeyframePropertyClassDepthBytesWritten;

                int totalTimelinePropertyClassDepth = rootObject.GetTotalTimelinePropertyDefinitionsClassDepth();
                int totalTimelinePropertyClassDepthBytesWritten = 0;
                writer.WritePackedUInt((uint)totalTimelinePropertyClassDepth, out totalTimelinePropertyClassDepthBytesWritten);
                xur.Logger?.Here().Verbose("Wrote total timeline property class depth of {0:X8}, {1} bytes.", totalTimelinePropertyClassDepth, totalTimelinePropertyClassDepthBytesWritten);
                bytesWritten += totalTimelinePropertyClassDepthBytesWritten;

                int timelinesCount = rootObject.GetTimelinesCount();
                int timelinesCountBytesWritten = 0;
                writer.WritePackedUInt((uint)timelinesCount, out timelinesCountBytesWritten);
                xur.Logger?.Here().Verbose("Wrote timelines count of {0:X8}, {1} bytes.", timelinesCount, timelinesCountBytesWritten);
                bytesWritten += timelinesCountBytesWritten;

                int keyframePropertiesCount = 0;
                IKEYPSection? keypSection = ((IXUR)xur).TryFindXURSectionByMagic<IKEYPSection>(IKEYPSection.ExpectedMagic);
                if (keypSection != null)
                {
                    keyframePropertiesCount = keypSection.PropertyIndexes.Count;
                }
                int keyframePropertiesCountBytesWritten = 0;
                writer.WritePackedUInt((uint)keyframePropertiesCount, out keyframePropertiesCountBytesWritten);
                xur.Logger?.Here().Verbose("Wrote keyframe properties count of {0:X8}, {1} bytes.", timelinesCount, keyframePropertiesCountBytesWritten);
                bytesWritten += keyframePropertiesCountBytesWritten;

                int keyframeDataCount = 0;
                IKEYDSection? keydSection = ((IXUR)xur).TryFindXURSectionByMagic<IKEYDSection>(IKEYDSection.ExpectedMagic);
                if (keydSection != null)
                {
                    keyframeDataCount = keydSection.Keyframes.Count;
                }
                int keyframeDataCountBytesWritten = 0;
                writer.WritePackedUInt((uint)keyframeDataCount, out keyframeDataCountBytesWritten);
                xur.Logger?.Here().Verbose("Wrote keyframe data count of {0:X8}, {1} bytes.", keyframeDataCount, keyframeDataCountBytesWritten);
                bytesWritten += keyframeDataCountBytesWritten;

                int namedFramesCount = rootObject.GetNamedFramesCount();
                int namedFramesCountBytesWritten = 0;
                writer.WritePackedUInt((uint)namedFramesCount, out namedFramesCountBytesWritten);
                xur.Logger?.Here().Verbose("Wrote named frames count of {0:X8}, {1} bytes.", namedFramesCount, namedFramesCountBytesWritten);
                bytesWritten += namedFramesCountBytesWritten;

                int objectsWithChildrenCount = rootObject.GetObjectsWithChildrenCount();
                int objectsWithChildrenCountBytesWritten = 0;
                writer.WritePackedUInt((uint)objectsWithChildrenCount, out objectsWithChildrenCountBytesWritten);
                xur.Logger?.Here().Verbose("Wrote named frames count of {0:X8}, {1} bytes.", objectsWithChildrenCount, objectsWithChildrenCountBytesWritten);
                bytesWritten += objectsWithChildrenCountBytesWritten;

                xur.Logger?.Here().Verbose("Wrote XUR8 count header with a total of {0:X8} bytes successfully!", bytesWritten);
                return bytesWritten;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing XUR8 count header, returning null. The exception is: {0}", ex);
                return null;
            }
        }
    }
}
