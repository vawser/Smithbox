// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpRagdollConstraintDataAtomsData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpRagdollConstraintData.Atoms>
{
    public hkpRagdollConstraintDataAtomsData(Havok.hk2014.hkpRagdollConstraintData.Atoms instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transforms":
            case "transforms":
            {
                value = instance.m_transforms;
                return true;
            }
            case "m_setupStabilization":
            case "setupStabilization":
            {
                value = instance.m_setupStabilization;
                return true;
            }
            case "m_ragdollMotors":
            case "ragdollMotors":
            {
                value = instance.m_ragdollMotors;
                return true;
            }
            case "m_angFriction":
            case "angFriction":
            {
                value = instance.m_angFriction;
                return true;
            }
            case "m_twistLimit":
            case "twistLimit":
            {
                value = instance.m_twistLimit;
                return true;
            }
            case "m_coneLimit":
            case "coneLimit":
            {
                value = instance.m_coneLimit;
                return true;
            }
            case "m_planesLimit":
            case "planesLimit":
            {
                value = instance.m_planesLimit;
                return true;
            }
            case "m_ballSocket":
            case "ballSocket":
            {
                value = instance.m_ballSocket;
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
            case "m_transforms":
            case "transforms":
            {
                if (value is not Havok.hk2014.hkpSetLocalTransformsConstraintAtom castValue) return false;
                instance.m_transforms = castValue;
                return true;
            }
            case "m_setupStabilization":
            case "setupStabilization":
            {
                if (value is not Havok.hk2014.hkpSetupStabilizationAtom castValue) return false;
                instance.m_setupStabilization = castValue;
                return true;
            }
            case "m_ragdollMotors":
            case "ragdollMotors":
            {
                if (value is not Havok.hk2014.hkpRagdollMotorConstraintAtom castValue) return false;
                instance.m_ragdollMotors = castValue;
                return true;
            }
            case "m_angFriction":
            case "angFriction":
            {
                if (value is not Havok.hk2014.hkpAngFrictionConstraintAtom castValue) return false;
                instance.m_angFriction = castValue;
                return true;
            }
            case "m_twistLimit":
            case "twistLimit":
            {
                if (value is not Havok.hk2014.hkpTwistLimitConstraintAtom castValue) return false;
                instance.m_twistLimit = castValue;
                return true;
            }
            case "m_coneLimit":
            case "coneLimit":
            {
                if (value is not Havok.hk2014.hkpConeLimitConstraintAtom castValue) return false;
                instance.m_coneLimit = castValue;
                return true;
            }
            case "m_planesLimit":
            case "planesLimit":
            {
                if (value is not Havok.hk2014.hkpConeLimitConstraintAtom castValue) return false;
                instance.m_planesLimit = castValue;
                return true;
            }
            case "m_ballSocket":
            case "ballSocket":
            {
                if (value is not Havok.hk2014.hkpBallSocketConstraintAtom castValue) return false;
                instance.m_ballSocket = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
