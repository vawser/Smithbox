// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclStateTransitionSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclStateTransitionSetupObject>
{
    public hclStateTransitionSetupObjectData(Havok.hk2016.hclStateTransitionSetupObject instance) : base(instance) { }

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
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_useTransitionConstraints":
            case "useTransitionConstraints":
            {
                value = instance.m_useTransitionConstraints;
                return true;
            }
            case "m_useDynamicBlendTransitions":
            case "useDynamicBlendTransitions":
            {
                value = instance.m_useDynamicBlendTransitions;
                return true;
            }
            case "m_stateSetupObjects":
            case "stateSetupObjects":
            {
                value = instance.m_stateSetupObjects;
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
            case "m_useTransitionConstraints":
            case "useTransitionConstraints":
            {
                if (value is not bool castValue) return false;
                instance.m_useTransitionConstraints = castValue;
                return true;
            }
            case "m_useDynamicBlendTransitions":
            case "useDynamicBlendTransitions":
            {
                if (value is not bool castValue) return false;
                instance.m_useDynamicBlendTransitions = castValue;
                return true;
            }
            case "m_stateSetupObjects":
            case "stateSetupObjects":
            {
                if (value is not List<Havok.hk2016.hclClothStateSetupObject?> castValue) return false;
                instance.m_stateSetupObjects = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
