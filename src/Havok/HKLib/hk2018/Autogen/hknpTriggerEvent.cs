// Automatically Generated

namespace HKLib.hk2018;

public class hknpTriggerEvent : hknpBinaryBodyEvent
{
    public hknpMaterial.TriggerType m_type;

    public hknpTriggerEvent.Status m_status;

    public readonly hkHandle<uint>[] m_shapeKeys = new hkHandle<uint>[2];


    public enum Status : int
    {
        STATUS_ENTERED = 0,
        STATUS_EXITED = 1,
        STATUS_UPDATED = 2
    }

}

