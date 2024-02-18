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
    public class XUPropertyDefinition
    {
        [XmlAttribute("Id")]
        public int ID { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Type")]
        public XUPropertyDefinitionTypes Type { get; set; }

        [XmlElement("DefaultVal")]
        public XUDefaultValue DefaultValue { get; set; }

        public XUPropertyDefinition(int id, string name, XUPropertyDefinitionTypes type, XUDefaultValue defaultVal)
        {
            ID = id;
            Name = name;
            Type = type;
            DefaultValue = defaultVal;
        }

        public XUPropertyDefinition() 
        {
        
        } 
    }
}
