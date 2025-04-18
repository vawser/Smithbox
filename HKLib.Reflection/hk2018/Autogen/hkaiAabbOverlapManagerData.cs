// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiAabbOverlapManagerData : HavokData<hkaiAabbOverlapManager> 
{
    public hkaiAabbOverlapManagerData(HavokType type, hkaiAabbOverlapManager instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aNodes":
            case "aNodes":
            {
                if (instance.m_aNodes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bNodes":
            case "bNodes":
            {
                if (instance.m_bNodes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_aTree":
            case "aTree":
            {
                if (instance.m_aTree is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bTree":
            case "bTree":
            {
                if (instance.m_bTree is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_overlaps":
            case "overlaps":
            {
                if (instance.m_overlaps is not TGet castValue) return false;
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
            case "m_aNodes":
            case "aNodes":
            {
                if (value is not List<hkaiAabbOverlapManager.Node> castValue) return false;
                instance.m_aNodes = castValue;
                return true;
            }
            case "m_bNodes":
            case "bNodes":
            {
                if (value is not List<hkaiAabbOverlapManager.Node> castValue) return false;
                instance.m_bNodes = castValue;
                return true;
            }
            case "m_aTree":
            case "aTree":
            {
                if (value is not hkcdDynamicAabbTree castValue) return false;
                instance.m_aTree = castValue;
                return true;
            }
            case "m_bTree":
            case "bTree":
            {
                if (value is not hkcdDynamicAabbTree castValue) return false;
                instance.m_bTree = castValue;
                return true;
            }
            case "m_overlaps":
            case "overlaps":
            {
                if (value is not List<hkaiAabbOverlapManager.Overlap> castValue) return false;
                instance.m_overlaps = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
