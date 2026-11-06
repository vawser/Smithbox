// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiDynamicUserEdgeSetInfoBaseSectionIdxPairData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiDynamicUserEdgeSetInfoBase.SectionIdxPair>
{
    public hkaiDynamicUserEdgeSetInfoBaseSectionIdxPairData(Havok.hk2018.hkaiDynamicUserEdgeSetInfoBase.SectionIdxPair instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aSectionIdx":
            case "aSectionIdx":
            {
                value = instance.m_aSectionIdx;
                return true;
            }
            case "m_bSectionIdx":
            case "bSectionIdx":
            {
                value = instance.m_bSectionIdx;
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
