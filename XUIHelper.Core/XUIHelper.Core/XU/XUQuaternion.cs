using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public class XUQuaternion
    {
        public float X { get; private set; } = 0.0f;
        public float Y { get; private set; } = 0.0f;
        public float Z { get; private set; } = 0.0f;
        public float W { get; private set; } = 0.0f;

        public XUQuaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public XUQuaternion()
        {

        }

        public override string ToString()
        {
            return string.Format("X: {0}, Y: {1}, Z: {2}, W: {3}", X, Y, Z, W);
        }
    }
}
