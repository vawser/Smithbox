// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingProcessGeometryFaceInputData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceInput> 
{
    public hkaiNavMeshGenerationSnapshottingProcessGeometryFaceInputData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFaceInput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_geometryFaceKey":
            case "geometryFaceKey":
            {
                if (instance.m_geometryFaceKey is not TGet castValue) return false;
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
            case "m_geometryFaceKey":
            case "geometryFaceKey":
            {
                if (value is not HKLib.hk2019.hkHandle<uint> castValue) return false;
                instance.m_geometryFaceKey = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
