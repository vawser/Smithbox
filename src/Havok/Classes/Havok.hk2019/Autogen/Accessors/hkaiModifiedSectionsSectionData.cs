// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiModifiedSectionsSectionData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiModifiedSections.Section>
{
    public hkaiModifiedSectionsSectionData(Havok.hk2019.hkaiModifiedSections.Section instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sectionFlags":
            case "sectionFlags":
            {
                value = instance.m_sectionFlags;
                return true;
            }
            case "m_fireEvents":
            case "fireEvents":
            {
                value = instance.m_fireEvents;
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
            case "m_sectionFlags":
            case "sectionFlags":
            {
                if (value is Havok.hk2019.hkaiModifiedSections.SectionBits castValue)
                {
                    instance.m_sectionFlags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_sectionFlags = (Havok.hk2019.hkaiModifiedSections.SectionBits)byteValue;
                    return true;
                }
                return false;
            }
            case "m_fireEvents":
            case "fireEvents":
            {
                if (value is not bool castValue) return false;
                instance.m_fireEvents = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
