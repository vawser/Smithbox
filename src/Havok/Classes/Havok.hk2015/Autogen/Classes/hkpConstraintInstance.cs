// Automatically Generated

namespace Havok.hk2015;

public class hkpConstraintInstance : hkReferencedObject
{
    public hkpConstraintData? m_data;

    public hkpModifierConstraintAtom? m_constraintModifiers;

    public readonly hkpEntity?[] m_entities = new hkpEntity?[2];

    public byte m_priority;

    public bool m_wantRuntime;

    public byte m_destructionRemapInfo;

    public string? m_name;

    public ulong m_userData;


    public class SmallArraySerializeOverrideType : IHavokObject
    {
        public ushort m_size;

        public ushort m_capacityAndFlags;

    }

}
