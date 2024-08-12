using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace XUIHelper.Core
{
    [Serializable]
    public class XUIHelperRelationalExtension
    {
        [XmlText]
        public string RelativeFilePath { get; set; } = string.Empty;

        public XUIHelperRelationalExtension()
        {

        }
    }
}
