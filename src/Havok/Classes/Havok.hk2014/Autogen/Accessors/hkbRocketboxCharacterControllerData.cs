// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbRocketboxCharacterControllerData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbRocketboxCharacterController>
{
    public hkbRocketboxCharacterControllerData(Havok.hk2014.hkbRocketboxCharacterController instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                value = instance.m_variableBindingSet;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_enable":
            case "enable":
            {
                value = instance.m_enable;
                return true;
            }
            case "m_child":
            case "child":
            {
                value = instance.m_child;
                return true;
            }
            case "m_autoTurnsAllowed":
            case "autoTurnsAllowed":
            {
                value = instance.m_autoTurnsAllowed;
                return true;
            }
            case "m_desiredAIMovementMode":
            case "desiredAIMovementMode":
            {
                value = instance.m_desiredAIMovementMode;
                return true;
            }
            case "m_effectiveLinearSpeed":
            case "effectiveLinearSpeed":
            {
                value = instance.m_effectiveLinearSpeed;
                return true;
            }
            case "m_effectiveAngularSpeed":
            case "effectiveAngularSpeed":
            {
                value = instance.m_effectiveAngularSpeed;
                return true;
            }
            case "m_effectiveHorizontalAim":
            case "effectiveHorizontalAim":
            {
                value = instance.m_effectiveHorizontalAim;
                return true;
            }
            case "m_effectiveVerticalAim":
            case "effectiveVerticalAim":
            {
                value = instance.m_effectiveVerticalAim;
                return true;
            }
            case "m_torsoTiltAngle":
            case "torsoTiltAngle":
            {
                value = instance.m_torsoTiltAngle;
                return true;
            }
            case "m_desiredAIMovementSpeed":
            case "desiredAIMovementSpeed":
            {
                value = instance.m_desiredAIMovementSpeed;
                return true;
            }
            case "m_currentMaximumSpeed":
            case "currentMaximumSpeed":
            {
                value = instance.m_currentMaximumSpeed;
                return true;
            }
            case "m_stopRequest":
            case "stopRequest":
            {
                value = instance.m_stopRequest;
                return true;
            }
            case "m_idleToMoveAnimIdx":
            case "idleToMoveAnimIdx":
            {
                value = instance.m_idleToMoveAnimIdx;
                return true;
            }
            case "m_linearSpeed":
            case "linearSpeed":
            {
                value = instance.m_linearSpeed;
                return true;
            }
            case "m_angularSpeed":
            case "angularSpeed":
            {
                value = instance.m_angularSpeed;
                return true;
            }
            case "m_horizontalAim":
            case "horizontalAim":
            {
                value = instance.m_horizontalAim;
                return true;
            }
            case "m_verticalAim":
            case "verticalAim":
            {
                value = instance.m_verticalAim;
                return true;
            }
            case "m_rotationAllowed":
            case "rotationAllowed":
            {
                value = instance.m_rotationAllowed;
                return true;
            }
            case "m_poseChangeAllowed":
            case "poseChangeAllowed":
            {
                value = instance.m_poseChangeAllowed;
                return true;
            }
            case "m_modifyLinearSpeed":
            case "modifyLinearSpeed":
            {
                value = instance.m_modifyLinearSpeed;
                return true;
            }
            case "m_poseIdx":
            case "poseIdx":
            {
                value = instance.m_poseIdx;
                return true;
            }
            case "m_leftFootDownEvent":
            case "leftFootDownEvent":
            {
                value = instance.m_leftFootDownEvent;
                return true;
            }
            case "m_rightFootDownEvent":
            case "rightFootDownEvent":
            {
                value = instance.m_rightFootDownEvent;
                return true;
            }
            case "m_immediateStopEvent":
            case "immediateStopEvent":
            {
                value = instance.m_immediateStopEvent;
                return true;
            }
            case "m_rapidTurnEvent":
            case "rapidTurnEvent":
            {
                value = instance.m_rapidTurnEvent;
                return true;
            }
            case "m_changeMovementDirectionEvent":
            case "changeMovementDirectionEvent":
            {
                value = instance.m_changeMovementDirectionEvent;
                return true;
            }
            case "m_changePoseEvent":
            case "changePoseEvent":
            {
                value = instance.m_changePoseEvent;
                return true;
            }
            case "m_moveEvent":
            case "moveEvent":
            {
                value = instance.m_moveEvent;
                return true;
            }
            case "m_stopEvent":
            case "stopEvent":
            {
                value = instance.m_stopEvent;
                return true;
            }
            case "m_characterHasHalted":
            case "characterHasHalted":
            {
                value = instance.m_characterHasHalted;
                return true;
            }
            case "m_moveVelocities":
            case "moveVelocities":
            {
                value = instance.m_moveVelocities;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbVariableBindingSet castValue)
                {
                    instance.m_variableBindingSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_enable":
            case "enable":
            {
                if (value is not bool castValue) return false;
                instance.m_enable = castValue;
                return true;
            }
            case "m_child":
            case "child":
            {
                if (value is null)
                {
                    instance.m_child = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbGenerator castValue)
                {
                    instance.m_child = castValue;
                    return true;
                }
                return false;
            }
            case "m_autoTurnsAllowed":
            case "autoTurnsAllowed":
            {
                if (value is not bool castValue) return false;
                instance.m_autoTurnsAllowed = castValue;
                return true;
            }
            case "m_desiredAIMovementMode":
            case "desiredAIMovementMode":
            {
                if (value is not int castValue) return false;
                instance.m_desiredAIMovementMode = castValue;
                return true;
            }
            case "m_effectiveLinearSpeed":
            case "effectiveLinearSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_effectiveLinearSpeed = castValue;
                return true;
            }
            case "m_effectiveAngularSpeed":
            case "effectiveAngularSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_effectiveAngularSpeed = castValue;
                return true;
            }
            case "m_effectiveHorizontalAim":
            case "effectiveHorizontalAim":
            {
                if (value is not float castValue) return false;
                instance.m_effectiveHorizontalAim = castValue;
                return true;
            }
            case "m_effectiveVerticalAim":
            case "effectiveVerticalAim":
            {
                if (value is not float castValue) return false;
                instance.m_effectiveVerticalAim = castValue;
                return true;
            }
            case "m_torsoTiltAngle":
            case "torsoTiltAngle":
            {
                if (value is not float castValue) return false;
                instance.m_torsoTiltAngle = castValue;
                return true;
            }
            case "m_desiredAIMovementSpeed":
            case "desiredAIMovementSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_desiredAIMovementSpeed = castValue;
                return true;
            }
            case "m_currentMaximumSpeed":
            case "currentMaximumSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_currentMaximumSpeed = castValue;
                return true;
            }
            case "m_stopRequest":
            case "stopRequest":
            {
                if (value is not bool castValue) return false;
                instance.m_stopRequest = castValue;
                return true;
            }
            case "m_idleToMoveAnimIdx":
            case "idleToMoveAnimIdx":
            {
                if (value is not int castValue) return false;
                instance.m_idleToMoveAnimIdx = castValue;
                return true;
            }
            case "m_linearSpeed":
            case "linearSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_linearSpeed = castValue;
                return true;
            }
            case "m_angularSpeed":
            case "angularSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_angularSpeed = castValue;
                return true;
            }
            case "m_horizontalAim":
            case "horizontalAim":
            {
                if (value is not float castValue) return false;
                instance.m_horizontalAim = castValue;
                return true;
            }
            case "m_verticalAim":
            case "verticalAim":
            {
                if (value is not float castValue) return false;
                instance.m_verticalAim = castValue;
                return true;
            }
            case "m_rotationAllowed":
            case "rotationAllowed":
            {
                if (value is not bool castValue) return false;
                instance.m_rotationAllowed = castValue;
                return true;
            }
            case "m_poseChangeAllowed":
            case "poseChangeAllowed":
            {
                if (value is not bool castValue) return false;
                instance.m_poseChangeAllowed = castValue;
                return true;
            }
            case "m_modifyLinearSpeed":
            case "modifyLinearSpeed":
            {
                if (value is not bool castValue) return false;
                instance.m_modifyLinearSpeed = castValue;
                return true;
            }
            case "m_poseIdx":
            case "poseIdx":
            {
                if (value is not int castValue) return false;
                instance.m_poseIdx = castValue;
                return true;
            }
            case "m_leftFootDownEvent":
            case "leftFootDownEvent":
            {
                if (value is not Havok.hk2014.hkbEventProperty castValue) return false;
                instance.m_leftFootDownEvent = castValue;
                return true;
            }
            case "m_rightFootDownEvent":
            case "rightFootDownEvent":
            {
                if (value is not Havok.hk2014.hkbEventProperty castValue) return false;
                instance.m_rightFootDownEvent = castValue;
                return true;
            }
            case "m_immediateStopEvent":
            case "immediateStopEvent":
            {
                if (value is not Havok.hk2014.hkbEventProperty castValue) return false;
                instance.m_immediateStopEvent = castValue;
                return true;
            }
            case "m_rapidTurnEvent":
            case "rapidTurnEvent":
            {
                if (value is not Havok.hk2014.hkbEventProperty castValue) return false;
                instance.m_rapidTurnEvent = castValue;
                return true;
            }
            case "m_changeMovementDirectionEvent":
            case "changeMovementDirectionEvent":
            {
                if (value is not Havok.hk2014.hkbEventProperty castValue) return false;
                instance.m_changeMovementDirectionEvent = castValue;
                return true;
            }
            case "m_changePoseEvent":
            case "changePoseEvent":
            {
                if (value is not Havok.hk2014.hkbEventProperty castValue) return false;
                instance.m_changePoseEvent = castValue;
                return true;
            }
            case "m_moveEvent":
            case "moveEvent":
            {
                if (value is not Havok.hk2014.hkbEventProperty castValue) return false;
                instance.m_moveEvent = castValue;
                return true;
            }
            case "m_stopEvent":
            case "stopEvent":
            {
                if (value is not Havok.hk2014.hkbEventProperty castValue) return false;
                instance.m_stopEvent = castValue;
                return true;
            }
            case "m_characterHasHalted":
            case "characterHasHalted":
            {
                if (value is not Havok.hk2014.hkbEventProperty castValue) return false;
                instance.m_characterHasHalted = castValue;
                return true;
            }
            case "m_moveVelocities":
            case "moveVelocities":
            {
                if (value is not List<float> castValue) return false;
                instance.m_moveVelocities = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
