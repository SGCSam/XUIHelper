﻿using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class XUR5 : XUR
    {
        public XUR5(string filePath, ILogger? logger = null) : base(filePath, new XUR5Header(), new XUR5SectionsTable(), logger)
        {

        }

        protected override IXURSection? TryGetXURSectionForMagic(int Magic)
        {
            try
            {
                Logger = Logger?.ForContext(typeof(XUR5));

                Logger?.Here().Verbose("Trying to get XUR section for magic {0:X8}", Magic);

                switch (Magic)
                {
                    case ISTRNSection.ExpectedMagic:
                    {
                        Logger?.Here().Verbose("Returning STRN5 section.");
                        return new STRN5Section();
                    }
                    case IVECTSection.ExpectedMagic:
                    {
                        Logger?.Here().Verbose("Returning VECT5 section.");
                        return new VECT5Section();
                    }    
                    default:
                    {
                        Logger?.Here().Error("Hit default case, unhandled magic.");
                        return null;
                    }
                }
            }
            catch (Exception ex) 
            {
                Logger?.Here().Error("Caught an exception when trying to get XUR section for magic {0:X8}, returning false. The exception is: {1}", Magic, ex);
                return null;
            }
        }
    }
}
