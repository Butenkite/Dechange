using System;
using UnityEngine;

namespace Dechange
{
    /// <summary>
    /// Double-precision 3D vector for orbital math. Unity's Vector3 is single-precision,
    /// which breaks at solar-system distances (~AU scale in km).
    /// </summary>
    public struct Vector3d
    {
        public double x, y, z;

        public static readonly Vector3d Zero = new(0, 0, 0);

        public Vector3d(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public double Magnitude => Math.Sqrt(x * x + y * y + z * z);

        public static Vector3d operator +(Vector3d a, Vector3d b) => new(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3d operator -(Vector3d a, Vector3d b) => new(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3d operator *(Vector3d a, double s) => new(a.x * s, a.y * s, a.z * s);

        public Vector3 ToVector3() => new((float)x, (float)y, (float)z);

        public override string ToString() => $"({x:F1}, {y:F1}, {z:F1})";
    }
}
