using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public class XUColour
    {
        public byte A { get; private set; } = 0;
        public byte R { get; private set; } = 0;
        public byte G { get; private set; } = 0;
        public byte B { get; private set; } = 0;

        public XUColour(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public XUColour()
        {

        }

        public override string ToString()
        {
            return string.Format("R: {0}, G: {1}, B: {2}, A: {3}", R, G, B, A);
        }
    }
}
