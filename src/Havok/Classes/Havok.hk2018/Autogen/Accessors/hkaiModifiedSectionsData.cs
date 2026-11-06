// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiModifiedSectionsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiModifiedSections>
{
    public hkaiModifiedSectionsData(Havok.hk2018.hkaiModifiedSections instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                if (value is not Havok.hk2018.hkHashMap<int, Havok.hk2018.hkaiModifiedSections.Section> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
