using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclVolumeConstraintMxFrameSingleData : IHavokObject
    {
        public virtual uint Signature { get => 2867475255; }
        
        public Vector4 m_frameVector;
        public ushort m_particleIndex;
        public float m_weight;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_frameVector = des.ReadVector4(br);
            m_particleIndex = br.ReadUInt16();
            br.ReadUInt16();
            m_weight = br.ReadSingle();
            br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_frameVector);
            bw.WriteUInt16(m_particleIndex);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_weight);
            bw.WriteUInt64(0);
        }
    }
}
