using System;

namespace DemoScanner
{
    public struct FPoint3D
    {
        public float X;
        public float Y;
        public float Z;

        public FPoint3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override bool Equals(object obj)
        {
            return obj is FPoint3D d &&
                   Math.Abs(X - d.X) < 0.00001f &&
                  Math.Abs(Y - d.Y) < 0.00001f &&
                   Math.Abs(Z - d.Z) < 0.00001f;
        }

        public override int GetHashCode()
        {
            int hashCode = -307843816;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(FPoint3D per1, FPoint3D per2)
        {
            return !(per1 != per2);
        }

        public static bool operator !=(FPoint3D per1, FPoint3D per2)
        {
            return Math.Abs(per1.X - per2.X) > 0.00001f || Math.Abs(per1.Y - per2.Y) > 0.00001f || Math.Abs(per1.Z - per2.Z) > 0.00001f;
        }
    }
}