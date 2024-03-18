using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core.Extensions
{
    public static class ListExtensions
    {
        public static int GetSequenceIndex(this IList list, IList other, int startingIndex = 0)
        {
            for(int i = startingIndex; i < list.Count; i++)
            {
                int matchingCount = 0;
                for(int j = 0; j < other.Count; j++)
                {
                    if (list[i + j].Equals(other[j]))
                    {
                        matchingCount++;
                    }
                    else
                    {
                        break;
                    }
                }

                if(matchingCount == other.Count)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
