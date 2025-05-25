// Automatically Generated

namespace HKLib.hk2018;

public class hkbHandIkControlData : IHavokObject
{
    public Vector4 m_targetPosition = new();

    public Quaternion m_targetRotation = new();

    public Vector4 m_targetNormal = new();

    public hkbHandle? m_targetHandle;

    public float m_transformOnFraction;

    public float m_normalOnFraction;

    public float m_fadeInDuration;

    public float m_fadeOutDuration;

    public float m_extrapolationTimeStep;

    public float m_handleChangeSpeed;

    public hkbHandIkControlData.HandleChangeMode m_handleChangeMode;

    public bool m_fixUp;


    public enum HandleChangeMode : int
    {
        HANDLE_CHANGE_MODE_ABRUPT = 0,
        HANDLE_CHANGE_MODE_CONSTANT_VELOCITY = 1
    }

}

