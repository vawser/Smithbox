// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingGetTrimeshOutputData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetTrimeshOutput> 
{
    public hkaiNavMeshGenerationSnapshottingGetTrimeshOutputData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetTrimeshOutput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_trimesh":
            case "trimesh":
            {
                if (instance.m_trimesh is null)
                {
                    return true;
                }
                if (instance.m_trimesh is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_trimesh":
            case "trimesh":
            {
                if (value is null)
                {
                    instance.m_trimesh = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkGeometry castValue)
                {
                    instance.m_trimesh = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
