// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbpTargetRigidBodyModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbpTargetRigidBodyModifier>
{
    public hkbpTargetRigidBodyModifierData(Havok.hk2014.hkbpTargetRigidBodyModifier instance) : base(instance) { }

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
            case "m_targetOut":
            case "targetOut":
            {
                value = instance.m_targetOut;
                return true;
            }
            case "m_targetMode":
            case "targetMode":
            {
                value = instance.m_targetMode;
                return true;
            }
            case "m_sensingLayer":
            case "sensingLayer":
            {
                value = instance.m_sensingLayer;
                return true;
            }
            case "m_targetOnlyOnce":
            case "targetOnlyOnce":
            {
                value = instance.m_targetOnlyOnce;
                return true;
            }
            case "m_ignoreMySystemGroup":
            case "ignoreMySystemGroup":
            {
                value = instance.m_ignoreMySystemGroup;
                return true;
            }
            case "m_maxTargetDistance":
            case "maxTargetDistance":
            {
                value = instance.m_maxTargetDistance;
                return true;
            }
            case "m_maxTargetHeightAboveSensor":
            case "maxTargetHeightAboveSensor":
            {
                value = instance.m_maxTargetHeightAboveSensor;
                return true;
            }
            case "m_closeToTargetDistanceThreshold":
            case "closeToTargetDistanceThreshold":
            {
                value = instance.m_closeToTargetDistanceThreshold;
                return true;
            }
            case "m_targetAngleMode":
            case "targetAngleMode":
            {
                value = instance.m_targetAngleMode;
                return true;
            }
            case "m_targetDistanceMode":
            case "targetDistanceMode":
            {
                value = instance.m_targetDistanceMode;
                return true;
            }
            case "m_maxAngleToTarget":
            case "maxAngleToTarget":
            {
                value = instance.m_maxAngleToTarget;
                return true;
            }
            case "m_sensorRagdollBoneIndex":
            case "sensorRagdollBoneIndex":
            {
                value = instance.m_sensorRagdollBoneIndex;
                return true;
            }
            case "m_sensorAnimationBoneIndex":
            case "sensorAnimationBoneIndex":
            {
                value = instance.m_sensorAnimationBoneIndex;
                return true;
            }
            case "m_closeToTargetRagdollBoneIndex":
            case "closeToTargetRagdollBoneIndex":
            {
                value = instance.m_closeToTargetRagdollBoneIndex;
                return true;
            }
            case "m_closeToTargetAnimationBoneIndex":
            case "closeToTargetAnimationBoneIndex":
            {
                value = instance.m_closeToTargetAnimationBoneIndex;
                return true;
            }
            case "m_sensorOffsetInBoneSpace":
            case "sensorOffsetInBoneSpace":
            {
                value = instance.m_sensorOffsetInBoneSpace;
                return true;
            }
            case "m_closeToTargetOffsetInBoneSpace":
            case "closeToTargetOffsetInBoneSpace":
            {
                value = instance.m_closeToTargetOffsetInBoneSpace;
                return true;
            }
            case "m_sensorDirectionBS":
            case "sensorDirectionBS":
            {
                value = instance.m_sensorDirectionBS;
                return true;
            }
            case "m_eventMode":
            case "eventMode":
            {
                value = instance.m_eventMode;
                return true;
            }
            case "m_sensingPropertyKey":
            case "sensingPropertyKey":
            {
                value = instance.m_sensingPropertyKey;
                return true;
            }
            case "m_sensorInWS":
            case "sensorInWS":
            {
                value = instance.m_sensorInWS;
                return true;
            }
            case "m_eventToSend":
            case "eventToSend":
            {
                value = instance.m_eventToSend;
                return true;
            }
            case "m_eventToSendToTarget":
            case "eventToSendToTarget":
            {
                value = instance.m_eventToSendToTarget;
                return true;
            }
            case "m_closeToTargetEvent":
            case "closeToTargetEvent":
            {
                value = instance.m_closeToTargetEvent;
                return true;
            }
            case "m_useVelocityPrediction":
            case "useVelocityPrediction":
            {
                value = instance.m_useVelocityPrediction;
                return true;
            }
            case "m_targetOnlySpheres":
            case "targetOnlySpheres":
            {
                value = instance.m_targetOnlySpheres;
                return true;
            }
            case "m_isCloseToTargetOut":
            case "isCloseToTargetOut":
            {
                value = instance.m_isCloseToTargetOut;
                return true;
            }
            case "m_targetPriority":
            case "targetPriority":
            {
                value = instance.m_targetPriority;
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
            case "m_targetOut":
            case "targetOut":
            {
                if (value is null)
                {
                    instance.m_targetOut = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbpTarget castValue)
                {
                    instance.m_targetOut = castValue;
                    return true;
                }
                return false;
            }
            case "m_targetMode":
            case "targetMode":
            {
                if (value is Havok.hk2014.hkbpTargetRigidBodyModifier.TargetMode castValue)
                {
                    instance.m_targetMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_targetMode = (Havok.hk2014.hkbpTargetRigidBodyModifier.TargetMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_sensingLayer":
            case "sensingLayer":
            {
                if (value is not int castValue) return false;
                instance.m_sensingLayer = castValue;
                return true;
            }
            case "m_targetOnlyOnce":
            case "targetOnlyOnce":
            {
                if (value is not bool castValue) return false;
                instance.m_targetOnlyOnce = castValue;
                return true;
            }
            case "m_ignoreMySystemGroup":
            case "ignoreMySystemGroup":
            {
                if (value is not bool castValue) return false;
                instance.m_ignoreMySystemGroup = castValue;
                return true;
            }
            case "m_maxTargetDistance":
            case "maxTargetDistance":
            {
                if (value is not float castValue) return false;
                instance.m_maxTargetDistance = castValue;
                return true;
            }
            case "m_maxTargetHeightAboveSensor":
            case "maxTargetHeightAboveSensor":
            {
                if (value is not float castValue) return false;
                instance.m_maxTargetHeightAboveSensor = castValue;
                return true;
            }
            case "m_closeToTargetDistanceThreshold":
            case "closeToTargetDistanceThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_closeToTargetDistanceThreshold = castValue;
                return true;
            }
            case "m_targetAngleMode":
            case "targetAngleMode":
            {
                if (value is Havok.hk2014.hkbpTargetRigidBodyModifier.ComputeTargetAngleMode castValue)
                {
                    instance.m_targetAngleMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_targetAngleMode = (Havok.hk2014.hkbpTargetRigidBodyModifier.ComputeTargetAngleMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_targetDistanceMode":
            case "targetDistanceMode":
            {
                if (value is Havok.hk2014.hkbpTargetRigidBodyModifier.ComputeTargetDistanceMode castValue)
                {
                    instance.m_targetDistanceMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_targetDistanceMode = (Havok.hk2014.hkbpTargetRigidBodyModifier.ComputeTargetDistanceMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_maxAngleToTarget":
            case "maxAngleToTarget":
            {
                if (value is not float castValue) return false;
                instance.m_maxAngleToTarget = castValue;
                return true;
            }
            case "m_sensorRagdollBoneIndex":
            case "sensorRagdollBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_sensorRagdollBoneIndex = castValue;
                return true;
            }
            case "m_sensorAnimationBoneIndex":
            case "sensorAnimationBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_sensorAnimationBoneIndex = castValue;
                return true;
            }
            case "m_closeToTargetRagdollBoneIndex":
            case "closeToTargetRagdollBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_closeToTargetRagdollBoneIndex = castValue;
                return true;
            }
            case "m_closeToTargetAnimationBoneIndex":
            case "closeToTargetAnimationBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_closeToTargetAnimationBoneIndex = castValue;
                return true;
            }
            case "m_sensorOffsetInBoneSpace":
            case "sensorOffsetInBoneSpace":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_sensorOffsetInBoneSpace = castValue;
                return true;
            }
            case "m_closeToTargetOffsetInBoneSpace":
            case "closeToTargetOffsetInBoneSpace":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_closeToTargetOffsetInBoneSpace = castValue;
                return true;
            }
            case "m_sensorDirectionBS":
            case "sensorDirectionBS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_sensorDirectionBS = castValue;
                return true;
            }
            case "m_eventMode":
            case "eventMode":
            {
                if (value is Havok.hk2014.hkbpTargetRigidBodyModifier.EventModeTRBAM castValue)
                {
                    instance.m_eventMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_eventMode = (Havok.hk2014.hkbpTargetRigidBodyModifier.EventModeTRBAM)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_sensingPropertyKey":
            case "sensingPropertyKey":
            {
                if (value is not uint castValue) return false;
                instance.m_sensingPropertyKey = castValue;
                return true;
            }
            case "m_sensorInWS":
            case "sensorInWS":
            {
                if (value is not bool castValue) return false;
                instance.m_sensorInWS = castValue;
                return true;
            }
            case "m_eventToSend":
            case "eventToSend":
            {
                if (value is not Havok.hk2014.hkbEventProperty castValue) return false;
                instance.m_eventToSend = castValue;
                return true;
            }
            case "m_eventToSendToTarget":
            case "eventToSendToTarget":
            {
                if (value is not Havok.hk2014.hkbEventProperty castValue) return false;
                instance.m_eventToSendToTarget = castValue;
                return true;
            }
            case "m_closeToTargetEvent":
            case "closeToTargetEvent":
            {
                if (value is not Havok.hk2014.hkbEventProperty castValue) return false;
                instance.m_closeToTargetEvent = castValue;
                return true;
            }
            case "m_useVelocityPrediction":
            case "useVelocityPrediction":
            {
                if (value is not bool castValue) return false;
                instance.m_useVelocityPrediction = castValue;
                return true;
            }
            case "m_targetOnlySpheres":
            case "targetOnlySpheres":
            {
                if (value is not bool castValue) return false;
                instance.m_targetOnlySpheres = castValue;
                return true;
            }
            case "m_isCloseToTargetOut":
            case "isCloseToTargetOut":
            {
                if (value is not bool castValue) return false;
                instance.m_isCloseToTargetOut = castValue;
                return true;
            }
            case "m_targetPriority":
            case "targetPriority":
            {
                if (value is not sbyte castValue) return false;
                instance.m_targetPriority = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
