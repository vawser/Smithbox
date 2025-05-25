// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiDynamicUserEdgeSetInfoUserEdgePairData : HavokData<hkaiDynamicUserEdgeSetInfo.UserEdgePair> 
{
    public hkaiDynamicUserEdgeSetInfoUserEdgePairData(HavokType type, hkaiDynamicUserEdgeSetInfo.UserEdgePair instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aFaceKey":
            case "aFaceKey":
            {
                if (instance.m_aFaceKey is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bFaceKey":
            case "bFaceKey":
            {
                if (instance.m_bFaceKey is not TGet castValue) return false;
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
            case "m_aFaceKey":
            case "aFaceKey":
            {
                if (value is not uint castValue) return false;
                instance.m_aFaceKey = castValue;
                return true;
            }
            case "m_bFaceKey":
            case "bFaceKey":
            {
                if (value is not uint castValue) return false;
                instance.m_bFaceKey = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
