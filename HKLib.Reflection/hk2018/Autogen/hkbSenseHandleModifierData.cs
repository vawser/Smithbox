// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbSenseHandleModifierData : HavokData<hkbSenseHandleModifier> 
{
    public hkbSenseHandleModifierData(HavokType type, hkbSenseHandleModifier instance) : base(type, instance) {}

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
            case "m_sensorLocalOffset":
            case "sensorLocalOffset":
            {
                if (instance.m_sensorLocalOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ranges":
            case "ranges":
            {
                if (instance.m_ranges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_handleOut":
            case "handleOut":
            {
                if (instance.m_handleOut is null)
                {
                    return true;
                }
                if (instance.m_handleOut is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_handleIn":
            case "handleIn":
            {
                if (instance.m_handleIn is null)
                {
                    return true;
                }
                if (instance.m_handleIn is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_localFrameName":
            case "localFrameName":
            {
                if (instance.m_localFrameName is null)
                {
                    return true;
                }
                if (instance.m_localFrameName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_sensorLocalFrameName":
            case "sensorLocalFrameName":
            {
                if (instance.m_sensorLocalFrameName is null)
                {
                    return true;
                }
                if (instance.m_sensorLocalFrameName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_minDistance":
            case "minDistance":
            {
                if (instance.m_minDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxDistance":
            case "maxDistance":
            {
                if (instance.m_maxDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_distanceOut":
            case "distanceOut":
            {
                if (instance.m_distanceOut is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                if (instance.m_collisionFilterInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sensorRagdollBoneIndex":
            case "sensorRagdollBoneIndex":
            {
                if (instance.m_sensorRagdollBoneIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sensorAnimationBoneIndex":
            case "sensorAnimationBoneIndex":
            {
                if (instance.m_sensorAnimationBoneIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sensingMode":
            case "sensingMode":
            {
                if (instance.m_sensingMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_sensingMode is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_extrapolateSensorPosition":
            case "extrapolateSensorPosition":
            {
                if (instance.m_extrapolateSensorPosition is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_keepFirstSensedHandle":
            case "keepFirstSensedHandle":
            {
                if (instance.m_keepFirstSensedHandle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_foundHandleOut":
            case "foundHandleOut":
            {
                if (instance.m_foundHandleOut is not TGet castValue) return false;
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
            case "m_sensorLocalOffset":
            case "sensorLocalOffset":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_sensorLocalOffset = castValue;
                return true;
            }
            case "m_ranges":
            case "ranges":
            {
                if (value is not List<hkbSenseHandleModifier.Range> castValue) return false;
                instance.m_ranges = castValue;
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
                if (value is hkbHandle castValue)
                {
                    instance.m_handleOut = castValue;
                    return true;
                }
                return false;
            }
            case "m_handleIn":
            case "handleIn":
            {
                if (value is null)
                {
                    instance.m_handleIn = default;
                    return true;
                }
                if (value is hkbHandle castValue)
                {
                    instance.m_handleIn = castValue;
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
            case "m_sensorLocalFrameName":
            case "sensorLocalFrameName":
            {
                if (value is null)
                {
                    instance.m_sensorLocalFrameName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_sensorLocalFrameName = castValue;
                    return true;
                }
                return false;
            }
            case "m_minDistance":
            case "minDistance":
            {
                if (value is not float castValue) return false;
                instance.m_minDistance = castValue;
                return true;
            }
            case "m_maxDistance":
            case "maxDistance":
            {
                if (value is not float castValue) return false;
                instance.m_maxDistance = castValue;
                return true;
            }
            case "m_distanceOut":
            case "distanceOut":
            {
                if (value is not float castValue) return false;
                instance.m_distanceOut = castValue;
                return true;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_collisionFilterInfo = castValue;
                return true;
            }
            case "m_sensorRagdollBoneIndex":
            case "sensorRagdollBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_sensorRagdollBoneIndex = castValue;
                return true;
            }
            case "m_sensorAnimationBoneIndex":
            case "sensorAnimationBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_sensorAnimationBoneIndex = castValue;
                return true;
            }
            case "m_sensingMode":
            case "sensingMode":
            {
                if (value is hkbSenseHandleModifier.SensingMode castValue)
                {
                    instance.m_sensingMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_sensingMode = (hkbSenseHandleModifier.SensingMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_extrapolateSensorPosition":
            case "extrapolateSensorPosition":
            {
                if (value is not bool castValue) return false;
                instance.m_extrapolateSensorPosition = castValue;
                return true;
            }
            case "m_keepFirstSensedHandle":
            case "keepFirstSensedHandle":
            {
                if (value is not bool castValue) return false;
                instance.m_keepFirstSensedHandle = castValue;
                return true;
            }
            case "m_foundHandleOut":
            case "foundHandleOut":
            {
                if (value is not bool castValue) return false;
                instance.m_foundHandleOut = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
