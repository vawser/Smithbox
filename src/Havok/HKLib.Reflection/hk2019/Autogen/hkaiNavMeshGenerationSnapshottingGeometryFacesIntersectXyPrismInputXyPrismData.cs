// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingGeometryFacesIntersectXyPrismInputXyPrismData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryFacesIntersectXyPrismInput.XyPrism> 
{
    public hkaiNavMeshGenerationSnapshottingGeometryFacesIntersectXyPrismInputXyPrismData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryFacesIntersectXyPrismInput.XyPrism instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_planes":
            case "planes":
            {
                if (instance.m_planes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_points":
            case "points":
            {
                if (instance.m_points is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minZ":
            case "minZ":
            {
                if (instance.m_minZ is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxZ":
            case "maxZ":
            {
                if (instance.m_maxZ is not TGet castValue) return false;
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
            case "m_planes":
            case "planes":
            {
                if (value is not List<HKLib.hk2019.hkaiClipper.QuadriplanarInnerTypes.LineEqn> castValue) return false;
                instance.m_planes = castValue;
                return true;
            }
            case "m_points":
            case "points":
            {
                if (value is not List<HKLib.hk2019.hkaiClipper.QuadriplanarInnerTypes.Vec2> castValue) return false;
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
