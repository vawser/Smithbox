// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiDynamicUserEdgeSetInfoBaseSectionIdxPairData : HavokData<hkaiDynamicUserEdgeSetInfoBase.SectionIdxPair> 
{
    public hkaiDynamicUserEdgeSetInfoBaseSectionIdxPairData(HavokType type, hkaiDynamicUserEdgeSetInfoBase.SectionIdxPair instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aSectionIdx":
            case "aSectionIdx":
            {
                if (instance.m_aSectionIdx is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bSectionIdx":
            case "bSectionIdx":
            {
                if (instance.m_bSectionIdx is not TGet castValue) return false;
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
            case "m_aSectionIdx":
            case "aSectionIdx":
            {
                if (value is not int castValue) return false;
                instance.m_aSectionIdx = castValue;
                return true;
            }
            case "m_bSectionIdx":
            case "bSectionIdx":
            {
                if (value is not int castValue) return false;
                instance.m_bSectionIdx = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
