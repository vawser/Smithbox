// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingGetGeometryFaceMaterialOutputData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFaceMaterialOutput> 
{
    public hkaiNavMeshGenerationSnapshottingGetGeometryFaceMaterialOutputData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFaceMaterialOutput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_geometryFaceMaterial":
            case "geometryFaceMaterial":
            {
                if (instance.m_geometryFaceMaterial is not TGet castValue) return false;
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
            case "m_geometryFaceMaterial":
            case "geometryFaceMaterial":
            {
                if (value is not HKLib.hk2019.hkHandle<uint> castValue) return false;
                instance.m_geometryFaceMaterial = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
