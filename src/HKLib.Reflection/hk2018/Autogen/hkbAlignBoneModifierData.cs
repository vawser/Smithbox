// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbAlignBoneModifierData : HavokData<hkbAlignBoneModifier> 
{
    public hkbAlignBoneModifierData(HavokType type, hkbAlignBoneModifier instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (instance.m_variableBindingSet is null)
                {
                    return true;
                }
                if (instance.m_variableBindingSet is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_enable":
            case "enable":
            {
                if (instance.m_enable is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_alignMode":
            case "alignMode":
            {
                if (instance.m_alignMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_alignMode is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_alignTargetMode":
            case "alignTargetMode":
            {
                if (instance.m_alignTargetMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_alignTargetMode is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_alignSingleAxis":
            case "alignSingleAxis":
            {
                if (instance.m_alignSingleAxis is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_alignAxis":
            case "alignAxis":
            {
                if (instance.m_alignAxis is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_alignTargetAxis":
            case "alignTargetAxis":
            {
                if (instance.m_alignTargetAxis is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_frameOfReference":
            case "frameOfReference":
            {
                if (instance.m_frameOfReference is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_duration":
            case "duration":
            {
                if (instance.m_duration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_alignModeIndex":
            case "alignModeIndex":
            {
                if (instance.m_alignModeIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_alignTargetModeIndex":
            case "alignTargetModeIndex":
            {
                if (instance.m_alignTargetModeIndex is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
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
                if (value is hkbVariableBindingSet castValue)
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
                if (value is hkbAlignBoneModifier.AlignModeABAM castValue)
                {
                    instance.m_alignMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_alignMode = (hkbAlignBoneModifier.AlignModeABAM)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_alignTargetMode":
            case "alignTargetMode":
            {
                if (value is hkbAlignBoneModifier.AlignTargetMode castValue)
                {
                    instance.m_alignTargetMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_alignTargetMode = (hkbAlignBoneModifier.AlignTargetMode)sbyteValue;
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
