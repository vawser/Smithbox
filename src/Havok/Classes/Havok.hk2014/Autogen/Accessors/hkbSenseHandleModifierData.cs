// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbSenseHandleModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbSenseHandleModifier>
{
    public hkbSenseHandleModifierData(Havok.hk2014.hkbSenseHandleModifier instance) : base(instance) { }

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
            case "m_sensorLocalOffset":
            case "sensorLocalOffset":
            {
                value = instance.m_sensorLocalOffset;
                return true;
            }
            case "m_ranges":
            case "ranges":
            {
                value = instance.m_ranges;
                return true;
            }
            case "m_handleOut":
            case "handleOut":
            {
                value = instance.m_handleOut;
                return true;
            }
            case "m_handleIn":
            case "handleIn":
            {
                value = instance.m_handleIn;
                return true;
            }
            case "m_localFrameName":
            case "localFrameName":
            {
                value = instance.m_localFrameName;
                return true;
            }
            case "m_sensorLocalFrameName":
            case "sensorLocalFrameName":
            {
                value = instance.m_sensorLocalFrameName;
                return true;
            }
            case "m_minDistance":
            case "minDistance":
            {
                value = instance.m_minDistance;
                return true;
            }
            case "m_maxDistance":
            case "maxDistance":
            {
                value = instance.m_maxDistance;
                return true;
            }
            case "m_distanceOut":
            case "distanceOut":
            {
                value = instance.m_distanceOut;
                return true;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                value = instance.m_collisionFilterInfo;
                return true;
            }
            case "m_sensorRagdollBoneIndex":
            case "sensorRagdollBoneIndex":
            {
                value = instance.m_sensorRagdollBoneIndex;
                return true;
            }
            case "m_sensorAnimationBoneIndex":
            case "sensorAnimationBoneIndex":
            {
                value = instance.m_sensorAnimationBoneIndex;
                return true;
            }
            case "m_sensingMode":
            case "sensingMode":
            {
                value = instance.m_sensingMode;
                return true;
            }
            case "m_extrapolateSensorPosition":
            case "extrapolateSensorPosition":
            {
                value = instance.m_extrapolateSensorPosition;
                return true;
            }
            case "m_keepFirstSensedHandle":
            case "keepFirstSensedHandle":
            {
                value = instance.m_keepFirstSensedHandle;
                return true;
            }
            case "m_foundHandleOut":
            case "foundHandleOut":
            {
                value = instance.m_foundHandleOut;
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
                if (value is not List<Havok.hk2014.hkbSenseHandleModifier.Range> castValue) return false;
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
                if (value is Havok.hk2014.hkbHandle castValue)
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
                if (value is Havok.hk2014.hkbHandle castValue)
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
                if (value is Havok.hk2014.hkbSenseHandleModifier.SensingMode castValue)
                {
                    instance.m_sensingMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_sensingMode = (Havok.hk2014.hkbSenseHandleModifier.SensingMode)sbyteValue;
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
