﻿using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class VECT5Section : IVECTSection
    {
        public List<XUVector> Vectors { get; private set; } = new List<XUVector>();

        public bool TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(VECT5Section));
                xur.Logger?.Here().Verbose("Reading VECT5 section.");

                XURSectionTableEntry? entry = xur.TryGetXURSectionTableEntryForMagic(IVECTSection.ExpectedMagic);
                if (entry == null)
                {
                    xur.Logger?.Here().Error("XUR section table entry was null, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Reading vectors from offset {0:X8}.", entry.Offset);
                reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

                int bytesRead;
                int vectIndex = 0;
                for (bytesRead = 0; bytesRead < entry.Length;)
                {
                    xur.Logger?.Here().Verbose("Reading vector index {0} from offset {1:X8}.", vectIndex, reader.BaseStream.Position);
                    float thisX = reader.ReadSingleBE();
                    float thisY = reader.ReadSingleBE();
                    float thisZ = reader.ReadSingleBE();
                    vectIndex++;
                    bytesRead += 12;

                    XUVector thisVector = new XUVector(thisX, thisY, thisZ);
                    Vectors.Add(thisVector);
                    xur.Logger?.Here().Verbose("Read vector index {0} as {1}.", vectIndex, thisVector);
                }

                xur.Logger?.Here().Verbose("Read vectors successfully, read a total of {0} vectors, {1:X8} bytes.", Vectors.Count, bytesRead);
                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading VECT5 section, returning false. The exception is: {0}", ex);
                return false;
            }
        }
    }
}
