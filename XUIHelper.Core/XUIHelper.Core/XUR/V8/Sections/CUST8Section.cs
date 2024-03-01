using Microsoft.VisualBasic;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUIHelper.Core.Extensions;

namespace XUIHelper.Core
{
    public class CUST8Section : ICUSTSection
    {
        public int Magic { get { return ICUSTSection.ExpectedMagic; } }

        public List<XUFigure> Figures { get; private set; } = new List<XUFigure>();

        public async Task<bool> TryReadAsync(IXUR xur, BinaryReader reader)
        {
            try
            {
                xur.Logger = xur.Logger?.ForContext(typeof(CUST8Section));
                xur.Logger?.Here().Verbose("Reading CUST8 section.");

                XURSectionTableEntry? entry = xur.TryGetXURSectionTableEntryForMagic(ICUSTSection.ExpectedMagic);
                if (entry == null)
                {
                    xur.Logger?.Here().Error("XUR section table entry was null, returning false.");
                    return false;
                }

                xur.Logger?.Here().Verbose("Reading customs from offset {0:X8}.", entry.Offset);
                reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

                int bytesRead;
                int custIndex = 0;
                for (bytesRead = 0; bytesRead < entry.Length;)
                {
                    xur.Logger?.Here().Verbose("Reading custom at index {0}", custIndex);

                    int dataLength = reader.ReadInt32BE();
                    xur.Logger?.Here().Verbose("Got a custom data length of {0:X8}", dataLength);
                    bytesRead += 4;

                    int dataRead = 0;
                    float boundingBoxX = reader.ReadSingleBE();
                    float boundingBoxY = reader.ReadSingleBE();
                    dataRead += 8;
                    XUPoint boundingBox = new XUPoint(boundingBoxX, boundingBoxY);
                    xur.Logger?.Here().Verbose("Got a bounding box of {0}", boundingBox);

                    int bezierPointsCount = reader.ReadInt32BE();
                    dataRead += 4;
                    xur.Logger?.Here().Verbose("Got a bezier points count of {0:X8}", bezierPointsCount);

                    List<XUBezierPoint> bezierPoints = new List<XUBezierPoint>();
                    for (int pointsIndex = 0; pointsIndex < bezierPointsCount; pointsIndex++)
                    {
                        xur.Logger?.Here().Verbose("Reading bezier point index {0}", pointsIndex);

                        float vectX = reader.ReadSingleBE();
                        float vectY = reader.ReadSingleBE();
                        dataRead += 8;
                        XUPoint vectPoint = new XUPoint(vectX, vectY);
                        xur.Logger?.Here().Verbose("Got vector point of {0}", vectPoint);

                        float oneX = reader.ReadSingleBE();
                        float oneY = reader.ReadSingleBE();
                        dataRead += 8;
                        XUPoint controlOnePoint = new XUPoint(oneX, oneY);
                        xur.Logger?.Here().Verbose("Got control one point of {0}", controlOnePoint);

                        float twoX = reader.ReadSingleBE();
                        float twoY = reader.ReadSingleBE();
                        dataRead += 8;
                        XUPoint controlTwoPoint = new XUPoint(twoX, twoY);
                        xur.Logger?.Here().Verbose("Got control two point of {0}", controlTwoPoint);

                        bezierPoints.Add(new XUBezierPoint(vectPoint, controlOnePoint, controlTwoPoint));
                    }

                    if (dataRead != dataLength)
                    {
                        xur.Logger?.Here().Error("Mismatch between the amount of data read and the length, returning false. Expected: {0:X8}, Actual: {1:X8}", dataLength, dataRead);
                        return false;
                    }

                    bytesRead += dataRead;
                    Figures.Add(new XUFigure(boundingBox, bezierPoints));
                    xur.Logger?.Here().Verbose("Read custom at index {0} successfully!", custIndex);
                    custIndex++;
                }

                return true;
            }
            catch (Exception ex)
            {
                xur.Logger?.Here().Error("Caught an exception when reading CUST8 section, returning false. The exception is: {0}", ex);
                return false;
            }
        }
    }
}
