// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbGetHandleOnBoneModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbGetHandleOnBoneModifier>
{
    public hkbGetHandleOnBoneModifierData(Havok.hk2014.hkbGetHandleOnBoneModifier instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_handleOut":
            case "handleOut":
            {
                value = instance.m_handleOut;
                return true;
            }
            case "m_localFrameName":
            case "localFrameName":
            {
                value = instance.m_localFrameName;
                return true;
            }
            case "m_ragdollBoneIndex":
            case "ragdollBoneIndex":
            {
                value = instance.m_ragdollBoneIndex;
                return true;
            }
            case "m_animationBoneIndex":
            case "animationBoneIndex":
            {
                value = instance.m_animationBoneIndex;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbVariableBindingSet castValue)
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
            case "m_handleOut":
            case "handleOut":
            {
                if (value is null)
                {
                    instance.m_handleOut = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbHandle castValue)
                {
                    instance.m_handleOut = castValue;
                    return true;
                }
                return false;
            }
            case "m_localFrameName":
            case "localFrameName":
            {
                if (value is null)
                {
                    instance.m_localFrameName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_localFrameName = castValue;
                    return true;
                }
                return false;
            }
            case "m_ragdollBoneIndex":
            case "ragdollBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_ragdollBoneIndex = castValue;
                return true;
            }
            case "m_animationBoneIndex":
            case "animationBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_animationBoneIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
