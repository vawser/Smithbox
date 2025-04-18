// Automatically Generated

namespace HKLib.hk2018;

public interface hkaiOverlappingTriangles : IHavokObject
{

    public enum WalkableTriangleSettings : int
    {
        ONLY_FIX_WALKABLE = 0,
        PREFER_WALKABLE = 1,
        PREFER_UNWALKABLE = 2
    }

    public class Settings : IHavokObject
    {
        public float m_coplanarityTolerance;

        public float m_raycastLengthMultiplier;

        public hkaiOverlappingTriangles.WalkableTriangleSettings m_walkableTriangleSettings;

    }


}

