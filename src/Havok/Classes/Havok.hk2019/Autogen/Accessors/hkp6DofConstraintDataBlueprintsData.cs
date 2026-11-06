// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkp6DofConstraintDataBlueprintsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkp6DofConstraintData.Blueprints>
{
    private static readonly System.Reflection.FieldInfo _axisModeInfo = typeof(Havok.hk2019.hkp6DofConstraintData.Blueprints).GetField("m_axisMode")!;
    private static readonly System.Reflection.FieldInfo _ellipticalMinMaxInfo = typeof(Havok.hk2019.hkp6DofConstraintData.Blueprints).GetField("m_ellipticalMinMax")!;
    private static readonly System.Reflection.FieldInfo _angFrictionsInfo = typeof(Havok.hk2019.hkp6DofConstraintData.Blueprints).GetField("m_angFrictions")!;
    private static readonly System.Reflection.FieldInfo _linearLimitsInfo = typeof(Havok.hk2019.hkp6DofConstraintData.Blueprints).GetField("m_linearLimits")!;
    private static readonly System.Reflection.FieldInfo _linearFrictionInfo = typeof(Havok.hk2019.hkp6DofConstraintData.Blueprints).GetField("m_linearFriction")!;
    public hkp6DofConstraintDataBlueprintsData(Havok.hk2019.hkp6DofConstraintData.Blueprints instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_axisMode":
            case "axisMode":
            {
                value = instance.m_axisMode;
                return true;
            }
            case "m_ellipticalMinMax":
            case "ellipticalMinMax":
            {
                value = instance.m_ellipticalMinMax;
                return true;
            }
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
            case "m_angFrictions":
            case "angFrictions":
            {
                value = instance.m_angFrictions;
                return true;
            }
            case "m_twistLimit":
            case "twistLimit":
            {
                value = instance.m_twistLimit;
                return true;
            }
            case "m_ellipticalLimit":
            case "ellipticalLimit":
            {
                value = instance.m_ellipticalLimit;
                return true;
            }
            case "m_stiffSpring":
            case "stiffSpring":
            {
                value = instance.m_stiffSpring;
                return true;
            }
            case "m_linearLimits":
            case "linearLimits":
            {
                value = instance.m_linearLimits;
                return true;
            }
            case "m_linearFriction":
            case "linearFriction":
            {
                value = instance.m_linearFriction;
                return true;
            }
            case "m_linearMotor0":
            case "linearMotor0":
            {
                value = instance.m_linearMotor0;
                return true;
            }
            case "m_linearMotor1":
            case "linearMotor1":
            {
                value = instance.m_linearMotor1;
                return true;
            }
            case "m_linearMotor2":
            case "linearMotor2":
            {
                value = instance.m_linearMotor2;
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
            case "m_axisMode":
            case "axisMode":
            {
                if (value is not Havok.hk2019.hkp6DofConstraintData.AxisMode[] castValue || castValue.Length != 6) return false;
                try
                {
                    _axisModeInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_ellipticalMinMax":
            case "ellipticalMinMax":
            {
                if (value is not float[] castValue || castValue.Length != 4) return false;
                try
                {
                    _ellipticalMinMaxInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_transforms":
            case "transforms":
            {
                if (value is not Havok.hk2019.hkpSetLocalTransformsConstraintAtom castValue) return false;
                instance.m_transforms = castValue;
                return true;
            }
            case "m_setupStabilization":
            case "setupStabilization":
            {
                if (value is not Havok.hk2019.hkpSetupStabilizationAtom castValue) return false;
                instance.m_setupStabilization = castValue;
                return true;
            }
            case "m_ragdollMotors":
            case "ragdollMotors":
            {
                if (value is not Havok.hk2019.hkpRagdollMotorConstraintAtom castValue) return false;
                instance.m_ragdollMotors = castValue;
                return true;
            }
            case "m_angFrictions":
            case "angFrictions":
            {
                if (value is not Havok.hk2019.hkpAngFrictionConstraintAtom[] castValue || castValue.Length != 3) return false;
                try
                {
                    _angFrictionsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_twistLimit":
            case "twistLimit":
            {
                if (value is not Havok.hk2019.hkpTwistLimitConstraintAtom castValue) return false;
                instance.m_twistLimit = castValue;
                return true;
            }
            case "m_ellipticalLimit":
            case "ellipticalLimit":
            {
                if (value is not Havok.hk2019.hkpEllipticalLimitConstraintAtom castValue) return false;
                instance.m_ellipticalLimit = castValue;
                return true;
            }
            case "m_stiffSpring":
            case "stiffSpring":
            {
                if (value is not Havok.hk2019.hkpStiffSpringConstraintAtom castValue) return false;
                instance.m_stiffSpring = castValue;
                return true;
            }
            case "m_linearLimits":
            case "linearLimits":
            {
                if (value is not Havok.hk2019.hkpLinLimitConstraintAtom[] castValue || castValue.Length != 3) return false;
                try
                {
                    _linearLimitsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_linearFriction":
            case "linearFriction":
            {
                if (value is not Havok.hk2019.hkpLinFrictionConstraintAtom[] castValue || castValue.Length != 3) return false;
                try
                {
                    _linearFrictionInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_linearMotor0":
            case "linearMotor0":
            {
                if (value is not Havok.hk2019.hkpLinMotorConstraintAtom castValue) return false;
                instance.m_linearMotor0 = castValue;
                return true;
            }
            case "m_linearMotor1":
            case "linearMotor1":
            {
                if (value is not Havok.hk2019.hkpLinMotorConstraintAtom castValue) return false;
                instance.m_linearMotor1 = castValue;
                return true;
            }
            case "m_linearMotor2":
            case "linearMotor2":
            {
                if (value is not Havok.hk2019.hkpLinMotorConstraintAtom castValue) return false;
                instance.m_linearMotor2 = castValue;
                return true;
            }
            case "m_ballSocket":
            case "ballSocket":
            {
                if (value is not Havok.hk2019.hkpBallSocketConstraintAtom castValue) return false;
                instance.m_ballSocket = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
