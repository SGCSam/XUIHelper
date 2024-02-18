using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public static class XURReadExtensions
    {
        public static XUProperty? TryReadProperty(this IXUR xur, BinaryReader reader, XUPropertyDefinition propertyDefinition)
        {
            try
            {
                if (xur is XUR5 xur5)
                {
                    xur.Logger?.Here().Verbose("Trying to read XUR5 property {0}", propertyDefinition.Name);
                    return xur5.TryReadProperty(reader, propertyDefinition);
                }

                xur.Logger?.Here().Error("Unhandled IXUR for property {0}, returning null.", propertyDefinition.Name);
                return null;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to read property {0}, returning null. The exception is: {1}", propertyDefinition.Name, ex);
                return null;
            }
        }
    }
}
