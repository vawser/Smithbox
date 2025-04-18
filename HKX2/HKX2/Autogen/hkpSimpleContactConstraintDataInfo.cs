using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpSimpleContactConstraintDataInfo : IHavokObject
    {
        public virtual uint Signature { get => 666540059; }
        
        public ushort m_flags;
        public ushort m_biNormalAxis;
        public short m_rollingFrictionMultiplier;
        public short m_internalData1;
        public short m_rhsRolling_0;
        public short m_rhsRolling_1;
        public float m_contactRadius;
        public float m_data_0;
        public float m_data_1;
        public float m_data_2;
        public float m_data_3;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_flags = br.ReadUInt16();
            m_biNormalAxis = br.ReadUInt16();
            m_rollingFrictionMultiplier = br.ReadInt16();
            m_internalData1 = br.ReadInt16();
            m_rhsRolling_0 = br.ReadInt16();
            m_rhsRolling_1 = br.ReadInt16();
            m_contactRadius = br.ReadSingle();
            m_data_0 = br.ReadSingle();
            m_data_1 = br.ReadSingle();
            m_data_2 = br.ReadSingle();
            m_data_3 = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt16(m_flags);
            bw.WriteUInt16(m_biNormalAxis);
            bw.WriteInt16(m_rollingFrictionMultiplier);
            bw.WriteInt16(m_internalData1);
            bw.WriteInt16(m_rhsRolling_0);
            bw.WriteInt16(m_rhsRolling_1);
            bw.WriteSingle(m_contactRadius);
            bw.WriteSingle(m_data_0);
            bw.WriteSingle(m_data_1);
            bw.WriteSingle(m_data_2);
            bw.WriteSingle(m_data_3);
        }
    }
}
