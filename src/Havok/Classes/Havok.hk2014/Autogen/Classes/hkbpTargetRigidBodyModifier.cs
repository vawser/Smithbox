// Automatically Generated

namespace Havok.hk2014;

public class hkbpTargetRigidBodyModifier : hkbModifier
{
    public hkbpTarget? m_targetOut;

    public hkbpTargetRigidBodyModifier.TargetMode m_targetMode;

    public int m_sensingLayer;

    public bool m_targetOnlyOnce;

    public bool m_ignoreMySystemGroup;

    public float m_maxTargetDistance;

    public float m_maxTargetHeightAboveSensor;

    public float m_closeToTargetDistanceThreshold;

    public hkbpTargetRigidBodyModifier.ComputeTargetAngleMode m_targetAngleMode;

    public hkbpTargetRigidBodyModifier.ComputeTargetDistanceMode m_targetDistanceMode;

    public float m_maxAngleToTarget;

    public short m_sensorRagdollBoneIndex;

    public short m_sensorAnimationBoneIndex;

    public short m_closeToTargetRagdollBoneIndex;

    public short m_closeToTargetAnimationBoneIndex;

    public Vector4 m_sensorOffsetInBoneSpace = new();

    public Vector4 m_closeToTargetOffsetInBoneSpace = new();

    public Vector4 m_sensorDirectionBS = new();

    public hkbpTargetRigidBodyModifier.EventModeTRBAM m_eventMode;

    public uint m_sensingPropertyKey;

    public bool m_sensorInWS;

    public hkbEventProperty m_eventToSend = new();

    public hkbEventProperty m_eventToSendToTarget = new();

    public hkbEventProperty m_closeToTargetEvent = new();

    public bool m_useVelocityPrediction;

    public bool m_targetOnlySpheres;

    public bool m_isCloseToTargetOut;

    public sbyte m_targetPriority;


    public enum TargetMode : int
    {
        TARGET_MODE_CONE_AROUND_CHARACTER_FORWARD = 0,
        TARGET_MODE_CONE_AROUND_LOCAL_AXIS = 1,
        TARGET_MODE_RAYCAST_ALONG_CHARACTER_FORWARD = 2,
        TARGET_MODE_RAYCAST_ALONG_LOCAL_AXIS = 3,
        TARGET_MODE_ANY_DIRECTION = 4
    }

    public enum ComputeTargetAngleMode : int
    {
        COMPUTE_ANGLE_USING_TARGET_COM = 0,
        COMPUTE_ANGLE_USING_TARGET_CONTACT_POINT = 1
    }

    public enum ComputeTargetDistanceMode : int
    {
        COMPUTE_DISTANCE_USING_TARGET_POSITION = 0,
        COMPUTE_DISTANCE_USING_TARGET_CONTACT_POINT = 1
    }

    public enum EventModeTRBAM : int
    {
        EVENT_MODE_DO_NOT_SEND = 0,
        EVENT_MODE_SEND_ONCE = 1,
        EVENT_MODE_RESEND = 2
    }

}
