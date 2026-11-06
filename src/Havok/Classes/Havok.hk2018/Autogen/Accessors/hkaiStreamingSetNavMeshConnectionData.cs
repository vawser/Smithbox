// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiStreamingSetNavMeshConnectionData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiStreamingSet.NavMeshConnection>
{
    public hkaiStreamingSetNavMeshConnectionData(Havok.hk2018.hkaiStreamingSet.NavMeshConnection instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aFaceEdgeIndex":
            case "aFaceEdgeIndex":
            {
                value = instance.m_aFaceEdgeIndex;
                return true;
            }
            case "m_bFaceEdgeIndex":
            case "bFaceEdgeIndex":
            {
                value = instance.m_bFaceEdgeIndex;
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
            case "m_aFaceEdgeIndex":
            case "aFaceEdgeIndex":
            {
                if (value is not Havok.hk2018.hkaiFaceEdgeIndexPair castValue) return false;
                instance.m_aFaceEdgeIndex = castValue;
                return true;
            }
            case "m_bFaceEdgeIndex":
            case "bFaceEdgeIndex":
            {
                if (value is not Havok.hk2018.hkaiFaceEdgeIndexPair castValue) return false;
                instance.m_bFaceEdgeIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
