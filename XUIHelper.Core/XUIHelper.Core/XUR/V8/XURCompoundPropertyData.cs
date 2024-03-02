using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public class XURCompoundPropertyData
    {
        public int Index { get; private set; }
        public List<XUProperty> Properties { get; private set; } = new List<XUProperty>();

        public XURCompoundPropertyData(int index, List<XUProperty> properties)
        {
            Index = index;
            Properties = properties;
        }
    }
}
