// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiCollisionAvoidanceNeighborCollectorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiCollisionAvoidance.NeighborCollector>
{
    public hkaiCollisionAvoidanceNeighborCollectorData(Havok.hk2018.hkaiCollisionAvoidance.NeighborCollector instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_lineSegments":
            case "lineSegments":
            {
                value = instance.m_lineSegments;
                return true;
            }
            case "m_spheres":
            case "spheres":
            {
                value = instance.m_spheres;
                return true;
            }
            case "m_characters":
            case "characters":
            {
                value = instance.m_characters;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_lineSegments":
            case "lineSegments":
            {
                if (value is not List<Havok.hk2018.hkaiCollisionAvoidance.LineSegmentObstacle> castValue) return false;
                instance.m_lineSegments = castValue;
                return true;
            }
            case "m_spheres":
            case "spheres":
            {
                if (value is not List<Havok.hk2018.hkaiCollisionAvoidance.SphereObstacle> castValue) return false;
                instance.m_spheres = castValue;
                return true;
            }
            case "m_characters":
            case "characters":
            {
                if (value is not Havok.hk2018.hkaiMinArray<Havok.hk2018.hkaiCollisionAvoidance.Character?> castValue) return false;
                instance.m_characters = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
