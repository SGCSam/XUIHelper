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

        public override bool Equals(object? obj)
        {
            if (obj is not XUQuaternion xuQuaternion)
            {
                return false;
            }

            return xuQuaternion.X == X && xuQuaternion.Y == Y && xuQuaternion.Z == Z && xuQuaternion.W == W;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z, W);
        }
    }
}
