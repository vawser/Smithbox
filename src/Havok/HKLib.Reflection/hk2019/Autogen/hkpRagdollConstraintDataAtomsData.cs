// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkpRagdollConstraintDataAtomsData : HavokData<HKLib.hk2019.hkpRagdollConstraintData.Atoms> 
{
    public hkpRagdollConstraintDataAtomsData(HavokType type, HKLib.hk2019.hkpRagdollConstraintData.Atoms instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transforms":
            case "transforms":
            {
                if (instance.m_transforms is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_setupStabilization":
            case "setupStabilization":
            {
                if (instance.m_setupStabilization is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ragdollMotors":
            case "ragdollMotors":
            {
                if (instance.m_ragdollMotors is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angFriction":
            case "angFriction":
            {
                if (instance.m_angFriction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_twistLimit":
            case "twistLimit":
            {
                if (instance.m_twistLimit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_coneLimit":
            case "coneLimit":
            {
                if (instance.m_coneLimit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_planesLimit":
            case "planesLimit":
            {
                if (instance.m_planesLimit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ballSocket":
            case "ballSocket":
            {
                if (instance.m_ballSocket is not TGet castValue) return false;
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
            case "m_transforms":
            case "transforms":
            {
                if (value is not HKLib.hk2019.hkpSetLocalTransformsConstraintAtom castValue) return false;
                instance.m_transforms = castValue;
                return true;
            }
            case "m_setupStabilization":
            case "setupStabilization":
            {
                if (value is not HKLib.hk2019.hkpSetupStabilizationAtom castValue) return false;
                instance.m_setupStabilization = castValue;
                return true;
            }
            case "m_ragdollMotors":
            case "ragdollMotors":
            {
                if (value is not HKLib.hk2019.hkpRagdollMotorConstraintAtom castValue) return false;
                instance.m_ragdollMotors = castValue;
                return true;
            }
            case "m_angFriction":
            case "angFriction":
            {
                if (value is not HKLib.hk2019.hkpAngFrictionConstraintAtom castValue) return false;
                instance.m_angFriction = castValue;
                return true;
            }
            case "m_twistLimit":
            case "twistLimit":
            {
                if (value is not HKLib.hk2019.hkpTwistLimitConstraintAtom castValue) return false;
                instance.m_twistLimit = castValue;
                return true;
            }
            case "m_coneLimit":
            case "coneLimit":
            {
                if (value is not HKLib.hk2019.hkpConeLimitConstraintAtom castValue) return false;
                instance.m_coneLimit = castValue;
                return true;
            }
            case "m_planesLimit":
            case "planesLimit":
            {
                if (value is not HKLib.hk2019.hkpConeLimitConstraintAtom castValue) return false;
                instance.m_planesLimit = castValue;
                return true;
            }
            case "m_ballSocket":
            case "ballSocket":
            {
                if (value is not HKLib.hk2019.hkpBallSocketConstraintAtom castValue) return false;
                instance.m_ballSocket = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
