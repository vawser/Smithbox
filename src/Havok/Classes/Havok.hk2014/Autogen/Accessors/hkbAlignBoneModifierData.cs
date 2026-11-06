// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbAlignBoneModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbAlignBoneModifier>
{
    public hkbAlignBoneModifierData(Havok.hk2014.hkbAlignBoneModifier instance) : base(instance) { }

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
            case "m_alignMode":
            case "alignMode":
            {
                value = instance.m_alignMode;
                return true;
            }
            case "m_alignTargetMode":
            case "alignTargetMode":
            {
                value = instance.m_alignTargetMode;
                return true;
            }
            case "m_alignSingleAxis":
            case "alignSingleAxis":
            {
                value = instance.m_alignSingleAxis;
                return true;
            }
            case "m_alignAxis":
            case "alignAxis":
            {
                value = instance.m_alignAxis;
                return true;
            }
            case "m_alignTargetAxis":
            case "alignTargetAxis":
            {
                value = instance.m_alignTargetAxis;
                return true;
            }
            case "m_frameOfReference":
            case "frameOfReference":
            {
                value = instance.m_frameOfReference;
                return true;
            }
            case "m_duration":
            case "duration":
            {
                value = instance.m_duration;
                return true;
            }
            case "m_alignModeIndex":
            case "alignModeIndex":
            {
                value = instance.m_alignModeIndex;
                return true;
            }
            case "m_alignTargetModeIndex":
            case "alignTargetModeIndex":
            {
                value = instance.m_alignTargetModeIndex;
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
            case "m_alignMode":
            case "alignMode":
            {
                if (value is Havok.hk2014.hkbAlignBoneModifier.AlignModeABAM castValue)
                {
                    instance.m_alignMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_alignMode = (Havok.hk2014.hkbAlignBoneModifier.AlignModeABAM)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_alignTargetMode":
            case "alignTargetMode":
            {
                if (value is Havok.hk2014.hkbAlignBoneModifier.AlignTargetMode castValue)
                {
                    instance.m_alignTargetMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_alignTargetMode = (Havok.hk2014.hkbAlignBoneModifier.AlignTargetMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_alignSingleAxis":
            case "alignSingleAxis":
            {
                if (value is not bool castValue) return false;
                instance.m_alignSingleAxis = castValue;
                return true;
            }
            case "m_alignAxis":
            case "alignAxis":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_alignAxis = castValue;
                return true;
            }
            case "m_alignTargetAxis":
            case "alignTargetAxis":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_alignTargetAxis = castValue;
                return true;
            }
            case "m_frameOfReference":
            case "frameOfReference":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_frameOfReference = castValue;
                return true;
            }
            case "m_duration":
            case "duration":
            {
                if (value is not float castValue) return false;
                instance.m_duration = castValue;
                return true;
            }
            case "m_alignModeIndex":
            case "alignModeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_alignModeIndex = castValue;
                return true;
            }
            case "m_alignTargetModeIndex":
            case "alignTargetModeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_alignTargetModeIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
