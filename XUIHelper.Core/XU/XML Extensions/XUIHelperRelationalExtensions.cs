using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XUIHelper.Core
{
    [Serializable]
    public class XUIHelperRelationalExtensions
    {
        [XmlElement("RelationalExtension")]
        public List<XUIHelperRelationalExtension> RelationalExtensions { get; set; } = new List<XUIHelperRelationalExtension>();

        public XUIHelperRelationalExtensions()
        {

        }
    }
}
