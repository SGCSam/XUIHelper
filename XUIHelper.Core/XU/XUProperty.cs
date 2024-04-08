using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public class XUProperty
    {
        public XUPropertyDefinition PropertyDefinition { get; private set; }
        public object Value { get; private set; }

        public XUProperty(XUPropertyDefinition definition, object value)
        {
            PropertyDefinition = definition;
            Value = value;
        }

        public int GetChildValuesCount()
        {
            if (Value is not IList valueList)
            {
                return 1;
            }

            if(valueList is not List<XUProperty> childPropertiesList)
            {
                return valueList.Count;
            }

            int retCount = 1;
            foreach (XUProperty childProperty in childPropertiesList)
            {
                retCount += childProperty.GetChildValuesCount();
            }

            return retCount;
        }

        public int GetCompoundPropertiesCount()
        {
            if (PropertyDefinition.Type != XUPropertyDefinitionTypes.Object)
            {
                return 0;
            }

            int retCount = 1;
            foreach (XUProperty childProperty in (List<XUProperty>)Value)
            {
                retCount += childProperty.GetCompoundPropertiesCount();
            }

            return retCount;
        }
    }
}
