using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class XUR8CountHeader : IXURCountHeader
    {

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(XUR8CountHeader));
                xur.Logger?.Here().Verbose("Reading XUR8 count header.");

                //TODO: ...
                reader.ReadBytes(0xE);

                xur.Logger?.Here().Verbose("XUR8 count header read successful!");
                return true;

            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading XUR8 count header, returning false. The exception is: {0}", ex);
                return false;
            }
        }

        public bool TryVerify(IXUR xur)
        {
            try
            {
                xur.Logger?.Here().Verbose("XUR8 count header verified successfully!");
                //TODO: ...
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when verifying XUR8 count header, returning false. The exception is: {0}", ex);
                return false;
            }
        }
    }
}
