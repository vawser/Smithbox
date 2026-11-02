// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingGeometryFacesIntersectXyPrismInputData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryFacesIntersectXyPrismInput> 
{
    public hkaiNavMeshGenerationSnapshottingGeometryFacesIntersectXyPrismInputData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryFacesIntersectXyPrismInput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_prism":
            case "prism":
            {
                if (instance.m_prism is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_prismAabb":
            case "prismAabb":
            {
                if (instance.m_prismAabb is not TGet castValue) return false;
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
            case "m_prism":
            case "prism":
            {
                if (value is not HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryFacesIntersectXyPrismInput.XyPrism castValue) return false;
                instance.m_prism = castValue;
                return true;
            }
            case "m_prismAabb":
            case "prismAabb":
            {
                if (value is not HKLib.hk2019.hkAabb castValue) return false;
                instance.m_prismAabb = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
