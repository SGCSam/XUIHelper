using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public static class XUR5ReadExtensions
    {
        public static XUProperty? TryReadProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                switch(propertyDefinition.Type)
                {
                    case XUPropertyDefinitionTypes.Bool:
                    {
                        return TryReadBoolProperty(xur, reader, propertyDefinition);
                    }
                    case XUPropertyDefinitionTypes.Integer: 
                    {
                        return TryReadIntegerProperty(xur, reader, propertyDefinition);
                    }
                    case XUPropertyDefinitionTypes.Unsigned:
                    {
                        return TryReadUnsignedProperty(xur, reader, propertyDefinition);
                    }
                    case XUPropertyDefinitionTypes.String:
                    {
                        return TryReadStringProperty(xur, reader, propertyDefinition);
                    }
                    case XUPropertyDefinitionTypes.Float:
                    {
                        return TryReadFloatProperty(xur, reader, propertyDefinition);
                    }
                    case XUPropertyDefinitionTypes.Vector:
                    {
                        return TryReadVectorProperty(xur, reader, propertyDefinition);
                    }
                    default:
                    {
                        xur.Logger?.Here().Error("Unhandled property type of {0} when reading property {1}, returning null.", propertyDefinition.Type, propertyDefinition.Name);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUProperty? TryReadBoolProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if(propertyDefinition.Type != XUPropertyDefinitionTypes.Bool)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not bool, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                bool val = reader.ReadByte() > 0 ? true : false;
                xur.Logger?.Here().Verbose("Read {0} boolean property value of {1}.", propertyDefinition.Name, val);
                return new XUProperty(propertyDefinition, val);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading bool property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUProperty? TryReadIntegerProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Integer)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not integer, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                int val = reader.ReadInt32BE();
                xur.Logger?.Here().Verbose("Read {0} integer property value of {1}.", propertyDefinition.Name, val);
                return new XUProperty(propertyDefinition, val);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading integer property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUProperty? TryReadUnsignedProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Unsigned)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not unsigned, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                uint val = reader.ReadUInt32BE();
                xur.Logger?.Here().Verbose("Read {0} unsigned property value of {1}.", propertyDefinition.Name, val);
                return new XUProperty(propertyDefinition, val);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading unsigned property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUProperty? TryReadStringProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.String)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not string, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                int stringIndex = reader.ReadInt16BE() - 1;
                xur.Logger?.Here()?.Verbose("Reading string, got string index of {0}", stringIndex);

                if (stringIndex <= 0x00)
                {
                    //This is VALID, NOT a bug/hack. Timelines who have animated string properties use index 0 to indicate an empty string.
                    //We may animate the Visual property for example - 1 keyframe may have no visual, the next may have a different visual
                    //So we can use 0x00 index instead of storing a blank string in the STRN table

                    //We also use less than, since the zero based index method of -1 takes does 0 - 1, so we'll get a negative number

                    xur.Logger?.Here()?.Verbose("String index was 0, returning empty string.");
                    return new XUProperty(propertyDefinition, string.Empty);
                }

                ISTRNSection? strnSection = ((IXUR)xur).TryFindXURSectionByMagic<ISTRNSection>(ISTRNSection.ExpectedMagic);
                if (strnSection == null)
                {
                    xur.Logger?.Here().Error("STRN section was null, returning null.");
                    return null;
                }

                if (strnSection.Strings.Count == 0 || strnSection.Strings.Count <= stringIndex)
                {
                    xur.Logger?.Here().Error("Failed to read string as we got an invalid index of {0}. The strings length is {1}. Returning null.", stringIndex, strnSection.Strings.Count);
                    return null;
                }

                string val = strnSection.Strings[stringIndex];
                xur.Logger?.Here().Verbose("Read string value of {0}.", val);
                return new XUProperty(propertyDefinition, val);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading string property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUProperty? TryReadFloatProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Float)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not float, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                float val = reader.ReadSingleBE();
                xur.Logger?.Here().Verbose("Read {0} float property value of {1}.", propertyDefinition.Name, val);
                return new XUProperty(propertyDefinition, val);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading float property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUProperty? TryReadVectorProperty(this XUR5 xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Vector)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not vector, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                int vectIndex = reader.ReadInt32BE();
                xur.Logger?.Here()?.Verbose("Reading vector, got vector index of {0}", vectIndex);

                IVECTSection? vectSection = ((IXUR)xur).TryFindXURSectionByMagic<IVECTSection>(IVECTSection.ExpectedMagic);
                if (vectSection == null)
                {
                    xur.Logger?.Here().Error("VECT section was null, returning null.");
                    return null;
                }

                if (vectSection.Vectors.Count == 0 || vectSection.Vectors.Count <= vectIndex)
                {
                    xur.Logger?.Here().Error("Failed to read vector as we got an invalid index of {0}. The vectors length is {1}. Returning null.", vectSection, vectSection.Vectors.Count);
                    return null;
                }

                XUVector val = vectSection.Vectors[vectIndex];
                xur.Logger?.Here().Verbose("Read vector value of {0}.", val);
                return new XUProperty(propertyDefinition, val);
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading vector property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }
    }
}
