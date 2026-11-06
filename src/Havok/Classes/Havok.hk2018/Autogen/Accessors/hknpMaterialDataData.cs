// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpMaterialDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpMaterialData>
{
    public hknpMaterialDataData(Havok.hk2018.hknpMaterialData instance) : base(instance) { }

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
            case "m_density":
            case "density":
            {
                value = instance.m_density;
                return true;
            }
            case "m_inertiaFactor":
            case "inertiaFactor":
            {
                value = instance.m_inertiaFactor;
                return true;
            }
            case "m_dynamicFriction":
            case "dynamicFriction":
            {
                value = instance.m_dynamicFriction;
                return true;
            }
            case "m_staticFriction":
            case "staticFriction":
            {
                value = instance.m_staticFriction;
                return true;
            }
            case "m_restitution":
            case "restitution":
            {
                value = instance.m_restitution;
                return true;
            }
            case "m_frictionCombinePolicy":
            case "frictionCombinePolicy":
            {
                value = instance.m_frictionCombinePolicy;
                return true;
            }
            case "m_restitutionCombinePolicy":
            case "restitutionCombinePolicy":
            {
                value = instance.m_restitutionCombinePolicy;
                return true;
            }
            case "m_weldingTolerance":
            case "weldingTolerance":
            {
                value = instance.m_weldingTolerance;
                return true;
            }
            case "m_triggerVolumeType":
            case "triggerVolumeType":
            {
                value = instance.m_triggerVolumeType;
                return true;
            }
            case "m_triggerVolumeTolerance":
            case "triggerVolumeTolerance":
            {
                value = instance.m_triggerVolumeTolerance;
                return true;
            }
            case "m_maxContactImpulse":
            case "maxContactImpulse":
            {
                value = instance.m_maxContactImpulse;
                return true;
            }
            case "m_fractionOfClippedImpulseToApply":
            case "fractionOfClippedImpulseToApply":
            {
                value = instance.m_fractionOfClippedImpulseToApply;
                return true;
            }
            case "m_massChangerCategory":
            case "massChangerCategory":
            {
                value = instance.m_massChangerCategory;
                return true;
            }
            case "m_massChangerHeavyObjectFactor":
            case "massChangerHeavyObjectFactor":
            {
                value = instance.m_massChangerHeavyObjectFactor;
                return true;
            }
            case "m_softContactForceFactor":
            case "softContactForceFactor":
            {
                value = instance.m_softContactForceFactor;
                return true;
            }
            case "m_softContactDampFactor":
            case "softContactDampFactor":
            {
                value = instance.m_softContactDampFactor;
                return true;
            }
            case "m_softContactSeparationVelocity":
            case "softContactSeparationVelocity":
            {
                value = instance.m_softContactSeparationVelocity;
                return true;
            }
            case "m_surfaceVelocity":
            case "surfaceVelocity":
            {
                value = instance.m_surfaceVelocity;
                return true;
            }
            case "m_disablingCollisionsBetweenCvxCvxDynamicObjectsDistance":
            case "disablingCollisionsBetweenCvxCvxDynamicObjectsDistance":
            {
                value = instance.m_disablingCollisionsBetweenCvxCvxDynamicObjectsDistance;
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
            case "m_density":
            case "density":
            {
                if (value is not float castValue) return false;
                instance.m_density = castValue;
                return true;
            }
            case "m_inertiaFactor":
            case "inertiaFactor":
            {
                if (value is not float castValue) return false;
                instance.m_inertiaFactor = castValue;
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
                if (value is Havok.hk2018.hknpMaterial.CombinePolicy castValue)
                {
                    instance.m_frictionCombinePolicy = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_frictionCombinePolicy = (Havok.hk2018.hknpMaterial.CombinePolicy)byteValue;
                    return true;
                }
                return false;
            }
            case "m_restitutionCombinePolicy":
            case "restitutionCombinePolicy":
            {
                if (value is Havok.hk2018.hknpMaterial.CombinePolicy castValue)
                {
                    instance.m_restitutionCombinePolicy = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_restitutionCombinePolicy = (Havok.hk2018.hknpMaterial.CombinePolicy)byteValue;
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
            case "m_triggerVolumeType":
            case "triggerVolumeType":
            {
                if (value is Havok.hk2018.hknpMaterial.TriggerType castValue)
                {
                    instance.m_triggerVolumeType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_triggerVolumeType = (Havok.hk2018.hknpMaterial.TriggerType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_triggerVolumeTolerance":
            case "triggerVolumeTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_triggerVolumeTolerance = castValue;
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
                if (value is Havok.hk2018.hknpMaterial.MassChangerCategory castValue)
                {
                    instance.m_massChangerCategory = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_massChangerCategory = (Havok.hk2018.hknpMaterial.MassChangerCategory)byteValue;
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
                if (value is not float castValue) return false;
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
                if (value is Havok.hk2018.hknpSurfaceVelocity castValue)
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
            default:
            return false;
        }
    }

}
