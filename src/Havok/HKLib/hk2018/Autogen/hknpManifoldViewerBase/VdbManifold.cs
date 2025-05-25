// Automatically Generated

namespace HKLib.hk2018.hknpManifoldViewerBase;

public class VdbManifold : IHavokObject
{
    public readonly hknpBodyId[] m_bodyIds = new hknpBodyId[2];

    public readonly hkHandle<uint>[] m_shapeKeys = new hkHandle<uint>[2];

    public byte m_lodInfo;

    public hknpManifoldViewerBase.VdbManifold.StatusEnum m_status;

    public hkcdManifold4 m_manifold = new();

    public Vector4 m_impulses = new();


    [Flags]
    public enum StatusEnum : int
    {
        STATUS_DISABLED = 1,
        STATUS_REUSED = 2,
        STATUS_TIME_STEALING = 4,
        STATUS_APPLIED_IMPULSE = 8,
        STATUS_FULL_CAST = 16,
        STATUS_FIRST_PROCESS = 32,
        STATUS_DESTROYED = 64,
        STATUS_AFTER_WELDING = 128,
        STATUS_TRIANGLE_WELDED = 256,
        STATUS_MOTION_WELDED = 512,
        STATUS_NEIGHBOR_WELDED = 1024,
        STATUS_AFTER_PROCESSING = 2048,
        STATUS_STEP_MARKER = 4096
    }

}

