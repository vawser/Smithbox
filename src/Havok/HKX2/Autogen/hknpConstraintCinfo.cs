using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpConstraintCinfo : IHavokObject
    {
        public virtual uint Signature { get => 1743427693; }
        
        public hkpConstraintData m_constraintData;
        public uint m_bodyA;
        public uint m_bodyB;
        public byte m_flags;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_constraintData = des.ReadClassPointer<hkpConstraintData>(br);
            m_bodyA = br.ReadUInt32();
            m_bodyB = br.ReadUInt32();
            m_flags = br.ReadByte();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassPointer<hkpConstraintData>(bw, m_constraintData);
            bw.WriteUInt32(m_bodyA);
            bw.WriteUInt32(m_bodyB);
            bw.WriteByte(m_flags);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
