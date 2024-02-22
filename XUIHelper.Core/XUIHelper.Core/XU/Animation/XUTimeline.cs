using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public class XUTimeline
    {
        public string ElementName { get; private set; }
        public List<XUKeyframe> Keyframes { get; private set; } = new List<XUKeyframe>();

        public XUTimeline(string elementName, List<XUKeyframe> keyframes) 
        { 
            ElementName = elementName;
            Keyframes = keyframes;
        }
    }
}
