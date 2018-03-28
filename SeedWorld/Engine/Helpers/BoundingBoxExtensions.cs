using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SeedWorld
{
    /// <summary>
    /// A set of helpful methods for working with rectangles.
    /// </summary>
    public static class BoundingBoxExtensions
    {
        /// <summary>
        /// Calculates the signed depth of intersection between two bounding boxes.
        /// </summary>
        /// <returns>
        /// The amount of overlap between two intersecting rectangles. These
        /// depth values can be negative depending on which sides the rectangles
        /// intersect. This allows callers to determine the correct direction
        /// to push objects in order to resolve collisions.
        /// If the rectangles are not intersecting, Vector2.Zero is returned.
        /// </returns>
        public static Vector3 GetIntersectionDepth(this BoundingBox boxA, BoundingBox boxB)
        {
            // Calculate half sizes.
            float halfWidthA = Math.Abs(boxA.Max.X - boxA.Min.X) / 2.0f;
            float halfHeightA = Math.Abs(boxA.Max.Y - boxA.Min.Y) / 2.0f;
            float halfDepthA = Math.Abs(boxA.Max.Z - boxA.Min.Z) / 2.0f;

            float halfWidthB = Math.Abs(boxB.Max.X - boxB.Min.X) / 2.0f;
            float halfHeightB = Math.Abs(boxB.Max.Y - boxB.Min.Y) / 2.0f;
            float halfDepthB = Math.Abs(boxB.Max.Z - boxB.Min.Z) / 2.0f;

            // Calculate centers.
            Vector3 centerA = boxA.Min + new Vector3(halfWidthA, halfHeightA, halfDepthA);
            Vector3 centerB = boxB.Min + new Vector3(halfWidthB, halfHeightB, halfDepthB);

            // Calculate current and minimum-non-intersecting distances between centers.
            float distanceX = centerA.X - centerB.X;
            float distanceY = centerA.Y - centerB.Y;
            float distanceZ = centerA.Z - centerB.Z;

            float minDistanceX = halfWidthA + halfWidthB;
            float minDistanceY = halfHeightA + halfHeightB;
            float minDistanceZ = halfDepthA + halfDepthB;

            // If we are not intersecting at all, return (0, 0).
            if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY || 
                Math.Abs(distanceZ) >= minDistanceZ)
                return Vector3.Zero;

            // Calculate and return intersection depths.
            float depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
            float depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;
            float depthZ = distanceZ > 0 ? minDistanceZ - distanceZ : -minDistanceZ - distanceZ;

            return new Vector3(depthX, depthY, depthZ);
        }

        /// <summary>
        /// Gets the position of the center of the bottom edge of the rectangle.
        /// </summary>
        public static Vector2 GetBottomCenter(this Rectangle rect)
        {
            return new Vector2(rect.X + rect.Width / 2.0f, rect.Bottom);
        }
    }
}
