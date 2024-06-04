// Automatically Generated

namespace HKLib.hk2018;

public class hknpContactImpulseEvent : hknpContactSolverEvent
{
    public hknpContactImpulseEvent.Status m_status;

    public float m_frictionFactor;

    public readonly float[] m_contactImpulses = new float[4];


    public enum Status : int
    {
        STATUS_NONE = 0,
        STATUS_STARTED = 1,
        STATUS_FINISHED = 2,
        STATUS_CONTINUED = 3,
        STATUS_IMMEDIATE = 4
    }

}

