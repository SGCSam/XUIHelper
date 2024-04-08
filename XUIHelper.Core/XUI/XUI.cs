using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using XUIHelper.Core.Extensions;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Reflection.Metadata;
using System.Xml;

namespace XUIHelper.Core
{
    public abstract class XUI : IXUI
    {
        public ILogger? Logger { get; set; }

        public string FilePath { get; private set; }

        public XUObject? RootObject;

        public XUI(string filePath, ILogger? logger = null)
        {
            FilePath = filePath;
            Logger = logger?.ForContext(typeof(XUI));
        }

        public virtual async Task<bool> TryReadAsync()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    Logger?.Here().Error("The XUI file at {0} doesn't exist, returning false.", FilePath);
                    return false;
                }

                Logger?.Here().Information("Reading XUI file at {0}", FilePath);

                XDocument document = XDocument.Load(FilePath);
                if(document.Root == null)
                {
                    Logger?.Here().Error("The document root element was null, returning false.");
                    return false;
                }

                XUObject dummyParent = new XUObject("");
                RootObject = TryReadObject(document.Root, ref dummyParent);
                if (RootObject == null)
                {
                    Logger?.Here().Error("The root object was null, an error must have occurred, returning false.");
                    return false;
                }

                Logger?.Here().Information("Read successful!");
                return true;
            }
            catch (Exception ex)
            {
                Logger?.Here().Error("Caught an exception when trying to read XUI file at {0}, returning false. The exception is: {1}", FilePath, ex);
                return false;
            }
        }

        private XUObject? TryReadObject(XElement objectElement, ref XUObject parent)
        {

            Logger?.Here().Verbose("Reading class {0}", objectElement.Name);
            XUClass? elementClass = XMLExtensionsManager.TryGetClassByName(objectElement.Name.ToString());
            if(elementClass == null) 
            {
                Logger?.Here().Error("Failed to find class {0}, returning null.", objectElement.Name);
                return null;
            }

            List<XUClass>? classHierarchy = XMLExtensionsManager.TryGetClassHierarchy(elementClass.Name);
            if (classHierarchy == null)
            {
                Logger?.Here().Error("Failed to get class hierarchy for {0}, returning null.", elementClass.Name);
                return null;
            }

            XElement? parentPropertiesElement = objectElement.Element("Properties");
            if(parentPropertiesElement == null) 
            {
                Logger?.Here().Error("Failed to find properties element, returning null.");
                return null;
            }

            XUObject thisObject = new XUObject(elementClass.Name);

            IEnumerable<XElement> childPropertyElements = parentPropertiesElement.Elements();
            Logger?.Here().Verbose("Class {0} has {1} properties.", elementClass.Name, childPropertyElements.Count());

            foreach(XElement propertyElement in childPropertyElements)
            {
                string propertyName = propertyElement.Name.ToString();
                Logger?.Here().Verbose("Handling property {0}", propertyName);

                bool found = false;
                foreach(XUClass hierarchyClass in classHierarchy)
                {
                    XUPropertyDefinition? propertyDefinition = hierarchyClass.PropertyDefinitions.Where(x => x.Name == propertyName).FirstOrDefault();
                    if (propertyDefinition == null)
                    {
                        continue;
                    }

                    Logger?.Here().Verbose("Found property definition successfully in {0}, reading property...", hierarchyClass.Name);
                    XUProperty? readProperty = this.TryReadProperty(propertyDefinition, propertyElement);
                    if (readProperty == null)
                    {
                        Logger?.Here().Error("Read property was null, an error must have occurred, returning null.");
                        return null;
                    }

                    if(readProperty.PropertyDefinition.Type == XUPropertyDefinitionTypes.Custom)
                    {
                        Logger?.Here().Verbose("Property definition {0} was custom, updating bounding box...", propertyDefinition.Name);
                        XUFigure oldFigure = (XUFigure)readProperty.Value;

                        float widthValue = 60.0f;
                        XUProperty? widthProperty = thisObject.Properties.Where(x => x.PropertyDefinition.Name == "Width").FirstOrDefault();
                        if(widthProperty == null)
                        {
                            Logger?.Here().Error("Width property was null, using default of {0}", widthValue);
                        }
                        else
                        {
                            widthValue = (float)widthProperty.Value;
                        }

                        float heightValue = 30.0f;
                        XUProperty? heightProperty = thisObject.Properties.Where(x => x.PropertyDefinition.Name == "Height").FirstOrDefault();
                        if (heightProperty == null)
                        {
                            Logger?.Here().Error("Height property was null, using default of {0}", heightValue);
                        }
                        else
                        {
                            heightValue = (float)heightProperty.Value;
                        }

                        XUFigure newFigure = new XUFigure(new XUPoint(widthValue, heightValue), oldFigure.Points);
                        readProperty = new XUProperty(propertyDefinition, newFigure);
                    }

                    thisObject.Properties.Add(readProperty);
                    found = true;
                    break;
                }

                if(!found)
                {
                    Logger?.Here().Error("Failed to find property definition for {0}, returning null.", propertyName);
                    return null;
                }
            }

            foreach (XElement childObjectElement in objectElement.Elements())
            {
                if (childObjectElement.Name != "Properties" && childObjectElement.Name != "Timelines")
                {
                    Logger?.Here().Verbose("Reading child object {0}", childObjectElement.Name);
                    XUObject? thisChildObject = TryReadObject(childObjectElement, ref thisObject);
                    if (thisChildObject == null)
                    {
                        Logger?.Here().Error("Read child object was null, an error must have occurred, returning null.");
                        return null;
                    }

                    thisObject.Children.Add(thisChildObject);
                }
            }

            XElement? parentTimelinesElement = objectElement.Element("Timelines");
            if (parentTimelinesElement != null)
            {
                XElement? parentNamedFramesElement = parentTimelinesElement.Element("NamedFrames");
                if (parentNamedFramesElement != null)
                {
                    IEnumerable<XElement> childNamedFrameElements = parentNamedFramesElement.Elements("NamedFrame");
                    Logger?.Here().Verbose("Class {0} has {1} named frames.", elementClass.Name, childNamedFrameElements.Count());
                    foreach (XElement childNamedFrameElement in childNamedFrameElements)
                    {
                        XUNamedFrame? thisChildNamedFrame = this.TryReadNamedFrame(childNamedFrameElement);
                        if (thisChildNamedFrame == null)
                        {
                            Logger?.Here().Error("Read named frame object was null, an error must have occurred, returning null.");
                            return null;
                        }

                        thisObject.NamedFrames.Add(thisChildNamedFrame);
                    }
                }

                IEnumerable<XElement> childTimelineElements = parentTimelinesElement.Elements("Timeline");
                Logger?.Here().Verbose("Class {0} has {1} timelines.", elementClass.Name, childTimelineElements.Count());
                foreach(XElement childTimelineElement in childTimelineElements)
                {
                    XUTimeline? thisChildTimeline = this.TryReadTimeline(childTimelineElement, thisObject);
                    if (thisChildTimeline == null)
                    {
                        Logger?.Here().Error("Read timeline object was null, an error must have occurred, returning null.");
                        return null;
                    }

                    thisObject.Timelines.Add(thisChildTimeline);
                }
            }

            return thisObject;
        }

        public async Task<bool> TryWriteAsync(XUObject rootObject)
        {
            try
            {
                if(rootObject.ClassName != "XuiCanvas")
                {
                    Logger?.Here().Error("The object to write wasn't the root XuiCanvas, returning false.");
                    return false;
                }

                Logger?.Here().Information("Writing XUI file at {0}", FilePath);

                XElement dummyParent = new XElement("dummy");
                XElement? rootElement = TryWriteObject(ref dummyParent, rootObject);
                if (rootElement == null)
                {
                    Logger?.Here().Error("Failed to write root object, an error must have occurred, returning false.");
                    return false;
                }

                if(this is XUI12)
                {
                    rootElement.SetAttributeValue("version", "000c");
                }
                else
                {
                    Logger?.Here().Error("Unhandled XUI for version attribute, returning false.");
                    return false;
                }

                if (!File.Exists(FilePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                }

                XDocument document = new XDocument(rootElement);
                XmlWriterSettings writerSettings = new XmlWriterSettings();
                writerSettings.Async = true;
                writerSettings.OmitXmlDeclaration = true;
                writerSettings.Indent = true;
                writerSettings.IndentChars = "";
                writerSettings.NewLineChars = "\r\n";
                writerSettings.NewLineHandling = NewLineHandling.Replace;

                using (XmlWriter xw = XmlWriter.Create(FilePath, writerSettings))
                {
                    await document.SaveAsync(xw, CancellationToken.None);
                }

                Logger?.Here().Information("Write successful!");
                return true;
            }
            catch (Exception ex)
            {
                Logger?.Here().Error("Caught an exception when trying to read XUI file at {0}, returning false. The exception is: {1}", FilePath, ex);
                return false;
            }
        }

        private XElement? TryWriteObject(ref XElement parentElement, XUObject xuObject)
        {
            try
            {
                XElement thisObjectElement = new XElement(xuObject.ClassName);
                Logger?.Here().Verbose("Writing {0}", xuObject.ClassName);

                if (xuObject.Properties.Count > 0)
                {
                    Logger?.Here().Verbose("{0} has {1} properties.", xuObject.ClassName, xuObject.Properties.Count);

                    XElement parentPropertiesElement = new XElement("Properties");

                    foreach(XUProperty childProperty in xuObject.Properties) 
                    {
                        Logger?.Here().Verbose("Writing property {0}.", childProperty.PropertyDefinition.Name);
                        List<XElement>? thisPropertyElements = this.TryWriteProperty(childProperty);
                        if (thisPropertyElements == null)
                        {
                            Logger?.Here().Error("Failed to write property {0}, an error must have occurred, returning null.", childProperty.PropertyDefinition.Name);
                            return null;
                        }

                        Logger?.Here().Verbose("Wrote property {0} as {1}.", childProperty.PropertyDefinition.Name, string.Join("\n", thisPropertyElements));

                        foreach(XElement propertyElement in thisPropertyElements)
                        {
                            parentPropertiesElement.Add(propertyElement);
                        }
                    }

                    thisObjectElement.Add(parentPropertiesElement);
                }

                if (xuObject.Children.Count > 0)
                {
                    Logger?.Here().Verbose("{0} has {1} children.", xuObject.ClassName, xuObject.Children.Count);

                    XElement parentPropertiesElement = new XElement("Properties");
                    foreach (XUObject childObject in xuObject.Children)
                    {
                        XElement? thisChildElement = TryWriteObject(ref thisObjectElement, childObject);
                        if (thisChildElement == null)
                        {
                            Logger?.Here().Error("Failed to write child object {0}, an error must have occurred, returning null.", childObject.ClassName);
                            return null;
                        }

                        Logger?.Here().Verbose("Wrote child object {0} successfully!.", childObject.ClassName);
                        thisObjectElement.Add(thisChildElement);
                    }
                }

                if(xuObject.Timelines.Count > 0 || xuObject.NamedFrames.Count > 0)
                {
                    Logger?.Here().Verbose("{0} has {1} timelines.", xuObject.ClassName, xuObject.Timelines.Count);

                    XElement parentTimelinesElement = new XElement("Timelines");
                    if(xuObject.NamedFrames.Count > 0)
                    {
                        Logger?.Here().Verbose("{0} has {1} named frames.", xuObject.ClassName, xuObject.NamedFrames.Count);
                        XElement parentNamedFramesElement = new XElement("NamedFrames");

                        foreach(XUNamedFrame namedFrame in xuObject.NamedFrames)
                        {
                            XElement? thisNamedFrameElement = this.TryWriteNamedFrame(namedFrame);
                            if (thisNamedFrameElement == null)
                            {
                                Logger?.Here().Error("Failed to write named frame {0} for object {1}, an error must have occurred, returning null.", namedFrame.Name, xuObject.ClassName);
                                return null;
                            }

                            Logger?.Here().Verbose("Wrote named frame {0} as {1}", namedFrame.Name, thisNamedFrameElement);
                            parentNamedFramesElement.Add(thisNamedFrameElement);
                        }

                        parentTimelinesElement.Add(parentNamedFramesElement);
                    }

                    foreach (XUTimeline timeline in xuObject.Timelines)
                    {
                        XElement? thisTimelineElement = this.TryWriteTimeline(timeline);
                        if (thisTimelineElement == null)
                        {
                            Logger?.Here().Error("Failed to write timeline, an error must have occurred, returning null.");
                            return null;
                        }

                        Logger?.Here().Verbose("Wrote timeline as {0}", thisTimelineElement);
                        parentTimelinesElement.Add(thisTimelineElement);
                    }

                    thisObjectElement.Add(parentTimelinesElement);
                }

                return thisObjectElement;
            }
            catch (Exception ex)
            {
                Logger?.Here().Error("Caught an exception when writing object {0}, returning null. The exception is: {1}", xuObject.ClassName, ex);
                return null;
            }
        }
    }
}
