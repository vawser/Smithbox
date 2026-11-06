// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingProcessGeometryFragmentOutputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFragmentOutput>
{
    public hkaiNavMeshGenerationSnapshottingProcessGeometryFragmentOutputData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFragmentOutput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_useAsFace":
            case "useAsFace":
            {
                value = instance.m_useAsFace;
                return true;
            }
            case "m_faceData":
            case "faceData":
            {
                value = instance.m_faceData;
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
