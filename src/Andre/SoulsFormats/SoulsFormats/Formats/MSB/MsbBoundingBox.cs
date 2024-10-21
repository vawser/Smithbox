using System;
using System.Numerics;

namespace SoulsFormats
{
    public struct MsbBoundingBox
    {
        /// <summary>
        /// The minimum extent of the bounding box.
        /// </summary>
        public Vector3 Min;

        /// <summary>
        /// The maximum extent of the bounding box.
        /// </summary>
        public Vector3 Max;

        /// <summary>
        /// The origin of the bounding box, calculated from the minimum and maximum extent.
        /// </summary>
        public Vector3 Origin { get; init; }

        /// <summary>
        /// The distance between the furthest vertex of the bounding box and origin.
        /// </summary>
        // Does not seem entirely accurate, but is very close, might just be precision differences.
        public float Radius { get; init; }

        /// <summary>
        /// Create a new <see cref="MsbBoundingBox"/>.
        /// </summary>
        /// <param name="min">The minimum extent of the bounding box.</param>
        /// <param name="max">The maximum extent of the bounding box.</param>
        public MsbBoundingBox(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
            Origin = new Vector3((min.X + max.X) / 2, (min.Y + max.Y) / 2, (min.Z + max.Z) / 2);
            Radius = CalculateRadius(min, max, Origin);
        }

        /// <summary>
        /// Create a new <see cref="MsbBoundingBox"/>.
        /// </summary>
        /// <param name="min">The minimum extent of the bounding box.</param>
        /// <param name="max">The maximum extent of the bounding box.</param>
        /// <param name="origin">The origin of the bounding box, calculated from the minimum and maximum extent.</param>
        /// <param name="radius">The distance between the furthest vertex of the bounding box and origin.</param>
        internal MsbBoundingBox(Vector3 min, Vector3 max, Vector3 origin, float radius)
        {
            Min = min;
            Max = max;
            Origin = origin;
            Radius = radius;
        }

        /// <summary>
        /// Calculate the <see cref="Radius"/> value for an <see cref="MsbBoundingBox"/>.
        /// </summary>
        public static float CalculateRadius(Vector3 min, Vector3 max)
            => CalculateRadius(min, max, new Vector3((min.X + max.X) / 2, (min.Y + max.Y) / 2, (min.Z + max.Z) / 2));

        /// <summary>
        /// Calculate the <see cref="Radius"/> value for an <see cref="MsbBoundingBox"/>.
        /// </summary>
        private static float CalculateRadius(Vector3 min, Vector3 max, Vector3 origin)
        {
            // Get the position of each point on the bounding box
            Span<Vector3> vertices =
            [
                new Vector3(min.X, min.Y, max.Z),
                new Vector3(max.X, min.Y, max.Z),
                new Vector3(min.X, max.Y, max.Z),
                new Vector3(max.X, max.Y, max.Z),
                new Vector3(min.X, min.Y, min.Z),
                new Vector3(max.X, min.Y, min.Z),
                new Vector3(min.X, max.Y, min.Z),
                new Vector3(max.X, max.Y, min.Z),
            ];

            // Calculate the distances of each point from the origin
            Span<float> distances =
            [
                Vector3.Distance(origin, vertices[0]),
                Vector3.Distance(origin, vertices[1]),
                Vector3.Distance(origin, vertices[2]),
                Vector3.Distance(origin, vertices[3]),
                Vector3.Distance(origin, vertices[4]),
                Vector3.Distance(origin, vertices[5]),
                Vector3.Distance(origin, vertices[6]),
                Vector3.Distance(origin, vertices[7]),
             ];

            // Find the max distance value
            float maxDist = distances[0];
            for (int i = 1; i < 7; i++)
            {
                float distance = distances[i];
                if (distance > maxDist)
                {
                    maxDist = distance;
                }
            }

            return maxDist;
        }
    }
}
