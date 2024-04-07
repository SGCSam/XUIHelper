using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public static class XUIHelperCoreUtilities
    {
        public static bool IsStringValidPath(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    Path.GetFullPath(path);
                    if(path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                    {
                        return false;
                    }

                    return Path.IsPathRooted(path);
                }

                return false;
            }

            catch
            {
                return false;
            }
        }
    }
}
