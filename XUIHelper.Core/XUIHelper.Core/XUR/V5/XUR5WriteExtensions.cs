using Serilog;
using Serilog.Core;
using System;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public static class XUR5WriteExtensions
    {
        public static int? TryWriteProperty(this XUR5 xur, BinaryWriter writer, XUProperty property)
        {
            try
            {
                int? bytesWritten = null;
                switch (property.PropertyDefinition.Type)
                {
                    case XUPropertyDefinitionTypes.Bool:
                    {
                        throw new NotImplementedException();
                    }
                    case XUPropertyDefinitionTypes.Integer:
                    {
                        throw new NotImplementedException();
                    }
                    case XUPropertyDefinitionTypes.Unsigned:
                    {
                        throw new NotImplementedException();
                    }
                    case XUPropertyDefinitionTypes.String:
                    {
                        bytesWritten = TryWriteStringProperty(xur, writer, property.PropertyDefinition, property.Value);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Float:
                    {
                        bytesWritten = TryWriteFloatProperty(xur, writer, property.PropertyDefinition, property.Value);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Vector:
                    {
                        bytesWritten = TryWriteVectorProperty(xur, writer, property.PropertyDefinition, property.Value);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Object:
                    {
                        throw new NotImplementedException();
                    }
                    case XUPropertyDefinitionTypes.Colour:
                    {
                        throw new NotImplementedException();
                    }
                    case XUPropertyDefinitionTypes.Custom:
                    {
                        throw new NotImplementedException();
                    }
                    case XUPropertyDefinitionTypes.Quaternion:
                    {
                        throw new NotImplementedException(); ;
                    }
                    default:
                    {
                        xur.Logger?.Here().Error("Unhandled property type of {0} when writing property {1}, returning null.", property.PropertyDefinition.Type, property.PropertyDefinition.Name);
                        return null;
                    }
                }

                if (bytesWritten == null)
                {
                    xur.Logger?.Here().Error("Bytes written was null when writing property {0}, an error must have occurred, returning null.", property.PropertyDefinition.Name);
                    return null;
                }

                return bytesWritten;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing property {0}, returning null. The exception is: {1}", property.PropertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteStringProperty(this XUR5 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.String)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not string, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                if (val is not string stringVal)
                {
                    xur.Logger?.Here().Error("Property {0} marked as string had a non-string value of {1}, returning null.", propertyDefinition.Name, val);
                    return null;
                }

                ISTRNSection? strnSection = ((IXUR)xur).TryFindXURSectionByMagic<ISTRNSection>(ISTRNSection.ExpectedMagic);
                if(strnSection == null)
                {
                    xur.Logger?.Here().Error("STRN section was null, returning null.");
                    return null;
                }

                short stringIndex = (short)(strnSection.Strings.IndexOf(stringVal) + 1);
                if(stringIndex == 0)
                {
                    xur.Logger?.Here().Error("Failed to get string index for property {0} value {1}, returning null.", propertyDefinition.Name, stringVal);
                    return null;
                }

                writer.WriteInt16BE(stringIndex);
                xur.Logger?.Here().Verbose("Written {0} string property value of {1} as index {2}.", propertyDefinition.Name, stringVal, stringIndex);
                return 4;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing string property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteFloatProperty(this XUR5 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Float)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not float, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                if(val is not float floatVal)
                {
                    xur.Logger?.Here().Error("Property {0} marked as float had a non-float value of {1}, returning null.", propertyDefinition.Name, val);
                    return null;
                }

                writer.WriteSingleBE(floatVal);
                xur.Logger?.Here().Verbose("Written {0} float property value of {1}.", propertyDefinition.Name, floatVal);
                return 4;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing float property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteVectorProperty(this XUR5 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Vector)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not vector, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                if (val is not XUVector vectorVal)
                {
                    xur.Logger?.Here().Error("Property {0} marked as vector had a non-vector value of {1}, returning null.", propertyDefinition.Name, val);
                    return null;
                }

                IVECTSection? vectSection = ((IXUR)xur).TryFindXURSectionByMagic<IVECTSection>(IVECTSection.ExpectedMagic);
                if (vectSection == null)
                {
                    xur.Logger?.Here().Error("VECT section was null, returning null.");
                    return null;
                }

                int vectorIndex = vectSection.Vectors.IndexOf(vectorVal);
                if (vectorIndex == -1)
                {
                    xur.Logger?.Here().Error("Failed to get vector index for property {0} value {1}, returning null.", propertyDefinition.Name, vectorVal);
                    return null;
                }

                writer.WriteInt32BE(vectorIndex);
                xur.Logger?.Here().Verbose("Written {0} vector property value of {1} as index {2}.", propertyDefinition.Name, vectorVal, vectorIndex);
                return 4;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing vector property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }
    }
}
