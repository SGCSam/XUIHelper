using Serilog;
using Serilog.Core;
using System;
using System.Collections;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public static class XUR8WriteExtensions
    {
        public static int? TryWriteProperty(this XUR8 xur, BinaryWriter writer, XUProperty property, object val)
        {
            try
            {
                int? bytesWritten = null;
                switch (property.PropertyDefinition.Type)
                {
                    case XUPropertyDefinitionTypes.Bool:
                    {
                        bytesWritten = TryWriteBoolProperty(xur, writer, property.PropertyDefinition, val);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Integer:
                    {
                        bytesWritten = TryWriteIntegerProperty(xur, writer, property.PropertyDefinition, val);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Unsigned:
                    {
                        bytesWritten = TryWriteUnsignedProperty(xur, writer, property.PropertyDefinition, val);
                        break;
                    }
                    case XUPropertyDefinitionTypes.String:
                    {
                        bytesWritten = TryWriteStringProperty(xur, writer, property.PropertyDefinition, val);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Float:
                    {
                        bytesWritten = TryWriteFloatProperty(xur, writer, property.PropertyDefinition, val);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Vector:
                    {
                        bytesWritten = TryWriteVectorProperty(xur, writer, property.PropertyDefinition, val);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Object:
                    {
                        bytesWritten = TryWriteObjectProperty(xur, writer, property.PropertyDefinition, val);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Colour:
                    {
                        bytesWritten = TryWriteColourProperty(xur, writer, property.PropertyDefinition, val);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Custom:
                    {
                        bytesWritten = TryWriteCustomProperty(xur, writer, property.PropertyDefinition, val);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Quaternion:
                    {
                        bytesWritten = TryWriteQuaternionProperty(xur, writer, property.PropertyDefinition, val);
                        break;
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

        public static int? TryWriteBoolProperty(this XUR8 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Bool)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not boolean, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                if (val is not bool boolVal)
                {
                    xur.Logger?.Here().Error("Property {0} marked as boolean had a non-boolean value of {1}, returning null.", propertyDefinition.Name, val);
                    return null;
                }

                writer.Write(boolVal ? (byte)0x1 : (byte)0x0);
                xur.Logger?.Here().Verbose("Written {0} boolean property value of {1}.", propertyDefinition.Name, boolVal);
                return 1;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing boolean property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteIntegerProperty(this XUR8 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing integer property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteUnsignedProperty(this XUR8 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing unsigned property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteStringProperty(this XUR8 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing string property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteFloatProperty(this XUR8 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Float)
                {
                    xur.Logger?.Here().Error("Property type for {0} is not float, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                if (val is not float floatVal)
                {
                    xur.Logger?.Here().Error("Property {0} marked as float had a non-float value of {1}, returning null.", propertyDefinition.Name, val);
                    return null;
                }

                IFLOTSection? flotSection = ((IXUR)xur).TryFindXURSectionByMagic<IFLOTSection>(IFLOTSection.ExpectedMagic);
                if (flotSection == null)
                {
                    xur.Logger?.Here().Error("FLOT section was null, returning null.");
                    return null;
                }

                int floatIndex = flotSection.Floats.IndexOf(floatVal);
                if (floatIndex == -1)
                {
                    xur.Logger?.Here().Error("Failed to get float index for {0} with value {1}, returning null.", propertyDefinition.Name, floatVal);
                    return null;
                }

                int floatIndexBytesWritten = 0;
                writer.WritePackedUInt((uint)floatIndex, out floatIndexBytesWritten);
                xur.Logger?.Here().Verbose("Written {0} float property value of {1} as index {2}, {3} bytes.", propertyDefinition.Name, floatVal, floatIndex, floatIndexBytesWritten);
                return floatIndexBytesWritten;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing float property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteVectorProperty(this XUR8 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing vector property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteObjectProperty(this XUR8 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing object property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteColourProperty(this XUR8 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing colour property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteCustomProperty(this XUR8 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing custom property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteQuaternionProperty(this XUR8 xur, BinaryWriter writer, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing quaternion property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryWriteNamedFrame(this XUR8 xur, BinaryWriter writer, XUNamedFrame namedFrame)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing named frame, returning null. The exception is: {0}", ex);
                return null;
            }
        }

        public static int? TryWriteTimeline(this XUR8 xur, BinaryWriter writer, XUObject parentObject, XUTimeline timeline)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when writing timeline, returning null. The exception is: {0}", ex);
                return null;
            }
        }
    }
}
