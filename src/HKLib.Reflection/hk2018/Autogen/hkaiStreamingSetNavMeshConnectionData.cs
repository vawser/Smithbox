// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiStreamingSetNavMeshConnectionData : HavokData<hkaiStreamingSet.NavMeshConnection> 
{
    public hkaiStreamingSetNavMeshConnectionData(HavokType type, hkaiStreamingSet.NavMeshConnection instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aFaceEdgeIndex":
            case "aFaceEdgeIndex":
            {
                if (instance.m_aFaceEdgeIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bFaceEdgeIndex":
            case "bFaceEdgeIndex":
            {
                if (instance.m_bFaceEdgeIndex is not TGet castValue) return false;
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
            case "m_aFaceEdgeIndex":
            case "aFaceEdgeIndex":
            {
                if (value is not hkaiFaceEdgeIndexPair castValue) return false;
                instance.m_aFaceEdgeIndex = castValue;
                return true;
            }
            case "m_bFaceEdgeIndex":
            case "bFaceEdgeIndex":
            {
                if (value is not hkaiFaceEdgeIndexPair castValue) return false;
                instance.m_bFaceEdgeIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
