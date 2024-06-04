// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hclConstraintStiffnessDispatcher;
using Type = HKLib.hk2018.hclConstraintStiffnessDispatcher.Type;

namespace HKLib.Reflection.hk2018;

internal class hclSimClothInstanceData : HavokData<hclSimClothInstance> 
{
    public hclSimClothInstanceData(HavokType type, hclSimClothInstance instance) : base(type, instance) {}

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
            case "m_lastTimeStepUsed":
            case "lastTimeStepUsed":
            {
                if (instance.m_lastTimeStepUsed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_effectiveGlobalDampingInv":
            case "effectiveGlobalDampingInv":
            {
                if (instance.m_effectiveGlobalDampingInv is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particlesAabb":
            case "particlesAabb":
            {
                if (instance.m_particlesAabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_collisionParticlesAabb":
            case "collisionParticlesAabb":
            {
                if (instance.m_collisionParticlesAabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_updateCollisionAabbs":
            case "updateCollisionAabbs":
            {
                if (instance.m_updateCollisionAabbs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_landscapeCollisionParticlesAabb":
            case "landscapeCollisionParticlesAabb":
            {
                if (instance.m_landscapeCollisionParticlesAabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_previousTransferWorldFromModel":
            case "previousTransferWorldFromModel":
            {
                if (instance.m_previousTransferWorldFromModel is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_constraintStiffnessModelType":
            case "constraintStiffnessModelType":
            {
                if (instance.m_constraintStiffnessModelType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_constraintStiffnessModelType is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_slowMotionFactor":
            case "slowMotionFactor":
            {
                if (instance.m_slowMotionFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_worldSteppingFactor":
            case "worldSteppingFactor":
            {
                if (instance.m_worldSteppingFactor is not TGet castValue) return false;
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
            case "m_transferMotionEnabled":
            case "transferMotionEnabled":
            {
                if (instance.m_transferMotionEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_landscapeCollisionEnabled":
            case "landscapeCollisionEnabled":
            {
                if (instance.m_landscapeCollisionEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pinchDetectionEnabled":
            case "pinchDetectionEnabled":
            {
                if (instance.m_pinchDetectionEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_virtualCollisionPointsFlags":
            case "virtualCollisionPointsFlags":
            {
                if (instance.m_virtualCollisionPointsFlags is not TGet castValue) return false;
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
                if (value is not hkAabb castValue) return false;
                instance.m_particlesAabb = castValue;
                return true;
            }
            case "m_collisionParticlesAabb":
            case "collisionParticlesAabb":
            {
                if (value is not hkAabb castValue) return false;
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
                if (value is not hkAabb castValue) return false;
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
                if (value is Type castValue)
                {
                    instance.m_constraintStiffnessModelType = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_constraintStiffnessModelType = (Type)intValue;
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
