// Automatically Generated

namespace HKLib.hk2018;

public class hknpBodyCinfo : IHavokObject
{
    public hknpShape? m_shape;

    public int m_flags;

    public short m_collisionCntrl;

    public uint m_collisionFilterInfo;

    public ushort m_materialId;

    public byte m_qualityId;

    public string? m_name;

    public ulong m_userData;

    public byte m_motionType;

    public Vector4 m_position = new();

    public Quaternion m_orientation = new();

    public Vector4 m_linearVelocity = new();

    public Vector4 m_angularVelocity = new();

    public float m_mass;

    public hknpRefMassDistribution? m_massDistribution;

    public hknpRefDragProperties? m_dragProperties;

    public ushort m_motionPropertiesId;

    public hknpBodyId m_desiredBodyId = new();

    public uint m_motionId;

    public float m_collisionLookAheadDistance;

    public hkLocalFrame? m_localFrame;

    public sbyte m_activationPriority;

}

