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
    public static class XMLExtensionsManager
    {
        public const string XHEFileExtension = ".xhe";

        public class XUIHelperExtensionsGroupData
        {
            public string GroupName { get; private set; }
            public List<XUIHelperExtensionsFile> ExtensionsFiles { get; private set; } = new List<XUIHelperExtensionsFile>();

            public XUIHelperExtensionsGroupData(string groupName, List<XUIHelperExtensionsFile> extensionsFiles)
            {
                GroupName = groupName;
                ExtensionsFiles = extensionsFiles;
            }

            public XUIHelperExtensionsGroupData(string groupName)
            {
                GroupName = groupName;
                ExtensionsFiles = new List<XUIHelperExtensionsFile>();
            }
        }

        public class XUIHelperExtensionsFile
        {
            public string FilePath { get; private set; } = string.Empty;
            public XUIHelperExtensions Data { get; private set; }

            public XUIHelperExtensionsFile(string filePath, XUIHelperExtensions data)
            {
                FilePath = filePath;
                Data = data;
            }
        }

        #region Events

        #region ExtensionGroupsChanged
        private static void OnExtensionGroupsChanged()
        {
            if(ExtensionGroupsChanged != null)
            {
                ExtensionGroupsChanged(null, EventArgs.Empty);
            }
        }

        public static event EventHandler? ExtensionGroupsChanged;
        #endregion

        #endregion

        public static ILogger? Logger { get; private set; }

        public static string CurrentGroup { get; private set; } = string.Empty;

        public static Dictionary<string, XUIHelperExtensionsGroupData> Groups { get; private set; } = new Dictionary<string, XUIHelperExtensionsGroupData>();

        public static void Initialize(ILogger? logger = null) 
        {
            Logger = logger?.ForContext(typeof(XMLExtensionsManager));
        }

        public static async Task<bool> TryRegisterExtensionsGroupAsync(string extensionsGroupName, string xheFilePath)
        {
            try
            {
                Logger?.Here().Verbose("Registering classes from {0} into group {1}.", xheFilePath, extensionsGroupName);

                if(Groups.ContainsKey(extensionsGroupName))
                {
                    foreach (XUIHelperExtensionsFile extensionsFile in Groups[extensionsGroupName].ExtensionsFiles)
                    {
                        if (extensionsFile.FilePath == xheFilePath)
                        {
                            Logger?.Here().Verbose("Already registered extensions from {0} for group {1}, won't re-register, returning true.", xheFilePath, extensionsGroupName);
                            return true;
                        }
                    }
                }

                string oldGroup = CurrentGroup;
                SetCurrentGroup(extensionsGroupName);

                XUIHelperExtensions? xhe = await TryDeserializeXHEAsync(xheFilePath);
                if (xhe == null)
                {
                    Logger?.Here().Error("Registered extension for {0} was null, returning false.", xheFilePath);
                    return false;
                }

                Groups[extensionsGroupName].ExtensionsFiles.Add(new XUIHelperExtensionsFile(xheFilePath, xhe));

                if (!string.IsNullOrEmpty(oldGroup))
                {
                    SetCurrentGroup(oldGroup);
                }

                OnExtensionGroupsChanged();
                return true;
            }
            catch (Exception ex)
            {
                Logger?.Here().Error("Caught an exception when trying to register XML extensions group {0}, returning false. The exception is: {1}", extensionsGroupName, ex);
                return false;
            }
        }

        public static void DeregisterExtensionFile(string xmlExtensionsFilePath)
        {
            foreach(XUIHelperExtensionsGroupData group in Groups.Values)
            {
                foreach (XUIHelperExtensionsFile extensionFile in group.ExtensionsFiles.ToList())
                {
                    if (extensionFile.FilePath == xmlExtensionsFilePath)
                    {
                        group.ExtensionsFiles.Remove(extensionFile);
                        OnExtensionGroupsChanged();
                        return;
                    }
                }
            }
        }

        public static void DeregisterAllExtensionsFromGroup(string groupName)
        {
            if(!Groups.ContainsKey(groupName))
            {
                return;
            }

            foreach (XUIHelperExtensionsFile extensionFile in Groups[groupName].ExtensionsFiles.ToList())
            {
                Groups[groupName].ExtensionsFiles.Remove(extensionFile);
            }

            OnExtensionGroupsChanged();
        }

        public static void SetCurrentGroup(string groupName)
        {
            if(!Groups.ContainsKey(groupName))
            {
                Groups[groupName] = new XUIHelperExtensionsGroupData(groupName);
            }

            CurrentGroup = groupName;
        }

        public static XUClass? TryGetClassByName(string name)
        {
            if (!Groups.ContainsKey(CurrentGroup))
            {
                Logger?.Here().Error("Failed to get class {0} as the current group {1} is invalid, returning null.", name, CurrentGroup);
                return null;
            }

            foreach(XUIHelperExtensionsFile extensionFile in Groups[CurrentGroup].ExtensionsFiles)
            {
                if(extensionFile.Data.Extensions == null)
                {
                    continue;
                }

                foreach (XUClass cla in extensionFile.Data.Extensions.Classes)
                {
                    if (cla.Name == name)
                    {
                        return cla;
                    }
                }
            }

            return null;
        }

        public static List<XUClass>? TryGetClassHierarchy(string derivedClass)
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

            while(true)
            {
                if (thisClass.BaseClassName == "(null)" || string.IsNullOrEmpty(thisClass.BaseClassName))
                {
                    break;
                }

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
            

            // Reverse the order so we start with the root class properties
            hierarchy.Reverse();
            return hierarchy;
        }

        public static bool? IsPropertyIgnored(XUPropertyDefinition propertyDefinition) 
        {
            try
            {
                if (!Groups.ContainsKey(CurrentGroup))
                {
                    Logger?.Here().Error("Failed to get check if property {0} is ignored as the current group {1} is invalid, returning null.", propertyDefinition.Name, CurrentGroup);
                    return null;
                }

                foreach(XUIHelperExtensionsFile extensionFile in Groups[CurrentGroup].ExtensionsFiles)
                {
                    if (extensionFile.Data.IgnoreProperties == null)
                    {
                        continue;
                    }

                    foreach (XUIHelperIgnoreClass ignoredClass in extensionFile.Data.IgnoreProperties.IgnoredClasses)
                    {
                        if (propertyDefinition.ParentClassName != ignoredClass.ClassName)
                        {
                            continue;
                        }

                        foreach (XUIHelperIgnoreProperty ignoredProperty in ignoredClass.IgnornedProperties)
                        {
                            if (ignoredProperty.Value == propertyDefinition.Name)
                            {
                                return true;
                            }
                        }
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Logger?.Here().Error("Caught an exception when trying check if property {0} was ignored, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }

        private static async Task<XUIHelperExtensions?> TryDeserializeXHEAsync(string xheFilePath)
        {
            try
            {
                if (!File.Exists(xheFilePath))
                {
                    Logger?.Here().Error("Failed to register XML extensions as the file does not exist at {0}, returning null.", xheFilePath);
                    return null;
                }

                string fileExtension = Path.GetExtension(xheFilePath).ToLower();
                if(fileExtension != XHEFileExtension)
                {
                    Logger?.Here().Error("Failed to register XML extensions as the file has an unexpected extension, returning null. Expected: {0}, Actual: {1}.", XHEFileExtension, fileExtension);
                    return null;
                }

                if (!Groups.ContainsKey(CurrentGroup))
                {
                    Logger?.Here().Error("Failed to register XML extensions as the current group {0} is invalid, returning null.", CurrentGroup);
                    return null;
                }

                XmlSerializer xheSerializer = new XmlSerializer(typeof(XUIHelperExtensions));
                using (FileStream extensionsFileStream = new FileStream(xheFilePath, FileMode.Open))
                {
                    XUIHelperExtensions? deserializedExtension = (XUIHelperExtensions?)xheSerializer.Deserialize(extensionsFileStream);
                    if (deserializedExtension == null)
                    {
                        Logger?.Here().Error("Failed to register XML extensions at {0} as the deserialization has failed, returning null.", xheFilePath);
                        return null;
                    }

                    if(deserializedExtension.Extensions == null)
                    {
                        Logger?.Here().Error("Failed to register XML extensions at {0} as the extensions are null, returning null.", xheFilePath);
                        return null;
                    }

                    if (deserializedExtension.RelationalExtensions != null)
                    {
                        string? xheDirectory = Path.GetDirectoryName(xheFilePath);
                        if (string.IsNullOrEmpty(xheDirectory))
                        {
                            Logger?.Here().Warning("Failed to register relational extensions from {0} as the XHE directory is invalid, extensions from this file won't be registered.", xheFilePath);
                        }
                        else
                        {
                            foreach (XUIHelperRelationalExtension relationalExtension in deserializedExtension.RelationalExtensions.RelationalExtensions)
                            {
                                string absoluteRelationalsFilePath = Path.Combine(xheDirectory, relationalExtension.RelativeFilePath);

                                try
                                {
                                    if (!File.Exists(absoluteRelationalsFilePath))
                                    {
                                        Logger?.Here().Warning("Failed to register relational extensions from {0} as the file does not exist, extensions from this file won't be registered.", absoluteRelationalsFilePath);
                                        continue;
                                    }

                                    XmlSerializer classesSerializer = new XmlSerializer(typeof(XUClassExtension));
                                    using (FileStream relationalFileStream = new FileStream(absoluteRelationalsFilePath, FileMode.Open))
                                    {
                                        XUClassExtension? deserializedRelationals = (XUClassExtension?)classesSerializer.Deserialize(relationalFileStream);
                                        if (deserializedRelationals == null)
                                        {
                                            Logger?.Here().Warning("Failed to register relational extensions from {0} as the deserialization has failed, extensions from this file won't be registered.", absoluteRelationalsFilePath);
                                            return null;
                                        }

                                        deserializedExtension.Extensions.Classes.AddRange(deserializedRelationals.Classes);
                                        Logger?.Here().Verbose("Added {0} classes from relational extensions at {1}.", deserializedRelationals.Classes.Count, absoluteRelationalsFilePath);
                                    }
                                }
                                catch(Exception relationalEx)
                                {
                                    Logger?.Here().Warning("Failed to register relational extension from {0} as an exception occurred, extensions from this file won't be registered. The exception is: {1}", absoluteRelationalsFilePath, relationalEx);
                                    continue;
                                }
                            }
                        }
                    }

                    if(deserializedExtension.IgnoreProperties != null)
                    {
                        HashSet<string> foundIgnoredClassNames = new HashSet<string>();
                        foreach (XUIHelperIgnoreClass ignoredClass in deserializedExtension.IgnoreProperties.IgnoredClasses)
                        {
                            if (foundIgnoredClassNames.Contains(ignoredClass.ClassName))
                            {
                                Logger?.Here().Error("Failed to add ignore class {0} in XML extensions at {1} as it was a duplicate, returning null.", ignoredClass.ClassName, xheFilePath);
                                return null;
                            }

                            XUClass? foundClass = null;
                            foreach (XUClass deserializedClass in deserializedExtension.Extensions.Classes)
                            {
                                if (ignoredClass.ClassName == deserializedClass.Name)
                                {
                                    foundClass = deserializedClass;
                                    break;
                                }
                            }

                            if (foundClass == null)
                            {
                                Logger?.Here().Error("Failed to find ignore class {0} in XML extensions at {1}, returning null.", ignoredClass.ClassName, xheFilePath);
                                return null;
                            }

                            foundIgnoredClassNames.Add(ignoredClass.ClassName);
                            Logger?.Here().Verbose("Handling ignored class {0}.", ignoredClass.ClassName);

                            HashSet<string> foundIgnoredPropertyNames = new HashSet<string>();
                            foreach (XUIHelperIgnoreProperty ignoredProperty in ignoredClass.IgnornedProperties)
                            {
                                if (foundIgnoredPropertyNames.Contains(ignoredProperty.Value))
                                {
                                    Logger?.Here().Error("Failed to add ignore class {0} in XML extensions at {1} as the property {2} was a duplicate, returning null.", ignoredClass.ClassName, xheFilePath, ignoredProperty.Value);
                                    return null;
                                }

                                bool found = false;
                                foreach (XUPropertyDefinition propertyDefinition in foundClass.PropertyDefinitions)
                                {
                                    if (propertyDefinition.Name == ignoredProperty.Value)
                                    {
                                        found = true;
                                        break;
                                    }
                                }

                                if (!found)
                                {
                                    Logger?.Here().Error("Failed to find property definition {0} from ignored class {1} in XML extensions at {2}, returning null.", ignoredProperty.Value, ignoredClass.ClassName, xheFilePath);
                                    return null;
                                }

                                Logger?.Here().Verbose("Will ignore property {0} of {1}.", ignoredProperty.Value, ignoredClass.ClassName);
                                foundIgnoredPropertyNames.Add(ignoredProperty.Value);
                            }
                        }
                    }

                    foreach (XUIHelperExtensionsFile existingExtensionFile in Groups[CurrentGroup].ExtensionsFiles)
                    {
                        if(existingExtensionFile.Data.Extensions == null)
                        {
                            continue;
                        }

                        foreach (XUClass existingClass in existingExtensionFile.Data.Extensions.Classes)
                        {
                            foreach (XUClass deserializedClass in deserializedExtension.Extensions.Classes)
                            {
                                if (existingClass.Name == deserializedClass.Name)
                                {
                                    Logger?.Here().Error("Failed to register XML extensions at {0} as the class {1} is a duplicate, returning null.", xheFilePath, existingClass.Name);
                                    return null;
                                }
                            }
                        }
                    }

                    //TODO: Need to verify every property in every class AND every property in the entire hierarchy of a class has a unique ID
                    //If not, XUI writing will produce incorrect results, see: XuiShader using a duplicate "Id" property

                    foreach (XUClass deserializedClass in deserializedExtension.Extensions.Classes)
                    {
                        foreach (XUPropertyDefinition propertyDefinition in deserializedClass.PropertyDefinitions)
                        {
                            propertyDefinition.ParentClassName = deserializedClass.Name;
                        }
                    }

                    Logger?.Here().Verbose("Registered a total of {0} classes from {1}.", deserializedExtension.Extensions.Classes.Count, xheFilePath);
                    return deserializedExtension;
                }
            }
            catch (Exception ex)
            {
                Logger?.Here().Error("Caught an exception when trying to register XML extensions at {0}, returning null. The exception is: {1}", xheFilePath, ex);
                return null;
            }
        }
    }
}
