namespace DemoScanner
{
    public struct Point3D
    {
        public float X;
        public float Y;
        public float Z;

        public Point3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override bool Equals(object obj)
        {
            return obj is Point3D d &&
                   X == d.X &&
                   Y == d.Y &&
                   Z == d.Z;
        }

        public override int GetHashCode()
        {
            int hashCode = -307843816;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            hashCode = hashCode * -1521134295 + Z.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Point3D per1, Point3D per2)
        {
            return !(per1 != per2);
        }

        public static bool operator !=(Point3D per1, Point3D per2)
        {
            return per1.X != per2.X || per1.Y != per2.Y || per1.Z != per2.Z;
        }
    }

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
                   X == d.X &&
                   Y == d.Y &&
                   Z == d.Z;
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
            return per1.X != per2.X || per1.Y != per2.Y || per1.Z != per2.Z;
        }
    }
}