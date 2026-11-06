// Automatically Generated

namespace Havok.hk2014;

public class hknpBody : IHavokObject
{
    public Matrix4x4 m_transform = new();

    public int m_flags;

    public uint m_collisionFilterInfo;

    public hknpShape? m_shape;

    public hkAabb16 m_aabb = new();

    public uint m_id;

    public uint m_nextAttachedBodyId;

    public uint m_motionId;

    public uint m_broadPhaseId;

    public ushort m_materialId;

    public byte m_qualityId;

    public byte m_timAngle;

    public ushort m_maxTimDistance;

    public ushort m_maxContactDistance;

    public uint m_indexIntoActiveListOrDeactivatedIslandId;

    public float m_radiusOfComCenteredBoundingSphere;

    public hknpBody.SpuFlagsEnum m_spuFlags;

    public byte m_shapeSizeDiv16;

    public readonly short[] m_motionToBodyRotation = new short[4];

    public ulong m_userData;


    [Flags]
    public enum SpuFlagsEnum : int
    {
        FORCE_NARROW_PHASE_PPU = 1
    }

}
