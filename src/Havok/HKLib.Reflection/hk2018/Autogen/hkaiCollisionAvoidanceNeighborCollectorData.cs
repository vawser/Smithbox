// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiCollisionAvoidance;

namespace HKLib.Reflection.hk2018;

internal class hkaiCollisionAvoidanceNeighborCollectorData : HavokData<NeighborCollector> 
{
    public hkaiCollisionAvoidanceNeighborCollectorData(HavokType type, NeighborCollector instance) : base(type, instance) {}

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
                if (value is not List<LineSegmentObstacle> castValue) return false;
                instance.m_lineSegments = castValue;
                return true;
            }
            case "m_spheres":
            case "spheres":
            {
                if (value is not List<SphereObstacle> castValue) return false;
                instance.m_spheres = castValue;
                return true;
            }
            case "m_characters":
            case "characters":
            {
                if (value is not hkaiMinArray<Character?> castValue) return false;
                instance.m_characters = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
