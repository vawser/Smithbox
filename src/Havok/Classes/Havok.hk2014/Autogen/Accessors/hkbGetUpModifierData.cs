// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbGetUpModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbGetUpModifier>
{
    public hkbGetUpModifierData(Havok.hk2014.hkbGetUpModifier instance) : base(instance) { }

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
            case "m_groundNormal":
            case "groundNormal":
            {
                value = instance.m_groundNormal;
                return true;
            }
            case "m_duration":
            case "duration":
            {
                value = instance.m_duration;
                return true;
            }
            case "m_alignWithGroundDuration":
            case "alignWithGroundDuration":
            {
                value = instance.m_alignWithGroundDuration;
                return true;
            }
            case "m_rootBoneIndex":
            case "rootBoneIndex":
            {
                value = instance.m_rootBoneIndex;
                return true;
            }
            case "m_otherBoneIndex":
            case "otherBoneIndex":
            {
                value = instance.m_otherBoneIndex;
                return true;
            }
            case "m_anotherBoneIndex":
            case "anotherBoneIndex":
            {
                value = instance.m_anotherBoneIndex;
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
            case "m_groundNormal":
            case "groundNormal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_groundNormal = castValue;
                return true;
            }
            case "m_duration":
            case "duration":
            {
                if (value is not float castValue) return false;
                instance.m_duration = castValue;
                return true;
            }
            case "m_alignWithGroundDuration":
            case "alignWithGroundDuration":
            {
                if (value is not float castValue) return false;
                instance.m_alignWithGroundDuration = castValue;
                return true;
            }
            case "m_rootBoneIndex":
            case "rootBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_rootBoneIndex = castValue;
                return true;
            }
            case "m_otherBoneIndex":
            case "otherBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_otherBoneIndex = castValue;
                return true;
            }
            case "m_anotherBoneIndex":
            case "anotherBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_anotherBoneIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
