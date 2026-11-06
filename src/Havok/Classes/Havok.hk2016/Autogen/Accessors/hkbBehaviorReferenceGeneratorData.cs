// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbBehaviorReferenceGeneratorData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbBehaviorReferenceGenerator>
{
    public hkbBehaviorReferenceGeneratorData(Havok.hk2016.hkbBehaviorReferenceGenerator instance) : base(instance) { }

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
            case "m_behaviorName":
            case "behaviorName":
            {
                value = instance.m_behaviorName;
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
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
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
                if (value is Havok.hk2016.hkbVariableBindingSet castValue)
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
            case "m_behaviorName":
            case "behaviorName":
            {
                if (value is null)
                {
                    instance.m_behaviorName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_behaviorName = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
