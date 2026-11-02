// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkpLimitedHingeConstraintDataAtomsData : HavokData<HKLib.hk2019.hkpLimitedHingeConstraintData.Atoms> 
{
    public hkpLimitedHingeConstraintDataAtomsData(HavokType type, HKLib.hk2019.hkpLimitedHingeConstraintData.Atoms instance) : base(type, instance) {}

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
            case "m_angMotor":
            case "angMotor":
            {
                if (instance.m_angMotor is not TGet castValue) return false;
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
            case "m_angLimit":
            case "angLimit":
            {
                if (instance.m_angLimit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_2dAng":
            case "2dAng":
            {
                if (instance.m_2dAng is not TGet castValue) return false;
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
            case "m_angMotor":
            case "angMotor":
            {
                if (value is not HKLib.hk2019.hkpAngMotorConstraintAtom castValue) return false;
                instance.m_angMotor = castValue;
                return true;
            }
            case "m_angFriction":
            case "angFriction":
            {
                if (value is not HKLib.hk2019.hkpAngFrictionConstraintAtom castValue) return false;
                instance.m_angFriction = castValue;
                return true;
            }
            case "m_angLimit":
            case "angLimit":
            {
                if (value is not HKLib.hk2019.hkpAngLimitConstraintAtom castValue) return false;
                instance.m_angLimit = castValue;
                return true;
            }
            case "m_2dAng":
            case "2dAng":
            {
                if (value is not HKLib.hk2019.hkp2dAngConstraintAtom castValue) return false;
                instance.m_2dAng = castValue;
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
