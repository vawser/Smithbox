// Automatically Generated

namespace Havok.hk2019;

public class hknpBody : IHavokObject
{
    public Matrix4x4 m_transform = new();

    public uint m_motionId;

    public hknpCollisionFlags.Enum m_flags;

    public hknpCollisionControl.Enum m_collisionControl;

    public ushort m_timAngle;

    public ushort m_maxTimDistance;

    public ushort m_maxTimDistanceFromRotation;

    public hkAabb24_16_24 m_aabb = new();

    public hknpShape? m_shape;

    public sbyte m_activationPriority;

    public byte m_qualityId;

    public ushort m_materialId;

    public uint m_collisionFilterInfo;

    public hknpBodyId m_id = new();

    public uint m_broadPhaseId;

    public hknpBodyId m_nextAttachedBodyId = new();

    public ushort m_maxContactDistance;

    public ushort m_maxContactDistanceFromRotation;

    public Quaternion m_motionToBodyRotation = new();

    public ulong m_userData;

    public float m_radiusOfComCenteredBoundingSphere;

    public float m_avgSurfaceVelocity;

}
