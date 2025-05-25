// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiAabbOverlapManagerOverlapData : HavokData<hkaiAabbOverlapManager.Overlap> 
{
    public hkaiAabbOverlapManagerOverlapData(HavokType type, hkaiAabbOverlapManager.Overlap instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aIndex":
            case "aIndex":
            {
                if (instance.m_aIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bIndex":
            case "bIndex":
            {
                if (instance.m_bIndex is not TGet castValue) return false;
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
            case "m_aIndex":
            case "aIndex":
            {
                if (value is not int castValue) return false;
                instance.m_aIndex = castValue;
                return true;
            }
            case "m_bIndex":
            case "bIndex":
            {
                if (value is not int castValue) return false;
                instance.m_bIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
