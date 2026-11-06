// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclSimClothInstanceData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclSimClothInstance>
{
    public hclSimClothInstanceData(Havok.hk2018.hclSimClothInstance instance) : base(instance) { }

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
            case "m_lastTimeStepUsed":
            case "lastTimeStepUsed":
            {
                value = instance.m_lastTimeStepUsed;
                return true;
            }
            case "m_effectiveGlobalDampingInv":
            case "effectiveGlobalDampingInv":
            {
                value = instance.m_effectiveGlobalDampingInv;
                return true;
            }
            case "m_particlesAabb":
            case "particlesAabb":
            {
                value = instance.m_particlesAabb;
                return true;
            }
            case "m_collisionParticlesAabb":
            case "collisionParticlesAabb":
            {
                value = instance.m_collisionParticlesAabb;
                return true;
            }
            case "m_updateCollisionAabbs":
            case "updateCollisionAabbs":
            {
                value = instance.m_updateCollisionAabbs;
                return true;
            }
            case "m_landscapeCollisionParticlesAabb":
            case "landscapeCollisionParticlesAabb":
            {
                value = instance.m_landscapeCollisionParticlesAabb;
                return true;
            }
            case "m_previousTransferWorldFromModel":
            case "previousTransferWorldFromModel":
            {
                value = instance.m_previousTransferWorldFromModel;
                return true;
            }
            case "m_constraintStiffnessModelType":
            case "constraintStiffnessModelType":
            {
                value = instance.m_constraintStiffnessModelType;
                return true;
            }
            case "m_slowMotionFactor":
            case "slowMotionFactor":
            {
                value = instance.m_slowMotionFactor;
                return true;
            }
            case "m_worldSteppingFactor":
            case "worldSteppingFactor":
            {
                value = instance.m_worldSteppingFactor;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_transferMotionEnabled":
            case "transferMotionEnabled":
            {
                value = instance.m_transferMotionEnabled;
                return true;
            }
            case "m_landscapeCollisionEnabled":
            case "landscapeCollisionEnabled":
            {
                value = instance.m_landscapeCollisionEnabled;
                return true;
            }
            case "m_pinchDetectionEnabled":
            case "pinchDetectionEnabled":
            {
                value = instance.m_pinchDetectionEnabled;
                return true;
            }
            case "m_virtualCollisionPointsFlags":
            case "virtualCollisionPointsFlags":
            {
                value = instance.m_virtualCollisionPointsFlags;
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
            case "m_lastTimeStepUsed":
            case "lastTimeStepUsed":
            {
                if (value is not float castValue) return false;
                instance.m_lastTimeStepUsed = castValue;
                return true;
            }
            case "m_effectiveGlobalDampingInv":
            case "effectiveGlobalDampingInv":
            {
                if (value is not float castValue) return false;
                instance.m_effectiveGlobalDampingInv = castValue;
                return true;
            }
            case "m_particlesAabb":
            case "particlesAabb":
            {
                if (value is not Havok.hk2018.hkAabb castValue) return false;
                instance.m_particlesAabb = castValue;
                return true;
            }
            case "m_collisionParticlesAabb":
            case "collisionParticlesAabb":
            {
                if (value is not Havok.hk2018.hkAabb castValue) return false;
                instance.m_collisionParticlesAabb = castValue;
                return true;
            }
            case "m_updateCollisionAabbs":
            case "updateCollisionAabbs":
            {
                if (value is not bool castValue) return false;
                instance.m_updateCollisionAabbs = castValue;
                return true;
            }
            case "m_landscapeCollisionParticlesAabb":
            case "landscapeCollisionParticlesAabb":
            {
                if (value is not Havok.hk2018.hkAabb castValue) return false;
                instance.m_landscapeCollisionParticlesAabb = castValue;
                return true;
            }
            case "m_previousTransferWorldFromModel":
            case "previousTransferWorldFromModel":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_previousTransferWorldFromModel = castValue;
                return true;
            }
            case "m_constraintStiffnessModelType":
            case "constraintStiffnessModelType":
            {
                if (value is Havok.hk2018.hclConstraintStiffnessDispatcher.Type castValue)
                {
                    instance.m_constraintStiffnessModelType = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_constraintStiffnessModelType = (Havok.hk2018.hclConstraintStiffnessDispatcher.Type)intValue;
                    return true;
                }
                return false;
            }
            case "m_slowMotionFactor":
            case "slowMotionFactor":
            {
                if (value is not float castValue) return false;
                instance.m_slowMotionFactor = castValue;
                return true;
            }
            case "m_worldSteppingFactor":
            case "worldSteppingFactor":
            {
                if (value is not float castValue) return false;
                instance.m_worldSteppingFactor = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_transferMotionEnabled":
            case "transferMotionEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_transferMotionEnabled = castValue;
                return true;
            }
            case "m_landscapeCollisionEnabled":
            case "landscapeCollisionEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_landscapeCollisionEnabled = castValue;
                return true;
            }
            case "m_pinchDetectionEnabled":
            case "pinchDetectionEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_pinchDetectionEnabled = castValue;
                return true;
            }
            case "m_virtualCollisionPointsFlags":
            case "virtualCollisionPointsFlags":
            {
                if (value is not uint castValue) return false;
                instance.m_virtualCollisionPointsFlags = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
