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

namespace XUIHelper.Core
{
    public abstract class XUI : IXUI
    {
        public ILogger? Logger { get; set; }

        public string FilePath { get; private set; }

        public XUObject? RootObject;

        private XMLExtensionsManager? _ExtensionsManager;

        public XUI(string filePath, ILogger? logger = null)
        {
            FilePath = filePath;
            Logger = logger?.ForContext(typeof(XUI));
        }

        public virtual async Task<bool> TryReadAsync(int extensionVersion)
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    Logger?.Here().Error("The XUI file at {0} doesn't exist, returning false.", FilePath);
                    return false;
                }

                if(!XUIHelperCoreConstants.VersionedExtensions.ContainsKey(extensionVersion))
                {
                    Logger?.Here().Error("Failed to find extensions with version {0}, returning false.", extensionVersion);
                    return false;
                }

                _ExtensionsManager = XUIHelperCoreConstants.VersionedExtensions[extensionVersion];
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

        public XUObject? TryReadObject(XElement objectElement, ref XUObject parent)
        {
            if(_ExtensionsManager == null) 
            {
                Logger?.Here().Error("Extensions manager was null, returning null.");
                return null;
            }

            Logger?.Here().Verbose("Reading class {0}", objectElement.Name);
            XUClass? elementClass = _ExtensionsManager.TryGetClassByName(objectElement.Name.ToString());
            if(elementClass == null) 
            {
                Logger?.Here().Error("Failed to find class {0}, returning null.", objectElement.Name);
                return null;
            }

            List<XUClass>? classHierarchy = _ExtensionsManager.TryGetClassHierarchy(elementClass.Name);
            if (classHierarchy == null)
            {
                Logger?.Here().Error("Failed to get class hierarchy for {0}, returning null.", elementClass.Name);
                return null;
            }

            XElement? parentPropertiesElement = objectElement.Descendants("Properties").FirstOrDefault();
            if(parentPropertiesElement == null) 
            {
                Logger?.Here().Error("Failed to find properties element, returning null.");
                return null;
            }

            IEnumerable<XElement> childPropertyElements = parentPropertiesElement.Descendants();
            Logger?.Here().Verbose("Class {0} has {1} properties.", elementClass.Name, childPropertyElements.Count());

            List<XUProperty> properties = new List<XUProperty>();
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

                    properties.Add(readProperty);
                    found = true;
                    break;
                }

                if(!found)
                {
                    Logger?.Here().Error("Failed to find property definition for {0}, returning null.", propertyName);
                    return null;
                }
            }

            XUObject thisObject = new XUObject(elementClass.Name);
            thisObject.Properties = properties;
            foreach(XElement childObjectElement in objectElement.Elements())
            {
                if(childObjectElement.Name != "Properties")
                {
                    Logger?.Here().Verbose("Reading child object {0}", childObjectElement.Name);
                    XUObject? thisChildObject = TryReadObject(childObjectElement, ref thisObject);
                    if(thisChildObject == null)
                    {
                        Logger?.Here().Error("Read child object was null, an error must have occurred, returning null.");
                        return null;
                    }

                    thisObject.Children.Add(thisChildObject);
                }
            }

            return thisObject;
        }
    }
}
