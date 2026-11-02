// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingGetTrimeshTrianglesForGeometryFaceOutputData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetTrimeshTrianglesForGeometryFaceOutput> 
{
    public hkaiNavMeshGenerationSnapshottingGetTrimeshTrianglesForGeometryFaceOutputData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetTrimeshTrianglesForGeometryFaceOutput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_indices":
            case "indices":
            {
                if (instance.m_indices is not TGet castValue) return false;
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
            case "m_indices":
            case "indices":
            {
                if (value is not List<int> castValue) return false;
                instance.m_indices = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
