using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XUIHelper.Core
{
    [Serializable]
    public class XUDefaultValue
    {
        [XmlText]
        public string Value { get; set; }

        public XUDefaultValue(string value)
        {
            Value = value;
        }

        public XUDefaultValue() 
        { 
        
        } 
    }
}
