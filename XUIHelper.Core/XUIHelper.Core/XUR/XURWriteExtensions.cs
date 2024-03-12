using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public static class XURWriteExtensions
    {
        public static int? TryWriteProperty(this IXUR xur, BinaryWriter writer, XUProperty property)
        {
            try
            {
                if (xur is XUR5 xur5)
                {
                    xur.Logger?.Here().Verbose("Trying to write XUR5 property {0}", property.PropertyDefinition.Name);
                    return xur5.TryWriteProperty(writer, property);
                }

                xur.Logger?.Here().Error("Unhandled IXUR for property {0}, returning null.", property.PropertyDefinition.Name);
                return null;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when trying to write property {0}, returning null. The exception is: {1}", property.PropertyDefinition.Name, ex);
                return null;
            }
        }
    }
}
