// Automatically Generated

namespace HKLib.hk2018;

public class hkbSenseHandleModifier : hkbModifier, hkbVerifiable
{
    public Vector4 m_sensorLocalOffset = new();

    public List<hkbSenseHandleModifier.Range> m_ranges = new();

    public hkbHandle? m_handleOut;

    public hkbHandle? m_handleIn;

    public string? m_localFrameName;

    public string? m_sensorLocalFrameName;

    public float m_minDistance;

    public float m_maxDistance;

    public float m_distanceOut;

    public uint m_collisionFilterInfo;

    public short m_sensorRagdollBoneIndex;

    public short m_sensorAnimationBoneIndex;

    public hkbSenseHandleModifier.SensingMode m_sensingMode;

    public bool m_extrapolateSensorPosition;

    public bool m_keepFirstSensedHandle;

    public bool m_foundHandleOut;


    public enum SensingMode : int
    {
        SENSE_IN_NEARBY_RIGID_BODIES = 0,
        SENSE_IN_RIGID_BODIES_OUTSIDE_THIS_CHARACTER = 1,
        SENSE_IN_OTHER_CHARACTER_RIGID_BODIES = 2,
        SENSE_IN_THIS_CHARACTER_RIGID_BODIES = 3,
        SENSE_IN_GIVEN_CHARACTER_RIGID_BODIES = 4,
        SENSE_IN_GIVEN_RIGID_BODY = 5,
        SENSE_IN_OTHER_CHARACTER_SKELETON = 6,
        SENSE_IN_THIS_CHARACTER_SKELETON = 7,
        SENSE_IN_GIVEN_CHARACTER_SKELETON = 8,
        SENSE_IN_GIVEN_LOCAL_FRAME_GROUP = 9
    }

    public class Range : IHavokObject
    {
        public hkbEventProperty m_event = new();

        public float m_minDistance;

        public float m_maxDistance;

        public bool m_ignoreHandle;

    }


}

