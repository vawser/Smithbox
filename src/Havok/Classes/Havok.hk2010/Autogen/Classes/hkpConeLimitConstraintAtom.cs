// Automatically Generated

namespace Havok.hk2010;

public class hkpConeLimitConstraintAtom : hkpConstraintAtom
{
    public byte m_isEnabled;

    public byte m_twistAxisInA;

    public byte m_refAxisInB;

    public hkpConeLimitConstraintAtom.MeasurementMode m_angleMeasurementMode;

    public byte m_memOffsetToAngleOffset;

    public float m_minAngle;

    public float m_maxAngle;

    public float m_angularLimitsTauFactor;


    public enum MeasurementMode : int
    {
        ZERO_WHEN_VECTORS_ALIGNED = 0,
        ZERO_WHEN_VECTORS_PERPENDICULAR = 1
    }

}
