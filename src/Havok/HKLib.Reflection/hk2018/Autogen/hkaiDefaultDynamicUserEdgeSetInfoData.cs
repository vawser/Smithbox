// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiDefaultDynamicUserEdgeSetInfoData : HavokData<hkaiDefaultDynamicUserEdgeSetInfo> 
{
    public hkaiDefaultDynamicUserEdgeSetInfoData(HavokType type, hkaiDefaultDynamicUserEdgeSetInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_sections":
            case "sections":
            {
                if (value is not hkHashMap<int, hkaiDefaultDynamicUserEdgeSetInfo.Section> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
