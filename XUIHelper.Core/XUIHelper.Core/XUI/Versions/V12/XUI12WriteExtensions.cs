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
        public static List<XElement>? TryWriteProperty(this XUI12 xui, XUProperty property)
        {
            try
            {
                int indexCount = 1;
                bool isIndexed = property.PropertyDefinition.FlagsSet.Contains(XUPropertyDefinitionFlags.Indexed);
                if (isIndexed)
                {
                    indexCount = (property.Value as IList).Count;
                }

                List<XElement> retValue = new List<XElement>(); 
                for(int i = 0; i < indexCount; i++)
                {
                    object propertyVal;
                    if(isIndexed)
                    {
                        propertyVal = (property.Value as List<object>)[i];
                    }
                    else
                    {
                        propertyVal = property.Value;
                    }

                    XElement? readValue = null;
                    switch (property.PropertyDefinition.Type)
                    {
                        case XUPropertyDefinitionTypes.Bool:
                        {
                            readValue = TryWriteBoolProperty(xui, property.PropertyDefinition, propertyVal);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Integer:
                        {
                            readValue = TryWriteIntegerProperty(xui, property.PropertyDefinition, propertyVal);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Unsigned:
                        {
                            readValue = TryWriteUnsignedProperty(xui, property.PropertyDefinition, propertyVal);
                            break;
                        }
                        case XUPropertyDefinitionTypes.String:
                        {
                            readValue = TryWriteStringProperty(xui, property.PropertyDefinition, propertyVal);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Float:
                        {
                            readValue = TryWriteFloatProperty(xui, property.PropertyDefinition, propertyVal);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Vector:
                        {
                            readValue = TryWriteVectorProperty(xui, property.PropertyDefinition, propertyVal);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Object:
                        {
                            readValue = TryWriteObjectProperty(xui, property.PropertyDefinition, propertyVal);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Colour:
                        {
                            readValue = TryWriteColourProperty(xui, property.PropertyDefinition, propertyVal);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Custom:
                        {
                            readValue = TryWriteCustomProperty(xui, property.PropertyDefinition, propertyVal);
                            break;
                        }
                        case XUPropertyDefinitionTypes.Quaternion:
                        {
                            readValue = TryWriteQuaternionProperty(xui, property.PropertyDefinition, propertyVal);
                            break;
                        }
                        default:
                        {
                            xui.Logger?.Here().Error("Unhandled property type of {0} when writing property {1}, returning null.", property.PropertyDefinition.Type, property.PropertyDefinition.Name);
                            return null;
                        }
                    }

                    if (readValue == null)
                    {
                        xui.Logger?.Here().Error("Written value was null when writing property {0}, an error must have occurred, returning null.", property.PropertyDefinition.Name);
                        return null;
                    }
                    else if (!isIndexed)
                    {
                        retValue = new List<XElement>() { readValue };
                        break;
                    }
                    else
                    {
                        readValue.SetAttributeValue("index", i);
                        retValue.Add(readValue);
                    }
                }

                return retValue;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing property {0}, returning null. The exception is: {1}", property.PropertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteBoolProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Bool)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not bool, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(propertyDefinition.Name);

                xui.Logger?.Here().Verbose("Writing bool property {0} with value {1}", propertyDefinition.Name, val);
                retElement.Value = (bool)val ? "true" : "false";
                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing bool property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteIntegerProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Integer)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not integer, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(propertyDefinition.Name);
                xui.Logger?.Here().Verbose("Writing integer property {0} with value {1}", propertyDefinition.Name, val);
                retElement.Value = ((int)val).ToString();
                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing integer property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteUnsignedProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Unsigned)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not unsigned, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(propertyDefinition.Name);
                xui.Logger?.Here().Verbose("Writing unsigned property {0} with value {1}", propertyDefinition.Name, val);
                retElement.Value = ((uint)val).ToString();
                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing unsigned property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteStringProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.String)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not string, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(propertyDefinition.Name);

                xui.Logger?.Here().Verbose("Writing string property {0} with value {1}", propertyDefinition.Name, val);
                retElement.Value = ((string)val).ToString();
                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing string property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteFloatProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Float)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not float, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(propertyDefinition.Name);

                xui.Logger?.Here().Verbose("Writing float property {0} with value {1}", propertyDefinition.Name, val);
                retElement.Value = ((float)val).ToString("0.000000");
                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing float property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteVectorProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Vector)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not vector, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(propertyDefinition.Name);

                xui.Logger?.Here().Verbose("Writing vector property {0} with value {1}", propertyDefinition.Name, val);
                XUVector? vect = val as XUVector;
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
                xui.Logger?.Here().Error("Caught an exception when writing vector property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteObjectProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Object)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not object, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(propertyDefinition.Name);
                xui.Logger?.Here().Verbose("Writing object property {0}", propertyDefinition.Name);

                XElement parentPropertiesElement = new XElement("Properties");
                foreach(XUProperty compoundProperty in val as List<XUProperty>) 
                { 
                    List<XElement>? thisPropertyElements = TryWriteProperty(xui, compoundProperty);
                    if (thisPropertyElements == null)
                    {
                        xui.Logger?.Here().Error("Failed to write compound property {0}, an error must have occurred, returning null.", compoundProperty.PropertyDefinition.Name);
                        return null;
                    }

                    foreach (XElement propertyElement in thisPropertyElements)
                    {
                        parentPropertiesElement.Add(propertyElement);
                    }
                }

                retElement.Add(parentPropertiesElement);
                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing object property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteColourProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Colour)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not colour, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(propertyDefinition.Name);

                xui.Logger?.Here().Verbose("Writing colour property {0} with value {1}", propertyDefinition.Name, val);
                XUColour? colour = val as XUColour;
                if (colour == null)
                {
                    xui.Logger?.Here().Error("Property value was not a colour, returning null.");
                    return null;
                }

                retElement.Value = string.Format("0x{0:x2}{1:x2}{2:x2}{3:x2}", colour.A, colour.R, colour.G, colour.B);
                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing colour property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteCustomProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Custom)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not custom, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(propertyDefinition.Name);

                xui.Logger?.Here().Verbose("Writing custom property {0} with value {1}", propertyDefinition.Name, val);
                XUFigure? figure = val as XUFigure;
                if (figure == null)
                {
                    xui.Logger?.Here().Error("Property value was not a figure, returning null.");
                    return null;
                }

                retElement.Value += figure.Points.Count;
                retElement.Value += ",";
                foreach(XUBezierPoint bezierPoint in figure.Points) 
                {
                    retElement.Value += bezierPoint.Point.X.ToString("0.000000");
                    retElement.Value += ",";
                    retElement.Value += bezierPoint.Point.Y.ToString("0.000000");
                    retElement.Value += ",";
                    retElement.Value += bezierPoint.ControlPointOne.X.ToString("0.000000");
                    retElement.Value += ",";
                    retElement.Value += bezierPoint.ControlPointOne.Y.ToString("0.000000");
                    retElement.Value += ",";
                    retElement.Value += bezierPoint.ControlPointTwo.X.ToString("0.000000");
                    retElement.Value += ",";
                    retElement.Value += bezierPoint.ControlPointTwo.Y.ToString("0.000000");
                    retElement.Value += ",";
                    retElement.Value += "0";
                    retElement.Value += ",";
                }

                return retElement;
            }
            catch (Exception ex)
            {
                xui.Logger?.Here().Error("Caught an exception when writing custom property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        public static XElement? TryWriteQuaternionProperty(this XUI12 xui, XUPropertyDefinition propertyDefinition, object val)
        {
            try
            {
                if (propertyDefinition.Type != XUPropertyDefinitionTypes.Quaternion)
                {
                    xui.Logger?.Here().Error("Property type for {0} is not quaternion, it is {1}, returning null.", propertyDefinition.Name, propertyDefinition.Type);
                    return null;
                }

                XElement retElement = new XElement(propertyDefinition.Name);

                xui.Logger?.Here().Verbose("Writing quaternion property {0} with value {1}", propertyDefinition.Name, val);
                XUQuaternion? quat = val as XUQuaternion;
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
                xui.Logger?.Here().Error("Caught an exception when writing quaternion property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }
    }
}
