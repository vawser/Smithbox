// Automatically Generated

namespace HKLib.hk2018;

public class hkbTwistModifier : hkbModifier, hkbVerifiable
{
    public Vector4 m_axisOfRotation = new();

    public float m_twistAngle;

    public short m_startBoneIndex;

    public short m_endBoneIndex;

    public hkbTwistModifier.SetAngleMethod m_setAngleMethod;

    public hkbTwistModifier.RotationAxisCoordinates m_rotationAxisCoordinates;

    public bool m_isAdditive;


    public enum RotationAxisCoordinates : int
    {
        ROTATION_AXIS_IN_MODEL_COORDINATES = 0,
        ROTATION_AXIS_IN_PARENT_COORDINATES = 1,
        ROTATION_AXIS_IN_LOCAL_COORDINATES = 2
    }

    public enum SetAngleMethod : int
    {
        LINEAR = 0,
        RAMPED = 1
    }

}

