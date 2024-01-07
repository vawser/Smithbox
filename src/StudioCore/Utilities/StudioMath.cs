using System.Numerics;

namespace StudioCore.Utilities;

public static class StudioMath
{
    public static bool EpsEqual(this float flt, float otherFloat, float epsilon)
    {
        return otherFloat <= flt + epsilon && otherFloat >= flt - epsilon;
    }

    public static bool EpsGreaterThanOrEqual(this float flt, float otherFloat, float epsilon)
    {
        return flt > otherFloat || EpsEqual(flt, otherFloat, epsilon);
    }

    public static bool EpsLessThanOrEqual(this float flt, float otherFloat, float epsilon)
    {
        return flt < otherFloat || EpsEqual(flt, otherFloat, epsilon);
    }

    public static bool EpsEqual(this Vector3 vec3, Vector3 otherVec3, float epsilon)
    {
        return EpsEqual(vec3.X, otherVec3.X, epsilon) && EpsEqual(vec3.Y, otherVec3.Y, epsilon) && EpsEqual(vec3.Z, otherVec3.Z, epsilon);
    }

    public static bool GreaterThanOrEqualToPoint(this Vector3 vec3, Vector3 boundingVec3, float epsilon)
    {
        return vec3.X.EpsGreaterThanOrEqual(boundingVec3.X, epsilon) && vec3.Y.EpsGreaterThanOrEqual(boundingVec3.Y, epsilon) && vec3.Z.EpsGreaterThanOrEqual(boundingVec3.Z, epsilon);
    }
    public static bool LessThanOrEqualToPoint(this Vector3 vec3, Vector3 boundingVec3, float epsilon)
    {
        return vec3.X.EpsLessThanOrEqual(boundingVec3.X, epsilon) && vec3.Y.EpsLessThanOrEqual(boundingVec3.Y, epsilon) && vec3.Z.EpsLessThanOrEqual(boundingVec3.Z, epsilon);
    }

    public static bool WithinBounds(this Vector3 point, Vector3 maxBounds, Vector3 minBounds, float epsilon)
    {
        return point.GreaterThanOrEqualToPoint(minBounds, epsilon) && point.LessThanOrEqualToPoint(maxBounds, epsilon);
    }

    public static bool BoundsIntersect(Vector3 maxBounds, Vector3 minBounds, Vector3 maxBounds2, Vector3 minBounds2, float epsilon)
    {
        bool xTest = CheckAxisBounds(maxBounds.X, minBounds.X, maxBounds2.X, minBounds2.X, epsilon);
        bool yTest = CheckAxisBounds(maxBounds.Y, minBounds.Y, maxBounds2.Y, minBounds2.Y, epsilon);
        bool zTest = CheckAxisBounds(maxBounds.Z, minBounds.Z, maxBounds2.Z, minBounds2.Z, epsilon);
        return xTest && yTest && zTest;
    }

    public static bool CheckAxisBounds(float max0, float min0, float max1, float min1, float epsilon)
    {
        bool max0GreaterThanMin1 = EpsGreaterThanOrEqual(max0, min1, epsilon);
        bool max0GreaterThanMax1 = EpsGreaterThanOrEqual(max0, max1, epsilon);
        bool min0GreaterThanMin1 = EpsGreaterThanOrEqual(min0, min1, epsilon);
        bool max1GreaterThanMin0 = EpsGreaterThanOrEqual(max1, min0, epsilon);
        bool max1GreaterThanMax0 = EpsGreaterThanOrEqual(max1, max0, epsilon);
        bool min1GreaterThanMin0 = EpsGreaterThanOrEqual(min1, min0, epsilon);

        return ((min1GreaterThanMin0 && max0GreaterThanMin1) ||
                (max1GreaterThanMin0 && max0GreaterThanMax1) ||
                (min0GreaterThanMin1 && max1GreaterThanMin0) ||
                (max0GreaterThanMin1 && max1GreaterThanMax0));
    }
}
