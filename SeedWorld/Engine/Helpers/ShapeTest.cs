using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SeedWorld
{
    public static class ShapeTest
    {
        /// <summary>
        /// Helper function to test points inside of a 3D ellipsoid
        /// </summary>
        public static bool InsideEllipsoid(Vector3 size, Vector3 center, Vector3 point)
        {
            float dx = point.X - center.X;
            float dy = point.Y - center.Y;
            float dz = point.Z - center.Z;

            return (
                (dx * dx) / (size.X * size.X) +
                (dy * dy) / (size.Y * size.Y) +
                (dz * dz) / (size.Z * size.Z) <= 1);
        }

        /// <summary>
        /// Helper function to test points inside of a sphere
        /// </summary>
        public static bool InsideSphere(float radius, Vector3 center, Vector3 point)
        {
            float dx = point.X - center.X;
            float dy = point.Y - center.Y;
            float dz = point.Z - center.Z;

            return (
                (dx * dx) / (radius * radius) +
                (dy * dy) / (radius * radius) +
                (dz * dz) / (radius * radius) <= 1);
        }

        /// <summary>
        /// Check if a point is inside the bounds of a chunk
        /// </summary>
        public static bool InsideChunkBounds(int size, float x, float z, float y)
        {
            return ((x >= 0 && x < size + 2) && 
                (z >= 0 && z < size + 2) && y < size);
        }

        /// <summary>
        /// Pick random points on a sphere
        /// </summary>
        public static Vector3[] GoldenSpiralPoints(int n, int side = -1)
        {
            List<Vector3> points = new List<Vector3>();

            float inc = (float)(Math.PI * (3 - Math.Sqrt(5)));
            float offset = 2f / n;
            for (int i = 0; i < n; i++)
            {
                float y = i * offset - 1 + (offset / 2f);
                float r = (float)Math.Sqrt(1 - y * y);
                float phi = i * inc;
                Vector3 point = new Vector3((float)Math.Cos(phi) * r, y, (float)Math.Sin(phi) * r);

                // Cull points to be added for each side
                // Top and bottom
                if (side == 4 && point.Y > 0.2f)  points.Add(point);
                if (side == 5 && point.Y < -0.2f) points.Add(point);

                // Left and right
                if (side == 0 && point.X < -0.2f) points.Add(point);
                if (side == 1 && point.X > 0.2f) points.Add(point);

                // Front and back
                if (side == 2 && point.Z > 0.2f)  points.Add(point);
                if (side == 3 && point.Z < -0.2f) points.Add(point);
            }

            return points.ToArray();
        }
    }
}
