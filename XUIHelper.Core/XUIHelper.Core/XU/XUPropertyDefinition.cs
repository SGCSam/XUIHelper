using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XUIHelper.Core
{
    [Serializable]
    public enum XUPropertyDefinitionTypes
    {
        Empty,

        [XmlEnum(Name = "bool")]
        Bool,

        [XmlEnum(Name = "integer")]
        Integer,

        [XmlEnum(Name = "unsigned")]
        Unsigned,

        [XmlEnum(Name = "float")]
        Float,

        [XmlEnum(Name = "string")]
        String,

        [XmlEnum(Name = "color")]
        Colour,

        [XmlEnum(Name = "vector")]
        Vector,

        [XmlEnum(Name = "quaternion")]
        Quaternion,

        [XmlEnum(Name = "object")]
        Object,

        [XmlEnum(Name = "custom")]
        Custom
    }

    [Serializable]
    public enum XUPropertyDefinitionFlags
    {
        [XmlEnum(Name = "indexed")]
        Indexed,

        [XmlEnum(Name = "hidden")]
        Hidden,

        [XmlEnum(Name = "localize")]
        Localized,

        [XmlEnum(Name = "noanim")]
        NoAnimation,

        [XmlEnum(Name = "filepath")]
        FilePath
    }

    [Serializable]
    public class XUPropertyDefinition
    {
        private string _FlagsString;

        [XmlAttribute("Id")]
        public int ID { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Type")]
        public XUPropertyDefinitionTypes Type { get; set; }

        [XmlAttribute("Flags")]
        public string FlagsString 
        { 
            get 
            { 
                return _FlagsString; 
            } 
            set 
            { 
                _FlagsString =  value;
                GetFlagsFromString();
            } 
        }

        [XmlElement("DefaultVal")]
        public XUDefaultValue DefaultValue { get; set; }

        [XmlIgnore]
        public HashSet<XUPropertyDefinitionFlags> FlagsSet { get; private set; } = new HashSet<XUPropertyDefinitionFlags>();

        private void GetFlagsFromString()
        {
            if(string.IsNullOrEmpty(FlagsString))
            {
                return;
            }

            foreach (string flag in FlagsString.Split('|'))
            {
                switch(flag)
                {
                    case "indexed":
                    {
                        FlagsSet.Add(XUPropertyDefinitionFlags.Indexed);
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }
            }
        }

        public XUPropertyDefinition(int id, string name, XUPropertyDefinitionTypes type, string flagsString, XUDefaultValue defaultVal)
        {
            ID = id;
            Name = name;
            Type = type;
            FlagsString = flagsString;
            DefaultValue = defaultVal;
        }

        public XUPropertyDefinition() 
        {

        }
    }
}
