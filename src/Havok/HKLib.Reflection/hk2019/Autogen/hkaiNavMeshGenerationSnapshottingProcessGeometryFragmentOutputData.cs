// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingProcessGeometryFragmentOutputData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFragmentOutput> 
{
    public hkaiNavMeshGenerationSnapshottingProcessGeometryFragmentOutputData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFragmentOutput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_useAsFace":
            case "useAsFace":
            {
                if (instance.m_useAsFace is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faceData":
            case "faceData":
            {
                if (instance.m_faceData is not TGet castValue) return false;
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
            case "m_useAsFace":
            case "useAsFace":
            {
                if (value is not bool castValue) return false;
                instance.m_useAsFace = castValue;
                return true;
            }
            case "m_faceData":
            case "faceData":
            {
                if (value is not List<int> castValue) return false;
                instance.m_faceData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
