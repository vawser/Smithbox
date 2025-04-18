// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkp6DofConstraintDataBlueprintsData : HavokData<hkp6DofConstraintData.Blueprints> 
{
    private static readonly System.Reflection.FieldInfo _axisModeInfo = typeof(hkp6DofConstraintData.Blueprints).GetField("m_axisMode")!;
    private static readonly System.Reflection.FieldInfo _ellipticalMinMaxInfo = typeof(hkp6DofConstraintData.Blueprints).GetField("m_ellipticalMinMax")!;
    private static readonly System.Reflection.FieldInfo _angFrictionsInfo = typeof(hkp6DofConstraintData.Blueprints).GetField("m_angFrictions")!;
    private static readonly System.Reflection.FieldInfo _linearLimitsInfo = typeof(hkp6DofConstraintData.Blueprints).GetField("m_linearLimits")!;
    private static readonly System.Reflection.FieldInfo _linearFrictionInfo = typeof(hkp6DofConstraintData.Blueprints).GetField("m_linearFriction")!;
    public hkp6DofConstraintDataBlueprintsData(HavokType type, hkp6DofConstraintData.Blueprints instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_axisMode":
            case "axisMode":
            {
                if (instance.m_axisMode is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ellipticalMinMax":
            case "ellipticalMinMax":
            {
                if (instance.m_ellipticalMinMax is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_angFrictions":
            case "angFrictions":
            {
                if (instance.m_angFrictions is not TGet castValue) return false;
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
            case "m_ellipticalLimit":
            case "ellipticalLimit":
            {
                if (instance.m_ellipticalLimit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stiffSpring":
            case "stiffSpring":
            {
                if (instance.m_stiffSpring is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_linearLimits":
            case "linearLimits":
            {
                if (instance.m_linearLimits is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_linearFriction":
            case "linearFriction":
            {
                if (instance.m_linearFriction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_linearMotor0":
            case "linearMotor0":
            {
                if (instance.m_linearMotor0 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_linearMotor1":
            case "linearMotor1":
            {
                if (instance.m_linearMotor1 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_linearMotor2":
            case "linearMotor2":
            {
                if (instance.m_linearMotor2 is not TGet castValue) return false;
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
            case "m_axisMode":
            case "axisMode":
            {
                if (value is not hkp6DofConstraintData.AxisMode[] castValue || castValue.Length != 6) return false;
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
                if (value is not hkpSetLocalTransformsConstraintAtom castValue) return false;
                instance.m_transforms = castValue;
                return true;
            }
            case "m_setupStabilization":
            case "setupStabilization":
            {
                if (value is not hkpSetupStabilizationAtom castValue) return false;
                instance.m_setupStabilization = castValue;
                return true;
            }
            case "m_ragdollMotors":
            case "ragdollMotors":
            {
                if (value is not hkpRagdollMotorConstraintAtom castValue) return false;
                instance.m_ragdollMotors = castValue;
                return true;
            }
            case "m_angFrictions":
            case "angFrictions":
            {
                if (value is not hkpAngFrictionConstraintAtom[] castValue || castValue.Length != 3) return false;
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
                if (value is not hkpTwistLimitConstraintAtom castValue) return false;
                instance.m_twistLimit = castValue;
                return true;
            }
            case "m_ellipticalLimit":
            case "ellipticalLimit":
            {
                if (value is not hkpEllipticalLimitConstraintAtom castValue) return false;
                instance.m_ellipticalLimit = castValue;
                return true;
            }
            case "m_stiffSpring":
            case "stiffSpring":
            {
                if (value is not hkpStiffSpringConstraintAtom castValue) return false;
                instance.m_stiffSpring = castValue;
                return true;
            }
            case "m_linearLimits":
            case "linearLimits":
            {
                if (value is not hkpLinLimitConstraintAtom[] castValue || castValue.Length != 3) return false;
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
                if (value is not hkpLinFrictionConstraintAtom[] castValue || castValue.Length != 3) return false;
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
                if (value is not hkpLinMotorConstraintAtom castValue) return false;
                instance.m_linearMotor0 = castValue;
                return true;
            }
            case "m_linearMotor1":
            case "linearMotor1":
            {
                if (value is not hkpLinMotorConstraintAtom castValue) return false;
                instance.m_linearMotor1 = castValue;
                return true;
            }
            case "m_linearMotor2":
            case "linearMotor2":
            {
                if (value is not hkpLinMotorConstraintAtom castValue) return false;
                instance.m_linearMotor2 = castValue;
                return true;
            }
            case "m_ballSocket":
            case "ballSocket":
            {
                if (value is not hkpBallSocketConstraintAtom castValue) return false;
                instance.m_ballSocket = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
