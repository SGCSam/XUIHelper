using Serilog;
using Serilog.Core;
using System;
using System.Collections;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using XUIHelper.Core.Extensions;
using static System.Net.Mime.MediaTypeNames;

namespace XUIHelper.Core
{
    public static class XUR12WriteExtensions
    {
        public static XElement? TryWriteProperty(this XUI12 xui, XUProperty property)
        {
            try
            {
                XElement? value = null;
                switch (property.PropertyDefinition.Type)
                {
                    case XUPropertyDefinitionTypes.Bool:
                    {
                        value = TryWriteBoolProperty(xui, property);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Integer:
                    {
                        value = TryWriteIntegerProperty(xui, property);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Unsigned:
                    {
                        value = TryWriteUnsignedProperty(xui, property);
                        break;
                    }
                    case XUPropertyDefinitionTypes.String:
                    {
                        value = TryWriteStringProperty(xui, property);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Float:
                    {
                        value = TryWriteFloatProperty(xui, property);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Vector:
                    {
                        value = TryWriteVectorProperty(xui, property);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Object:
                    {
                        value = TryWriteObjectProperty(xui, property);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Colour:
                    {
                        value = TryWriteColourProperty(xui, property);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Custom:
                    {
                        value = TryWriteCustomProperty(xui, property);
                        break;
                    }
                    case XUPropertyDefinitionTypes.Quaternion:
                    {
                        value = TryWriteQuaternionProperty(xui, property);
                        break;
                    }
                    default:
                    {
                        xui.Logger?.Here().Error("Unhandled property type of {0} when writing property {1}, returning null.", property.PropertyDefinition.Type, property.PropertyDefinition.Name);
                        return null;
                    }
                }

                if (value == null)
                {
                    xui.Logger?.Here().Error("Written value was null when writing property {0}, an error must have occurred, returning null.", property.PropertyDefinition.Name);
                    return null;
                }

                return value;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing property {0}, returning null. The exception is: {1}", property.PropertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteBoolProperty(this XUI12 xui, XUProperty property)
        {
            try
            {
                if (property.PropertyDefinition.Type != XUPropertyDefinitionTypes.Bool)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not bool, it is {1}, returning null.", property.PropertyDefinition.Name, property.PropertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(property.PropertyDefinition.Name);

                xui.Logger?.Here().Verbose("Writing bool property {0} with value {1}", property.PropertyDefinition.Name, property.Value);
                retElement.Value = (bool)property.Value ? "true" : "false";
                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing bool property {0}, returning null. The exception is: {1}", property.PropertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteIntegerProperty(this XUI12 xui, XUProperty property)
        {
            try
            {
                if (property.PropertyDefinition.Type != XUPropertyDefinitionTypes.Integer)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not integer, it is {1}, returning null.", property.PropertyDefinition.Name, property.PropertyDefinition.Type);
                    return null;
                }

                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing integer property {0}, returning null. The exception is: {1}", property.PropertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteUnsignedProperty(this XUI12 xui, XUProperty property)
        {
            try
            {
                if (property.PropertyDefinition.Type != XUPropertyDefinitionTypes.Unsigned)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not unsigned, it is {1}, returning null.", property.PropertyDefinition.Name, property.PropertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(property.PropertyDefinition.Name);
                xui.Logger?.Here().Verbose("Writing bool property {0} with value {1}", property.PropertyDefinition.Name, property.Value);
                retElement.Value = ((uint)property.Value).ToString();
                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing unsigned property {0}, returning null. The exception is: {1}", property.PropertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteStringProperty(this XUI12 xui, XUProperty property)
        {
            try
            {
                if (property.PropertyDefinition.Type != XUPropertyDefinitionTypes.String)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not string, it is {1}, returning null.", property.PropertyDefinition.Name, property.PropertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(property.PropertyDefinition.Name);

                xui.Logger?.Here().Verbose("Writing string property {0} with value {1}", property.PropertyDefinition.Name, property.Value);
                retElement.Value = ((string)property.Value).ToString();
                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing string property {0}, returning null. The exception is: {1}", property.PropertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteFloatProperty(this XUI12 xui, XUProperty property)
        {
            try
            {
                if (property.PropertyDefinition.Type != XUPropertyDefinitionTypes.Float)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not float, it is {1}, returning null.", property.PropertyDefinition.Name, property.PropertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(property.PropertyDefinition.Name);

                xui.Logger?.Here().Verbose("Writing float property {0} with value {1}", property.PropertyDefinition.Name, property.Value);
                retElement.Value = ((float)property.Value).ToString("0.000000");
                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing float property {0}, returning null. The exception is: {1}", property.PropertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteVectorProperty(this XUI12 xui, XUProperty property)
        {
            try
            {
                if (property.PropertyDefinition.Type != XUPropertyDefinitionTypes.Vector)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not vector, it is {1}, returning null.", property.PropertyDefinition.Name, property.PropertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(property.PropertyDefinition.Name);

                xui.Logger?.Here().Verbose("Writing vector property {0} with value {1}", property.PropertyDefinition.Name, property.Value);
                XUVector? vect = property.Value as XUVector;
                if(vect == null)
                {
                    xui.Logger?.Here().Error("Property value was not a vector, returning null.");
                    return null;
                }

                retElement.Value = string.Join(",", new List<string>()
                {
                    vect.X.ToString("0.000000"),
                    vect.Y.ToString("0.000000"),
                    vect.Z.ToString("0.000000")
                }).TrimEnd();

                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing vector property {0}, returning null. The exception is: {1}", property.PropertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteObjectProperty(this XUI12 xui, XUProperty property)
        {
            try
            {
                if (property.PropertyDefinition.Type != XUPropertyDefinitionTypes.Object)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not object, it is {1}, returning null.", property.PropertyDefinition.Name, property.PropertyDefinition.Type);
                    return null;
                }

                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing object property {0}, returning null. The exception is: {1}", property.PropertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteColourProperty(this XUI12 xui, XUProperty property)
        {
            try
            {
                if (property.PropertyDefinition.Type != XUPropertyDefinitionTypes.Colour)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not colour, it is {1}, returning null.", property.PropertyDefinition.Name, property.PropertyDefinition.Type);
                    return null;
                }

                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing colour property {0}, returning null. The exception is: {1}", property.PropertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteCustomProperty(this XUI12 xui, XUProperty property)
        {
            try
            {
                if (property.PropertyDefinition.Type != XUPropertyDefinitionTypes.Custom)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not custom, it is {1}, returning null.", property.PropertyDefinition.Name, property.PropertyDefinition.Type);
                    return null;
                }

                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing custom property {0}, returning null. The exception is: {1}", property.PropertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteQuaternionProperty(this XUI12 xui, XUProperty property)
        {
            try
            {
                if (property.PropertyDefinition.Type != XUPropertyDefinitionTypes.Quaternion)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not quaternion, it is {1}, returning null.", property.PropertyDefinition.Name, property.PropertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(property.PropertyDefinition.Name);

                xui.Logger?.Here().Verbose("Writing quaternion property {0} with value {1}", property.PropertyDefinition.Name, property.Value);
                XUQuaternion? quat = property.Value as XUQuaternion;
                if (quat == null)
                {
                    xui.Logger?.Here().Error("Property value was not a quaternion, returning null.");
                    return null;
                }

                retElement.Value = string.Join(",", new List<string>()
                {
                    quat.X.ToString("0.000000"),
                    quat.Y.ToString("0.000000"),
                    quat.Z.ToString("0.000000"),
                    quat.W.ToString("0.000000")
                }).TrimEnd();

                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing quaternion property {0}, returning null. The exception is: {1}", property.PropertyDefinition.Name, ex);
                return null;
            }
        }
    }
}
