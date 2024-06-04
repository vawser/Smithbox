// Automatically Generated

namespace HKLib.hk2018;

public class hkbDampingModifier : hkbModifier, hkbVerifiable
{
    public float m_kP;

    public float m_kI;

    public float m_kD;

    public bool m_enableScalarDamping;

    public bool m_enableVectorDamping;

    public float m_rawValue;

    public float m_dampedValue;

    public Vector4 m_rawVector = new();

    public Vector4 m_dampedVector = new();

    public Vector4 m_vecErrorSum = new();

    public Vector4 m_vecPreviousError = new();

    public float m_errorSum;

    public float m_previousError;

}

