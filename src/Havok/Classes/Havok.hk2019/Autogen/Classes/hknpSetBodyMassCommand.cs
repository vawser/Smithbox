// Automatically Generated

namespace Havok.hk2019;

public class hknpSetBodyMassCommand : hknpUnaryBodyApiCommand
{
    public hknpSetBodyMassCommand.Mode m_mode;

    public float m_massOrDensity;

    public hknpWorldWriter.UpdateMotionMode m_updateMotionMode;


    public enum Mode : int
    {
        SET_MASS = 0,
        SET_DENSITY = 1
    }

}
