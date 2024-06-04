// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiLocalSteeringInputData : HavokData<hkaiLocalSteeringInput> 
{
    public hkaiLocalSteeringInputData(HavokType type, hkaiLocalSteeringInput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_currentPosition":
            case "currentPosition":
            {
                if (instance.m_currentPosition is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_currentForward":
            case "currentForward":
            {
                if (instance.m_currentForward is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_currentUp":
            case "currentUp":
            {
                if (instance.m_currentUp is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_currentVelocity":
            case "currentVelocity":
            {
                if (instance.m_currentVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_desiredVelocity":
            case "desiredVelocity":
            {
                if (instance.m_desiredVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localGoalPlane":
            case "localGoalPlane":
            {
                if (instance.m_localGoalPlane is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_distToLocalGoal":
            case "distToLocalGoal":
            {
                if (instance.m_distToLocalGoal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_applyKinematicConstraints":
            case "applyKinematicConstraints":
            {
                if (instance.m_applyKinematicConstraints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_applyAvoidanceSteering":
            case "applyAvoidanceSteering":
            {
                if (instance.m_applyAvoidanceSteering is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableLocalSteering":
            case "enableLocalSteering":
            {
                if (instance.m_enableLocalSteering is not TGet castValue) return false;
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
            case "m_currentPosition":
            case "currentPosition":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_currentPosition = castValue;
                return true;
            }
            case "m_currentForward":
            case "currentForward":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_currentForward = castValue;
                return true;
            }
            case "m_currentUp":
            case "currentUp":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_currentUp = castValue;
                return true;
            }
            case "m_currentVelocity":
            case "currentVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_currentVelocity = castValue;
                return true;
            }
            case "m_desiredVelocity":
            case "desiredVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_desiredVelocity = castValue;
                return true;
            }
            case "m_localGoalPlane":
            case "localGoalPlane":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_localGoalPlane = castValue;
                return true;
            }
            case "m_distToLocalGoal":
            case "distToLocalGoal":
            {
                if (value is not float castValue) return false;
                instance.m_distToLocalGoal = castValue;
                return true;
            }
            case "m_applyKinematicConstraints":
            case "applyKinematicConstraints":
            {
                if (value is not bool castValue) return false;
                instance.m_applyKinematicConstraints = castValue;
                return true;
            }
            case "m_applyAvoidanceSteering":
            case "applyAvoidanceSteering":
            {
                if (value is not bool castValue) return false;
                instance.m_applyAvoidanceSteering = castValue;
                return true;
            }
            case "m_enableLocalSteering":
            case "enableLocalSteering":
            {
                if (value is not bool castValue) return false;
                instance.m_enableLocalSteering = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
