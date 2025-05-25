// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiEdgePathTraversalStateData : HavokData<hkaiEdgePath.TraversalState> 
{
    public hkaiEdgePathTraversalStateData(HavokType type, hkaiEdgePath.TraversalState instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceEdge":
            case "faceEdge":
            {
                if (instance.m_faceEdge is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_trailingEdge":
            case "trailingEdge":
            {
                if (instance.m_trailingEdge is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_highestUserEdgeCrossed":
            case "highestUserEdgeCrossed":
            {
                if (instance.m_highestUserEdgeCrossed is not TGet castValue) return false;
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
            case "m_faceEdge":
            case "faceEdge":
            {
                if (value is not int castValue) return false;
                instance.m_faceEdge = castValue;
                return true;
            }
            case "m_trailingEdge":
            case "trailingEdge":
            {
                if (value is not int castValue) return false;
                instance.m_trailingEdge = castValue;
                return true;
            }
            case "m_highestUserEdgeCrossed":
            case "highestUserEdgeCrossed":
            {
                if (value is not int castValue) return false;
                instance.m_highestUserEdgeCrossed = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
