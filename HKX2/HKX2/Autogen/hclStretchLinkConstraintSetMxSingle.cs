using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclStretchLinkConstraintSetMxSingle : IHavokObject
    {
        public virtual uint Signature { get => 530091824; }
        
        public float m_restLength;
        public float m_stiffness;
        public uint m_particleA;
        public uint m_particleB;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_restLength = br.ReadSingle();
            m_stiffness = br.ReadSingle();
            m_particleA = br.ReadUInt32();
            m_particleB = br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_restLength);
            bw.WriteSingle(m_stiffness);
            bw.WriteUInt32(m_particleA);
            bw.WriteUInt32(m_particleB);
        }
    }
}
