// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkpLimitedHingeConstraintDataAtomsData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkpLimitedHingeConstraintData.Atoms>
{
    public hkpLimitedHingeConstraintDataAtomsData(Havok.hk55.hkpLimitedHingeConstraintData.Atoms instance) : base(instance) { }

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
            case "m_angMotor":
            case "angMotor":
            {
                value = instance.m_angMotor;
                return true;
            }
            case "m_angFriction":
            case "angFriction":
            {
                value = instance.m_angFriction;
                return true;
            }
            case "m_angLimit":
            case "angLimit":
            {
                value = instance.m_angLimit;
                return true;
            }
            case "m_2dAng":
            case "2dAng":
            {
                value = instance.m_2dAng;
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
                if (value is not Havok.hk55.hkpSetLocalTransformsConstraintAtom castValue) return false;
                instance.m_transforms = castValue;
                return true;
            }
            case "m_angMotor":
            case "angMotor":
            {
                if (value is not Havok.hk55.hkpAngMotorConstraintAtom castValue) return false;
                instance.m_angMotor = castValue;
                return true;
            }
            case "m_angFriction":
            case "angFriction":
            {
                if (value is not Havok.hk55.hkpAngFrictionConstraintAtom castValue) return false;
                instance.m_angFriction = castValue;
                return true;
            }
            case "m_angLimit":
            case "angLimit":
            {
                if (value is not Havok.hk55.hkpAngLimitConstraintAtom castValue) return false;
                instance.m_angLimit = castValue;
                return true;
            }
            case "m_2dAng":
            case "2dAng":
            {
                if (value is not Havok.hk55.hkp2dAngConstraintAtom castValue) return false;
                instance.m_2dAng = castValue;
                return true;
            }
            case "m_ballSocket":
            case "ballSocket":
            {
                if (value is not Havok.hk55.hkpBallSocketConstraintAtom castValue) return false;
                instance.m_ballSocket = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
