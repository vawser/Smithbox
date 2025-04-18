// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiModifiedSectionsSectionData : HavokData<hkaiModifiedSections.Section> 
{
    public hkaiModifiedSectionsSectionData(HavokType type, hkaiModifiedSections.Section instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sectionFlags":
            case "sectionFlags":
            {
                if (instance.m_sectionFlags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_sectionFlags is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_fireEvents":
            case "fireEvents":
            {
                if (instance.m_fireEvents is not TGet castValue) return false;
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
            case "m_sectionFlags":
            case "sectionFlags":
            {
                if (value is hkaiModifiedSections.SectionBits castValue)
                {
                    instance.m_sectionFlags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_sectionFlags = (hkaiModifiedSections.SectionBits)byteValue;
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
