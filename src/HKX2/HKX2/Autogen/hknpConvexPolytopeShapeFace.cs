using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpConvexPolytopeShapeFace : IHavokObject
    {
        public virtual uint Signature { get => 4089468224; }
        
        public ushort m_firstIndex;
        public byte m_numIndices;
        public byte m_minHalfAngle;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_firstIndex = br.ReadUInt16();
            m_numIndices = br.ReadByte();
            m_minHalfAngle = br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt16(m_firstIndex);
            bw.WriteByte(m_numIndices);
            bw.WriteByte(m_minHalfAngle);
        }
    }
}
