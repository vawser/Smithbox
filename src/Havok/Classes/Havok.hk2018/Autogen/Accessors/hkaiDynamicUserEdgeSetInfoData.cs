// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiDynamicUserEdgeSetInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiDynamicUserEdgeSetInfo>
{
    public hkaiDynamicUserEdgeSetInfoData(Havok.hk2018.hkaiDynamicUserEdgeSetInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_setId":
            case "setId":
            {
                value = instance.m_setId;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                value = instance.m_sections;
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
            case "m_setId":
            case "setId":
            {
                if (value is not Havok.hk2018.hkHandle<uint> castValue) return false;
                instance.m_setId = castValue;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                if (value is not Havok.hk2018.hkHashMap<int, Havok.hk2018.hkaiDynamicUserEdgeSetInfo.Section> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
