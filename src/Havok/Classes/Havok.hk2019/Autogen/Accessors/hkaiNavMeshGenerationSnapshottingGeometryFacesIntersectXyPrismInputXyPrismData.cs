// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingGeometryFacesIntersectXyPrismInputXyPrismData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryFacesIntersectXyPrismInput.XyPrism>
{
    public hkaiNavMeshGenerationSnapshottingGeometryFacesIntersectXyPrismInputXyPrismData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryFacesIntersectXyPrismInput.XyPrism instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_planes":
            case "planes":
            {
                value = instance.m_planes;
                return true;
            }
            case "m_points":
            case "points":
            {
                value = instance.m_points;
                return true;
            }
            case "m_minZ":
            case "minZ":
            {
                value = instance.m_minZ;
                return true;
            }
            case "m_maxZ":
            case "maxZ":
            {
                value = instance.m_maxZ;
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
            case "m_planes":
            case "planes":
            {
                if (value is not List<Havok.hk2019.hkaiClipper.QuadriplanarInnerTypes.LineEqn> castValue) return false;
                instance.m_planes = castValue;
                return true;
            }
            case "m_points":
            case "points":
            {
                if (value is not List<Havok.hk2019.hkaiClipper.QuadriplanarInnerTypes.Vec2> castValue) return false;
                instance.m_points = castValue;
                return true;
            }
            case "m_minZ":
            case "minZ":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_minZ = castValue;
                return true;
            }
            case "m_maxZ":
            case "maxZ":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_maxZ = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
