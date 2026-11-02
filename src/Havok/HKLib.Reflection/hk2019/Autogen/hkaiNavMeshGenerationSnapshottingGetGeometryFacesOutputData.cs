// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingGetGeometryFacesOutputData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFacesOutput> 
{
    public hkaiNavMeshGenerationSnapshottingGetGeometryFacesOutputData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetGeometryFacesOutput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_geometryFaceKeys":
            case "geometryFaceKeys":
            {
                if (instance.m_geometryFaceKeys is not TGet castValue) return false;
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
            case "m_geometryFaceKeys":
            case "geometryFaceKeys":
            {
                if (value is not List<HKLib.hk2019.hkHandle<uint>> castValue) return false;
                instance.m_geometryFaceKeys = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
