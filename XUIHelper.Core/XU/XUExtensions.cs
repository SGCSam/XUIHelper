using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public static class XUExtensions
    {
        public static int? TryGetPropertiesListIndex(this List<List<XUProperty>> propertiesList, List<XUProperty> propertiesToFind)
        {
            int addedPropertiesIndex = 0;
            foreach (List<XUProperty> addedProperties in propertiesList)
            {
                if (addedProperties.Count == propertiesToFind.Count)
                {
                    bool allMatch = true;
                    for (int i = 0; i < addedProperties.Count; i++)
                    {
                        if (!addedProperties[i].Value.Equals(propertiesToFind[i].Value))
                        {
                            allMatch = false;
                            break;
                        }

                        if (addedProperties[i].PropertyDefinition != propertiesToFind[i].PropertyDefinition)
                        {
                            allMatch = false;
                            break;
                        }
                    }

                    if (allMatch)
                    {
                        return addedPropertiesIndex;
                    }
                }

                addedPropertiesIndex++;
            }

            return null;
        }
    }
}
