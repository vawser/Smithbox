// Automatically Generated

namespace HKLib.hk2018;

public class CustomLookAtTwistModifier : hkbModifier, hkbVerifiable
{
    public int m_ModifierID;

    public CustomLookAtTwistModifier.MultiRotationAxisType m_rotationAxisType;

    public int m_SensingDummyPoly;

    public List<CustomLookAtTwistModifier.TwistParam> m_twistParam = new();

    public float m_UpLimitAngle;

    public float m_DownLimitAngle;

    public float m_RightLimitAngle;

    public float m_LeftLimitAngle;

    public float m_UpMinimumAngle;

    public float m_DownMinimumAngle;

    public float m_RightMinimumAngle;

    public float m_LeftMinimumAngle;

    public short m_SensingAngle;

    public CustomLookAtTwistModifier.SetAngleMethod m_setAngleMethod;

    public bool m_isAdditive;


    public enum GainState : int
    {
        GainStateTargetGain = 0,
        GainStateOn = 1,
        GainStateOff = 2
    }

    public enum SetAngleMethod : int
    {
        LINEAR = 0,
        RAMPED = 1
    }

    public enum MultiRotationAxisType : int
    {
        AxisXY = 0,
        AxisYX = 1
    }

    public class TwistParam : IHavokObject
    {
        public short m_startBoneIndex;

        public short m_endBoneIndex;

        public float m_targetRotationRate;

        public float m_newTargetGain;

        public float m_onGain;

        public float m_offGain;

    }


}

