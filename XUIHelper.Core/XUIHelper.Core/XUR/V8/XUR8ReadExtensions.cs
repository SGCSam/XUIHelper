using Serilog;
using Serilog.Core;
using System;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public static class XUR8ReadExtensions
    {
        public static XUProperty? TryReadProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition, bool readIndex = true)
        {
            try
            {
                int indexCount = 1;
                if (readIndex && propertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                {
                    indexCount = (int)reader.ReadByte();
                    xur.Logger?.Here().Verbose("The property {0} is indexed and has an index count of {1}.", propertyDefinition.Name, indexCount);
                }

                List<object> readValues = new List<object>();
                for (int i = 0; i < indexCount; i++)
                {
                    object? value = null;

                    if (indexCount > 1)
                    {
                        xur.Logger?.Here().Verbose("Reading indexed property {0}.", i);
                    }

                    switch (propertyDefinition.Type)
                    {
                        case XUPropertyDefinitionTypes.Bool:
                        {
                            value = TryReadBoolProperty(xur, reader, propertyDefinition);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Integer:
                        {
                            value = TryReadIntegerProperty(xur, reader, propertyDefinition);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Unsigned:
                        {
                            value = TryReadUnsignedProperty(xur, reader, propertyDefinition);
                            break;
                        }
                        case XUPropertyDefinitionTypes.String:
                        {
                            value = TryReadStringProperty(xur, reader, propertyDefinition);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Float:
                        {
                            value = TryReadFloatProperty(xur, reader, propertyDefinition);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Vector:
                        {
                            value = TryReadVectorProperty(xur, reader, propertyDefinition);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Object:
                        {
                            value = TryReadObjectProperty(xur, reader, propertyDefinition);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Colour:
                        {
                            value = TryReadColourProperty(xur, reader, propertyDefinition);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Custom:
                        {
                            value = TryReadCustomProperty(xur, reader, propertyDefinition);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Quaternion:
                        {
                            value = TryReadQuaternionProperty(xur, reader, propertyDefinition);
                            break;
                        }
                        default:
                        {
                            xur.Logger?.Here().Error("Unhandled property type of {0} when reading property {1}, returning null.", propertyDefinition.Type, propertyDefinition.Name);
                            return null;
                        }
                    }

                    if (value == null)
                    {
                        xur.Logger?.Here().Error("Read value was null when reading property {0}, an error must have occurred, returning null.", propertyDefinition.Name);
                        return null;
                    }

                    if (!propertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed))
                    {
                        return new XUProperty(propertyDefinition, value);
                    }
                    else
                    {
                        readValues.Add(value);
                    }
                }

                return new XUProperty(propertyDefinition, readValues);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static bool? TryReadBoolProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Bool)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not bool, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                bool val = reader.ReadByte() > 0 ? true : false;
                xur.Logger?.Here().Verbose("Read boolean property value of {0}.", val);
                return val;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading bool property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryReadIntegerProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Integer)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not integer, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading integer property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static uint? TryReadUnsignedProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Unsigned)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not unsigned, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                uint val = reader.ReadPackedUInt();
                xur.Logger?.Here().Verbose("Read unsigned property value of {0}.", val);
                return val;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading unsigned property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static string? TryReadStringProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.String)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not string, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                int stringIndex = (int)reader.ReadPackedUInt() - 1;
                xur.Logger?.Here()?.Verbose("Reading string, got string index of {0}", stringIndex);

                ISTRNSection? strnSection = ((IXUR)xur).TryFindXURSectionByMagic<ISTRNSection>(ISTRNSection.ExpectedMagic);
                if (strnSection == null)
                {
                    xur.Logger?.Here().Error("STRN section was null, returning null.");
                    return null;
                }

                if (strnSection.Strings.Count == 0 || strnSection.Strings.Count <= stringIndex)
                {
                    xur.Logger?.Here().Error("Failed to read string as we got an invalid index of {0}. The string length is {1}. Returning null.", stringIndex, strnSection.Strings.Count);
                    return null;
                }

                string val = strnSection.Strings[stringIndex];
                xur.Logger?.Here().Verbose("Read string value of {0}.", val);
                return val;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading string property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static float? TryReadFloatProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Float)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not float, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                int floatIndex = (int)reader.ReadPackedUInt();
                xur.Logger?.Here()?.Verbose("Reading float, got float index of {0}", floatIndex);

                IFLOTSection? flotSection = ((IXUR)xur).TryFindXURSectionByMagic<IFLOTSection>(IFLOTSection.ExpectedMagic);
                if (flotSection == null)
                {
                    xur.Logger?.Here().Error("FLOT section was null, returning null.");
                    return null;
                }

                if (flotSection.Floats.Count == 0 || flotSection.Floats.Count <= floatIndex)
                {
                    xur.Logger?.Here().Error("Failed to read float as we got an invalid index of {0}. The floats length is {1}. Returning null.", floatIndex, flotSection.Floats.Count);
                    return null;
                }

                float val = flotSection.Floats[floatIndex];
                xur.Logger?.Here().Verbose("Read float value of {0}.", val);
                return val;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading float property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUVector? TryReadVectorProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Vector)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not vector, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                int vectIndex = (int)reader.ReadPackedUInt();
                xur.Logger?.Here()?.Verbose("Reading vector, got vector index of {0}", vectIndex);

                IVECTSection? vectSection = ((IXUR)xur).TryFindXURSectionByMagic<IVECTSection>(IVECTSection.ExpectedMagic);
                if (vectSection == null)
                {
                    xur.Logger?.Here().Error("VECT section was null, returning null.");
                    return null;
                }

                if (vectSection.Vectors.Count == 0 || vectSection.Vectors.Count <= vectIndex)
                {
                    xur.Logger?.Here().Error("Failed to read vector as we got an invalid index of {0}. The vectors length is {1}. Returning null.", vectIndex, vectSection.Vectors.Count);
                    return null;
                }

                XUVector val = vectSection.Vectors[vectIndex];
                xur.Logger?.Here().Verbose("Read vector value of {0}.", val);
                return val;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading vector property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static List<XUProperty>? TryReadObjectProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Object)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not object, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading object property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUColour? TryReadColourProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Colour)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not colour, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading colour property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUFigure? TryReadCustomProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Custom)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not custom, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading custom property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUQuaternion? TryReadQuaternionProperty(this XUR8 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Quaternion)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not quaternion, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading quaternion property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUNamedFrame? TryReadNamedFrame(this XUR8 xur, int index)
        {
            try
            {
                INAMESection? nameSection = ((IXUR)xur).TryFindXURSectionByMagic<INAMESection>(INAMESection.ExpectedMagic);
                if (nameSection == null)
                {
                    xur.Logger?.Here().Error("NAME section was null, returning null.");
                    return null;
                }

                if (nameSection.NamedFrames.Count == 0 || nameSection.NamedFrames.Count <= index)
                {
                    xur.Logger?.Here().Error("Failed to read named frame as we got an invalid index of {0}. The named frames length is {1}. Returning null.", index, nameSection.NamedFrames.Count);
                    return null;
                }

                XUNamedFrame val = nameSection.NamedFrames[index];
                xur.Logger?.Here().Verbose("Read named frame value of {0}.", val);
                return val;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading named frame, returning null. The exception is: {0}", ex);
                return null;
            }
        }

        public static XUTimeline? TryReadTimeline(this XUR8 xur, BinaryReader reader, XUObject obj)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading timeline, returning null. The exception is: {0}", ex);
                return null;
            }
        }
    }
}
