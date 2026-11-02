// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiCollisionAvoidanceNeighborCollectorData : HavokData<HKLib.hk2019.hkaiCollisionAvoidance.NeighborCollector> 
{
    public hkaiCollisionAvoidanceNeighborCollectorData(HavokType type, HKLib.hk2019.hkaiCollisionAvoidance.NeighborCollector instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_lineSegments":
            case "lineSegments":
            {
                if (instance.m_lineSegments is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_spheres":
            case "spheres":
            {
                if (instance.m_spheres is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characters":
            case "characters":
            {
                if (instance.m_characters is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_lineSegments":
            case "lineSegments":
            {
                if (value is not List<HKLib.hk2019.hkaiCollisionAvoidance.LineSegmentObstacle> castValue) return false;
                instance.m_lineSegments = castValue;
                return true;
            }
            case "m_spheres":
            case "spheres":
            {
                if (value is not List<HKLib.hk2019.hkaiCollisionAvoidance.SphereObstacle> castValue) return false;
                instance.m_spheres = castValue;
                return true;
            }
            case "m_characters":
            case "characters":
            {
                if (value is not HKLib.hk2019.hkaiMinArray<HKLib.hk2019.hkaiCollisionAvoidance.Character?> castValue) return false;
                instance.m_characters = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
