// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbCustomTestGeneratorHiddenTypesData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbCustomTestGeneratorHiddenTypes>
{
    public hkbCustomTestGeneratorHiddenTypesData(Havok.hk2018.hkbCustomTestGeneratorHiddenTypes instance) : base(instance) { }

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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                value = instance.m_variableBindingSet;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_inheritedHiddenMember":
            case "inheritedHiddenMember":
            {
                value = instance.m_inheritedHiddenMember;
                return true;
            }
            case "m_protectedInheritedHiddenMember":
            case "protectedInheritedHiddenMember":
            {
                value = instance.m_protectedInheritedHiddenMember;
                return true;
            }
            case "m_privateInheritedHiddenMember":
            case "privateInheritedHiddenMember":
            {
                value = instance.m_privateInheritedHiddenMember;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbVariableBindingSet castValue)
                {
                    instance.m_variableBindingSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_inheritedHiddenMember":
            case "inheritedHiddenMember":
            {
                if (value is not bool castValue) return false;
                instance.m_inheritedHiddenMember = castValue;
                return true;
            }
            case "m_protectedInheritedHiddenMember":
            case "protectedInheritedHiddenMember":
            {
                if (value is not bool castValue) return false;
                instance.m_protectedInheritedHiddenMember = castValue;
                return true;
            }
            case "m_privateInheritedHiddenMember":
            case "privateInheritedHiddenMember":
            {
                if (value is not bool castValue) return false;
                instance.m_privateInheritedHiddenMember = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
