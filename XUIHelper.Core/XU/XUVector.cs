using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUIHelper.Core
{
    public class XUVector
    {
        public float X { get; private set; } = 0.0f;
        public float Y { get; private set; } = 0.0f;
        public float Z { get; private set; } = 0.0f;

        public XUVector(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public XUVector()
        {

        }

        public override string ToString()
        {
            return string.Format("X: {0}, Y: {1}, Z: {2}", X, Y, Z);
        }

        public override bool Equals(object? obj)
        {
            if(obj is not XUVector xuVector)
            {
                return false;
            }

            return xuVector.X == X && xuVector.Y == Y && xuVector.Z == Z;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }
    }
}
