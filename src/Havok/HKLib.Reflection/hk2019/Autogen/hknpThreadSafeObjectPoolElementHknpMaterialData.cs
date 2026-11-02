// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpThreadSafeObjectPoolElementHknpMaterialData : HavokData<HKLib.hk2019.hknpThreadSafeObjectPoolElementHknpMaterial> 
{
    public hknpThreadSafeObjectPoolElementHknpMaterialData(HavokType type, HKLib.hk2019.hknpThreadSafeObjectPoolElementHknpMaterial instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_isExclusive":
            case "isExclusive":
            {
                if (instance.m_isExclusive is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_flags is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_triggerType":
            case "triggerType":
            {
                if (instance.m_triggerType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_triggerType is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_triggerManifoldTolerance":
            case "triggerManifoldTolerance":
            {
                if (instance.m_triggerManifoldTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dynamicFriction":
            case "dynamicFriction":
            {
                if (instance.m_dynamicFriction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_staticFriction":
            case "staticFriction":
            {
                if (instance.m_staticFriction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_restitution":
            case "restitution":
            {
                if (instance.m_restitution is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_frictionCombinePolicy":
            case "frictionCombinePolicy":
            {
                if (instance.m_frictionCombinePolicy is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_frictionCombinePolicy is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_restitutionCombinePolicy":
            case "restitutionCombinePolicy":
            {
                if (instance.m_restitutionCombinePolicy is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_restitutionCombinePolicy is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_weldingTolerance":
            case "weldingTolerance":
            {
                if (instance.m_weldingTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxContactImpulse":
            case "maxContactImpulse":
            {
                if (instance.m_maxContactImpulse is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fractionOfClippedImpulseToApply":
            case "fractionOfClippedImpulseToApply":
            {
                if (instance.m_fractionOfClippedImpulseToApply is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_massChangerCategory":
            case "massChangerCategory":
            {
                if (instance.m_massChangerCategory is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_massChangerCategory is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_massChangerHeavyObjectFactor":
            case "massChangerHeavyObjectFactor":
            {
                if (instance.m_massChangerHeavyObjectFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_softContactForceFactor":
            case "softContactForceFactor":
            {
                if (instance.m_softContactForceFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_softContactDampFactor":
            case "softContactDampFactor":
            {
                if (instance.m_softContactDampFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_softContactSeparationVelocity":
            case "softContactSeparationVelocity":
            {
                if (instance.m_softContactSeparationVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_surfaceVelocity":
            case "surfaceVelocity":
            {
                if (instance.m_surfaceVelocity is null)
                {
                    return true;
                }
                if (instance.m_surfaceVelocity is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_disablingCollisionsBetweenCvxCvxDynamicObjectsDistance":
            case "disablingCollisionsBetweenCvxCvxDynamicObjectsDistance":
            {
                if (instance.m_disablingCollisionsBetweenCvxCvxDynamicObjectsDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
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
            case "m_isExclusive":
            case "isExclusive":
            {
                if (value is not uint castValue) return false;
                instance.m_isExclusive = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is HKLib.hk2019.hknpCollisionFlags.Enum castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_flags = (HKLib.hk2019.hknpCollisionFlags.Enum)intValue;
                    return true;
                }
                return false;
            }
            case "m_triggerType":
            case "triggerType":
            {
                if (value is HKLib.hk2019.hknpMaterial.TriggerType castValue)
                {
                    instance.m_triggerType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_triggerType = (HKLib.hk2019.hknpMaterial.TriggerType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_triggerManifoldTolerance":
            case "triggerManifoldTolerance":
            {
                if (value is not HKLib.hk2019.hkUFloat8 castValue) return false;
                instance.m_triggerManifoldTolerance = castValue;
                return true;
            }
            case "m_dynamicFriction":
            case "dynamicFriction":
            {
                if (value is not float castValue) return false;
                instance.m_dynamicFriction = castValue;
                return true;
            }
            case "m_staticFriction":
            case "staticFriction":
            {
                if (value is not float castValue) return false;
                instance.m_staticFriction = castValue;
                return true;
            }
            case "m_restitution":
            case "restitution":
            {
                if (value is not float castValue) return false;
                instance.m_restitution = castValue;
                return true;
            }
            case "m_frictionCombinePolicy":
            case "frictionCombinePolicy":
            {
                if (value is HKLib.hk2019.hknpMaterial.CombinePolicy castValue)
                {
                    instance.m_frictionCombinePolicy = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_frictionCombinePolicy = (HKLib.hk2019.hknpMaterial.CombinePolicy)byteValue;
                    return true;
                }
                return false;
            }
            case "m_restitutionCombinePolicy":
            case "restitutionCombinePolicy":
            {
                if (value is HKLib.hk2019.hknpMaterial.CombinePolicy castValue)
                {
                    instance.m_restitutionCombinePolicy = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_restitutionCombinePolicy = (HKLib.hk2019.hknpMaterial.CombinePolicy)byteValue;
                    return true;
                }
                return false;
            }
            case "m_weldingTolerance":
            case "weldingTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_weldingTolerance = castValue;
                return true;
            }
            case "m_maxContactImpulse":
            case "maxContactImpulse":
            {
                if (value is not float castValue) return false;
                instance.m_maxContactImpulse = castValue;
                return true;
            }
            case "m_fractionOfClippedImpulseToApply":
            case "fractionOfClippedImpulseToApply":
            {
                if (value is not float castValue) return false;
                instance.m_fractionOfClippedImpulseToApply = castValue;
                return true;
            }
            case "m_massChangerCategory":
            case "massChangerCategory":
            {
                if (value is HKLib.hk2019.hknpMaterial.MassChangerCategory castValue)
                {
                    instance.m_massChangerCategory = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_massChangerCategory = (HKLib.hk2019.hknpMaterial.MassChangerCategory)byteValue;
                    return true;
                }
                return false;
            }
            case "m_massChangerHeavyObjectFactor":
            case "massChangerHeavyObjectFactor":
            {
                if (value is not float castValue) return false;
                instance.m_massChangerHeavyObjectFactor = castValue;
                return true;
            }
            case "m_softContactForceFactor":
            case "softContactForceFactor":
            {
                if (value is not float castValue) return false;
                instance.m_softContactForceFactor = castValue;
                return true;
            }
            case "m_softContactDampFactor":
            case "softContactDampFactor":
            {
                if (value is not float castValue) return false;
                instance.m_softContactDampFactor = castValue;
                return true;
            }
            case "m_softContactSeparationVelocity":
            case "softContactSeparationVelocity":
            {
                if (value is not HKLib.hk2019.hkUFloat8 castValue) return false;
                instance.m_softContactSeparationVelocity = castValue;
                return true;
            }
            case "m_surfaceVelocity":
            case "surfaceVelocity":
            {
                if (value is null)
                {
                    instance.m_surfaceVelocity = default;
                    return true;
                }
                if (value is HKLib.hk2019.hknpSurfaceVelocity castValue)
                {
                    instance.m_surfaceVelocity = castValue;
                    return true;
                }
                return false;
            }
            case "m_disablingCollisionsBetweenCvxCvxDynamicObjectsDistance":
            case "disablingCollisionsBetweenCvxCvxDynamicObjectsDistance":
            {
                if (value is not float castValue) return false;
                instance.m_disablingCollisionsBetweenCvxCvxDynamicObjectsDistance = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
