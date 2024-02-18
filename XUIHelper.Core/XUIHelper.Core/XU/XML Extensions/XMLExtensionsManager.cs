using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class XMLExtensionsManager
    {
        public ILogger? Logger { get; private set; }

        private List<XUClass> _Classes = new List<XUClass>();

        public XMLExtensionsManager(ILogger? logger = null) 
        {
            Logger = logger?.ForContext(typeof(XMLExtensionsManager));
        }

        public async Task<bool> TryRegisterXMLExtensionsAsync(string xmlExtensionFilePath)
        {
            try
            {
                if(!File.Exists(xmlExtensionFilePath)) 
                {
                    Logger?.Here().Error("Failed to register XML extensions as the file does not exist at {0}, returning false.", xmlExtensionFilePath);
                    return false;
                }

                XmlSerializer serializer = new XmlSerializer(typeof(XUClassExtension));
                using (FileStream extensionsFileStream = new FileStream(xmlExtensionFilePath, FileMode.Open))
                {
                    XUClassExtension? deserializedExtension = (XUClassExtension?)serializer.Deserialize(extensionsFileStream);
                    if (deserializedExtension == null)
                    {
                        Logger?.Here().Error("Failed to register XML extensions at {0} as the deserialization has failed, returning false.", xmlExtensionFilePath);
                        return false;
                    }

                    foreach (XUClass existingClass in _Classes)
                    {
                        foreach (XUClass deserializedClass in deserializedExtension.Classes)
                        {
                            if (existingClass.Name == deserializedClass.Name)
                            {
                                Logger?.Here().Error("Failed to register XML extensions at {0} as the class {1} is a duplicate, returning false.", xmlExtensionFilePath, existingClass.Name);
                                return false;
                            }
                        }
                    }

                    _Classes.AddRange(deserializedExtension.Classes);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger?.Here().Error("Caught an exception when trying to register XML extensions at {0}, returning false. The exception is: {1}", xmlExtensionFilePath, ex);
                return false;
            }
        }

        public XUClass? TryGetClassByName(string name)
        {
            foreach (XUClass cla in _Classes)
            {
                if (cla.Name == name)
                {
                    return cla;
                }
            }

            return null;
        }

        public List<XUClass>? TryGetClassHierarchy(string derivedClass)
        {
            List<XUClass> hierarchy = new List<XUClass>();
            XUClass? thisClass = TryGetClassByName(derivedClass);

            if (thisClass == null || thisClass.Name == null)
            {
                Logger?.Here().Error("Failed to load {0} derived class when getting hierarchy. This may be a custom class that needs registered by an XML extension.", derivedClass);
                return null;
            }

            else
            {
                hierarchy.Add(thisClass);
            }

            //Max level I've seen is 3 but 5 is just to be safe
            for (int i = 0; i < 5; i++)
            {
                if (thisClass.BaseClassName != "(null)")
                {
                    string baseClassName = thisClass.BaseClassName;
                    thisClass = TryGetClassByName(baseClassName);

                    if (thisClass == null || thisClass.Name == null)
                    {
                        Logger?.Here().Error("Failed to load {0} base class when getting hierarchy. This may be a custom class that needs registered by an XML extension.", baseClassName);
                        return null;
                    }

                    else
                    {
                        hierarchy.Add(thisClass);
                    }
                }
            }

            // Reverse the order so we start with the root class properties
            hierarchy.Reverse();
            return hierarchy;
        }
    }
}
