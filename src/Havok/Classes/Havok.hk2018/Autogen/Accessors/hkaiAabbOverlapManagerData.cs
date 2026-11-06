// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiAabbOverlapManagerData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiAabbOverlapManager>
{
    public hkaiAabbOverlapManagerData(Havok.hk2018.hkaiAabbOverlapManager instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aNodes":
            case "aNodes":
            {
                value = instance.m_aNodes;
                return true;
            }
            case "m_bNodes":
            case "bNodes":
            {
                value = instance.m_bNodes;
                return true;
            }
            case "m_aTree":
            case "aTree":
            {
                value = instance.m_aTree;
                return true;
            }
            case "m_bTree":
            case "bTree":
            {
                value = instance.m_bTree;
                return true;
            }
            case "m_overlaps":
            case "overlaps":
            {
                value = instance.m_overlaps;
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
            case "m_aNodes":
            case "aNodes":
            {
                if (value is not List<Havok.hk2018.hkaiAabbOverlapManager.Node> castValue) return false;
                instance.m_aNodes = castValue;
                return true;
            }
            case "m_bNodes":
            case "bNodes":
            {
                if (value is not List<Havok.hk2018.hkaiAabbOverlapManager.Node> castValue) return false;
                instance.m_bNodes = castValue;
                return true;
            }
            case "m_aTree":
            case "aTree":
            {
                if (value is not Havok.hk2018.hkcdDynamicAabbTree castValue) return false;
                instance.m_aTree = castValue;
                return true;
            }
            case "m_bTree":
            case "bTree":
            {
                if (value is not Havok.hk2018.hkcdDynamicAabbTree castValue) return false;
                instance.m_bTree = castValue;
                return true;
            }
            case "m_overlaps":
            case "overlaps":
            {
                if (value is not List<Havok.hk2018.hkaiAabbOverlapManager.Overlap> castValue) return false;
                instance.m_overlaps = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
