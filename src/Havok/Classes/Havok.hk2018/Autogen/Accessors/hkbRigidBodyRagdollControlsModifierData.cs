// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbRigidBodyRagdollControlsModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbRigidBodyRagdollControlsModifier>
{
    public hkbRigidBodyRagdollControlsModifierData(Havok.hk2018.hkbRigidBodyRagdollControlsModifier instance) : base(instance) { }

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
            case "m_enable":
            case "enable":
            {
                value = instance.m_enable;
                return true;
            }
            case "m_controlData":
            case "controlData":
            {
                value = instance.m_controlData;
                return true;
            }
            case "m_bones":
            case "bones":
            {
                value = instance.m_bones;
                return true;
            }
            case "m_animationBlendFraction":
            case "animationBlendFraction":
            {
                value = instance.m_animationBlendFraction;
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
            case "m_enable":
            case "enable":
            {
                if (value is not bool castValue) return false;
                instance.m_enable = castValue;
                return true;
            }
            case "m_controlData":
            case "controlData":
            {
                if (value is not Havok.hk2018.hkbRigidBodyRagdollControlData castValue) return false;
                instance.m_controlData = castValue;
                return true;
            }
            case "m_bones":
            case "bones":
            {
                if (value is null)
                {
                    instance.m_bones = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbBoneIndexArray castValue)
                {
                    instance.m_bones = castValue;
                    return true;
                }
                return false;
            }
            case "m_animationBlendFraction":
            case "animationBlendFraction":
            {
                if (value is not float castValue) return false;
                instance.m_animationBlendFraction = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
