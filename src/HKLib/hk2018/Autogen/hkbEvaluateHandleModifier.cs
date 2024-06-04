// Automatically Generated

namespace HKLib.hk2018;

public class hkbEvaluateHandleModifier : hkbModifier, hkbVerifiable
{
    public hkbHandle? m_handle;

    public Vector4 m_handlePositionOut = new();

    public Quaternion m_handleRotationOut = new();

    public bool m_isValidOut;

    public float m_extrapolationTimeStep;

    public float m_handleChangeSpeed;

    public hkbEvaluateHandleModifier.HandleChangeMode m_handleChangeMode;


    public enum HandleChangeMode : int
    {
        HANDLE_CHANGE_MODE_ABRUPT = 0,
        HANDLE_CHANGE_MODE_CONSTANT_VELOCITY = 1
    }

}

