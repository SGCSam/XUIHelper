using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public class XUPoint
    {
        public float X { get; private set; }
        public float Y { get; private set; }

        public XUPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public XUPoint()
        {

        }

        public override string ToString()
        {
            return string.Format("X: {0}, Y: {1}", X, Y);
        }
    }
}
