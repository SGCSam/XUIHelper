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
                        throw new NotImplementedException();
                    }
                    case XUPropertyDefinitionTypes.Float:
                    {
                        bytesWritten = TryWriteFloatProperty(xur, writer, property.PropertyDefinition, property.Value);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Vector:
                    {
                        throw new NotImplementedException();
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
    }
}
