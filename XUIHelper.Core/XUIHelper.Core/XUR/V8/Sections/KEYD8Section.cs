using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class KEYD8Section : IKEYDSection
    {
        public int Magic { get { return IKEYDSection.ExpectedMagic; } }

        public List<XURKeyframe> Keyframes { get; private set; } = new List<XURKeyframe>();

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(KEYD8Section));
                xur.Logger?.Here().Verbose("Reading KEYD8 section.");

                XURSectionTableEntry? entry = xur.TryGetXURSectionTableEntryForMagic(IKEYDSection.ExpectedMagic);
                if (entry == null)
                {
                    xur.Logger?.Here().Error("XUR section table entry was null, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Reading keyframe data from offset {0:X8}.", entry.Offset);
                reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

                int dataIndex = 0;
                for (int bytesRead = 0; bytesRead < entry.Length;)
                {
                    byte readFrameBytes;
                    int frame = (int)reader.ReadPackedUInt(out readFrameBytes);
                    bytesRead += readFrameBytes;

                    byte flagByte = reader.ReadByte();
                    bytesRead++;

                    byte flags = (byte)(flagByte & 0x3F);
                    int unknown = ((byte)(flagByte >> 6));
                    if(unknown != 0)
                    {
                        xur.Logger?.Here().Verbose("Read an unknown of {0}", unknown);
                    }

                    byte easeIn = 0;
                    byte easeOut = 0;
                    byte easeScale = 0;
                    XUKeyframeInterpolationTypes interpolationType = XUKeyframeInterpolationTypes.Linear;
                    int vectorIndex = 0;

                    if(flags != 0x0)
                    {
                        if (flags == 0x1)
                        {
                            interpolationType = XUKeyframeInterpolationTypes.None;
                        }
                        else if (flags == 0x2)
                        {
                            easeIn = reader.ReadByte();
                            easeOut = reader.ReadByte();
                            easeScale = reader.ReadByte();
                            interpolationType = XUKeyframeInterpolationTypes.Ease;
                            bytesRead += 3;
                        }
                        else if (flags == 0xA)
                        {
                            byte readVectorIndexBytes;
                            vectorIndex = (int)reader.ReadPackedUInt(out readVectorIndexBytes);
                            bytesRead += readVectorIndexBytes;
                        }
                        else
                        {
                            xur.Logger?.Here().Error("Unknown flag of {0:X8}.", flags);
                        }
                    }

                    byte readPropertyIndexBytes;
                    int propertyIndex = (int)reader.ReadPackedUInt(out readPropertyIndexBytes);
                    bytesRead += readPropertyIndexBytes;

                    XURKeyframe readKeyframe = new XURKeyframe(frame, interpolationType, easeIn, easeOut, easeScale, vectorIndex, propertyIndex);
                    Keyframes.Add(readKeyframe);
                    xur.Logger?.Here().Verbose("Read keyframe data index {0} as {1}.", dataIndex, readKeyframe);
                    dataIndex++;
                }

                xur.Logger?.Here().Verbose("Read keyframe data successfully, read a total of {0} keyframes", Keyframes.Count);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading KEYD8 section, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public async Task<bool> TryBuildAsync(IXUR xur, XUObject xuObject)
        {
            try
            {
                xur.Logger?.Here().Verbose("Building KEYD8 keyframes.");
                List<XURKeyframe> builtKeyframes = new List<XURKeyframe>();

                if (!TryBuildKeyframesFromObject(xur, xuObject, ref builtKeyframes))
                {
                    xur.Logger?.Here().Error("Failed to build keyframes, returning null.");
                    return false;
                }

                Keyframes = builtKeyframes.ToList();
                xur.Logger?.Here().Verbose("Built a total of {0} KEYD8 keyframes successfully!", Keyframes.Count);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when building KEYD8 section, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        private bool TryBuildKeyframesFromObject(IXUR xur, XUObject xuObject, ref List<XURKeyframe> builtKeyframes)
        {
            try
            {
                IKEYPSection? keyp = xur.TryFindXURSectionByMagic<IKEYPSection>(IKEYPSection.ExpectedMagic);
                if(keyp == null)
                {
                    xur.Logger?.Here().Error("Failed to find KEYP section, returning false.");
                    return false;
                }

                foreach (XUObject childObject in xuObject.Children)
                {
                    if (!TryBuildKeyframesFromObject(xur, childObject, ref builtKeyframes))
                    {
                        xur.Logger?.Here().Error("Failed to get keyframes for child {0}, returning false.", childObject.ClassName);
                        return false;
                    }
                }

                HashSet<int> foundIndexes = new HashSet<int>();
                foreach (XUTimeline childTimeline in xuObject.Timelines)
                {
                    List<XURKeyframe> builtXURKeyframes = new List<XURKeyframe>();
                    foreach (XUKeyframe childKeyframe in childTimeline.Keyframes)
                    {
                        List<uint>? propertyIndexes = KEYP8Section.TryGetKeyframePropertyIndexes(xur, childKeyframe);
                        if (propertyIndexes == null)
                        {
                            xur.Logger?.Here().Error("Failed to get property indexes, returning false.");
                            return false;
                        }

                        int groupIndex = 0;
                        foreach(List<uint> groupedIndexes in keyp.GroupedPropertyIndexes)
                        {
                            if(groupedIndexes.SequenceEqual(propertyIndexes))
                            {
                                XURKeyframe xurKeyframe = new XURKeyframe(childKeyframe, 0, groupIndex);
                                builtXURKeyframes.Add(xurKeyframe);
                                break;
                            }

                            groupIndex += groupedIndexes.Count;
                        }
                    }

                    int existingIndex = builtKeyframes.GetSequenceIndex(builtXURKeyframes);
                    if (existingIndex != -1)
                    {
                        xur.Logger?.Here().Error("Found an existing sequence of keyframes at index {0}, won't re-add.", existingIndex);
                        continue;
                    }

                    xur.Logger?.Here().Error("Adding a sequence of {0} keyframes for a new total of {1}. Added:\n{2}", builtXURKeyframes.Count, Keyframes.Count + builtXURKeyframes.Count, string.Join("\n", builtXURKeyframes));
                    builtKeyframes.AddRange(builtXURKeyframes);
                }

                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to build KEYD8 keyframes for object {0}, returning false. The exception is: {1}", xuObject.ClassName, ex);
                return false;
            }
        }

        public async Task<int?> TryWriteAsync(IXUR xur, XUObject xuObject, BinaryWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
