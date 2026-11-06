// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiLocalSteeringInputData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiLocalSteeringInput>
{
    public hkaiLocalSteeringInputData(Havok.hk2014.hkaiLocalSteeringInput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_currentPosition":
            case "currentPosition":
            {
                value = instance.m_currentPosition;
                return true;
            }
            case "m_currentForward":
            case "currentForward":
            {
                value = instance.m_currentForward;
                return true;
            }
            case "m_currentUp":
            case "currentUp":
            {
                value = instance.m_currentUp;
                return true;
            }
            case "m_currentVelocity":
            case "currentVelocity":
            {
                value = instance.m_currentVelocity;
                return true;
            }
            case "m_desiredVelocity":
            case "desiredVelocity":
            {
                value = instance.m_desiredVelocity;
                return true;
            }
            case "m_localGoalPlane":
            case "localGoalPlane":
            {
                value = instance.m_localGoalPlane;
                return true;
            }
            case "m_distToLocalGoal":
            case "distToLocalGoal":
            {
                value = instance.m_distToLocalGoal;
                return true;
            }
            case "m_applyKinematicConstraints":
            case "applyKinematicConstraints":
            {
                value = instance.m_applyKinematicConstraints;
                return true;
            }
            case "m_applyAvoidanceSteering":
            case "applyAvoidanceSteering":
            {
                value = instance.m_applyAvoidanceSteering;
                return true;
            }
            case "m_enableLocalSteering":
            case "enableLocalSteering":
            {
                value = instance.m_enableLocalSteering;
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
