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
    public class NAME8Section : INAMESection
    {
        public int Magic { get { return INAMESection.ExpectedMagic; } }

        public List<XUNamedFrame> NamedFrames { get; private set; } = new List<XUNamedFrame>();

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(NAME8Section));
                xur.Logger?.Here().Verbose("Reading NAME8 section.");

                XURSectionTableEntry? entry = xur.TryGetXURSectionTableEntryForMagic(INAMESection.ExpectedMagic);
                if (entry == null)
                {
                    xur.Logger?.Here().Error("XUR section table entry was null, returning false.");
                    return false;
                }

                ISTRNSection? strnSection = xur.TryFindXURSectionByMagic<ISTRNSection>(ISTRNSection.ExpectedMagic);
                if (strnSection == null)
                {
                    xur.Logger?.Here().Error("STRN section was null, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Reading named frames from offset {0:X8}.", entry.Offset);
                reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

                for (int bytesRead = 0; bytesRead < entry.Length;)
                {
                    byte readStringIndexBytes;
                    int stringIndex = (int)reader.ReadPackedULong(out readStringIndexBytes);
                    if (stringIndex < 0 || stringIndex >= strnSection.Strings.Count)
                    {
                        xur.Logger?.Here().Error("Read an invalid string index of {0}, it must be between 0 and {1}, returning false.", stringIndex, strnSection.Strings.Count);
                        return false;
                    }
                    string name = strnSection.Strings[stringIndex];
                    bytesRead += readStringIndexBytes;

                    byte readKeyframeBytes;
                    int keyframe = (int)reader.ReadPackedULong(out readKeyframeBytes);
                    bytesRead += readKeyframeBytes;

                    byte namedFrameCommandByte = reader.ReadByte();
                    if (!Enum.IsDefined(typeof(XUNamedFrameCommandTypes), (int)namedFrameCommandByte))
                    {
                        xur.Logger?.Here().Error("Command byte of {0:X8} is not a valid command, returning null.", namedFrameCommandByte);
                        return false;
                    }
                    bytesRead++;

                    XUNamedFrameCommandTypes commandType = (XUNamedFrameCommandTypes)namedFrameCommandByte;
                    if(commandType == XUNamedFrameCommandTypes.GoTo 
                        || commandType == XUNamedFrameCommandTypes.GoToAndPlay 
                        || commandType == XUNamedFrameCommandTypes.GoToAndStop)
                    {
                        byte readTargetStringIndexBytes;
                        int targetStringIndex = (int)reader.ReadPackedULong(out readTargetStringIndexBytes);
                        if (targetStringIndex < 0 || targetStringIndex >= strnSection.Strings.Count)
                        {
                            xur.Logger?.Here().Error("Read an invalid target string index of {0}, it must be between 0 and {1}, returning false.", targetStringIndex, strnSection.Strings.Count);
                            return false;
                        }

                        string target = strnSection.Strings[targetStringIndex];
                        bytesRead += readTargetStringIndexBytes;
                        NamedFrames.Add(new XUNamedFrame(name, (int)keyframe, commandType, target));
                    }
                    else
                    {
                        NamedFrames.Add(new XUNamedFrame(name, (int)keyframe, commandType));
                    }
                }

                xur.Logger?.Here().Verbose("Read named frames successfully, read a total of {0} named frames", NamedFrames.Count);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading NAME8 section, returning false. The exception is: {0}", ex);
                return false;
            }
        }
    }
}
