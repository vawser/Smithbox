// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiDynamicUserEdgeSetInfoData : HavokData<HKLib.hk2019.hkaiDynamicUserEdgeSetInfo> 
{
    public hkaiDynamicUserEdgeSetInfoData(HavokType type, HKLib.hk2019.hkaiDynamicUserEdgeSetInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_setId":
            case "setId":
            {
                if (instance.m_setId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                if (instance.m_sections is not TGet castValue) return false;
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
            case "m_setId":
            case "setId":
            {
                if (value is not HKLib.hk2019.hkHandle<uint> castValue) return false;
                instance.m_setId = castValue;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                if (value is not HKLib.hk2019.hkHashMap<int, HKLib.hk2019.hkaiDynamicUserEdgeSetInfo.Section> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
