// Automatically Generated

namespace HKLib.hk2018;

public class hknpManifoldEvent : hknpBinaryBodyEvent
{
    public hknpManifoldEvent.Status m_status;

    public byte m_isFullCast;

    public readonly hkHandle<uint>[] m_shapeKeys = new hkHandle<uint>[2];

    public hkcdManifold4 m_manifold = new();


    public enum Status : int
    {
        STATUS_CREATED = 1,
        STATUS_UPDATED = 2,
        STATUS_REUSED = 4,
        STATUS_DESTROYED = 8,
        STATUS_ALL = 15
    }

}

