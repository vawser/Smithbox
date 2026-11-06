// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiAabbOverlapManagerOverlapData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiAabbOverlapManager.Overlap>
{
    public hkaiAabbOverlapManagerOverlapData(Havok.hk2018.hkaiAabbOverlapManager.Overlap instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aIndex":
            case "aIndex":
            {
                value = instance.m_aIndex;
                return true;
            }
            case "m_bIndex":
            case "bIndex":
            {
                value = instance.m_bIndex;
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
