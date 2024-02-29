using Serilog;
using Serilog.Core;
using System;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public static class XUR12ReadExtensions
    {
        public static XUProperty? TryReadProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                object? value = null;
                switch (propertyDefinition.Type)
                {
                    case XUPropertyDefinitionTypes.Bool:
                    {
                        value = TryReadBoolProperty(xui, propertyDefinition, element);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Integer:
                    {
                        value = TryReadIntegerProperty(xui, propertyDefinition, element);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Unsigned:
                    {
                        value = TryReadUnsignedProperty(xui, propertyDefinition, element);
                        break;
                    }
                    case XUPropertyDefinitionTypes.String:
                    {
                        value = TryReadStringProperty(xui, propertyDefinition, element);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Float:
                    {
                        value = TryReadFloatProperty(xui, propertyDefinition, element);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Vector:
                    {
                        value = TryReadVectorProperty(xui, propertyDefinition, element);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Object:
                    {
                        value = TryReadObjectProperty(xui, propertyDefinition, element);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Colour:
                    {
                        value = TryReadColourProperty(xui, propertyDefinition, element);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Custom:
                    {
                        value = TryReadCustomProperty(xui, propertyDefinition, element);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Quaternion:
                    {
                        value = TryReadQuaternionProperty(xui, propertyDefinition, element);
                        break;
                    }
                    default:
                    {
                        xui.Logger?.Here().Error("Unhandled property type of {0} when reading property {1}, returning null.", propertyDefinition.Type, propertyDefinition.Name);
                        return null;
                    }
                }

                if (value == null)
                {
                    xui.Logger?.Here().Error("Read value was null when reading property {0}, an error must have occurred, returning null.", propertyDefinition.Name);
                    return null;
                }

                return new XUProperty(propertyDefinition, value);
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static bool? TryReadBoolProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading bool property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static int? TryReadIntegerProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading integer property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static uint? TryReadUnsignedProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading unsigned property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static string? TryReadStringProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.String)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not string, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                xui.Logger?.Here().Verbose("Converting {0} for string property {1}.", element.Value, propertyDefinition.Name);
                return Convert.ToString(element.Value);
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading string property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static float? TryReadFloatProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Float)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not float, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                xui.Logger?.Here().Verbose("Converting {0} for float property {1}.", element.Value, propertyDefinition.Name);
                return Convert.ToSingle(element.Value);
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading float property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUVector? TryReadVectorProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Vector)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not vector, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                xui.Logger?.Here().Verbose("Converting {0} for vector property {1}.", element.Value, propertyDefinition.Name);
                string[] vectComponents = element.Value.Split(',');
                if(vectComponents.Length != 3)
                {
                    xui.Logger?.Here().Error("Failed to read vector property as we received an unexpected number of components, returning false. Expected: 3, Actual: {0}", vectComponents.Length); ;
                    return null;
                }

                float x = Convert.ToSingle(vectComponents[0]);
                float y = Convert.ToSingle(vectComponents[1]);
                float z = Convert.ToSingle(vectComponents[2]);
                return new XUVector(x, y, z);
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading vector property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static List<XUProperty>? TryReadObjectProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading object property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUColour? TryReadColourProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading colour property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUFigure? TryReadCustomProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading custom property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XUQuaternion? TryReadQuaternionProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, XElement element)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when reading quaternion property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }
    }
}
