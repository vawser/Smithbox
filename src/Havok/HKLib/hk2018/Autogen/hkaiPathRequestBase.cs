// Automatically Generated

namespace HKLib.hk2018;

public class hkaiPathRequestBase : hkReferencedObject
{
    public hkHandle<byte> m_queueId = new();

    public int m_priority;

    public hkAtomic.Variable<hkaiPathRequestBase.RequestState> m_requestState = new();


    public enum RequestState : byte
    {
        PENDING = 0,
        PROCESSING = 1,
        FINISHED = 2,
        CANCELED = 3
    }

}

