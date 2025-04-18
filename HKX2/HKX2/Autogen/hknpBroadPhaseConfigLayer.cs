using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpBroadPhaseConfigLayer : IHavokObject
    {
        public virtual uint Signature { get => 31815071; }
        
        public uint m_collideWithLayerMask;
        public bool m_isVolatile;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_collideWithLayerMask = br.ReadUInt32();
            m_isVolatile = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt32(m_collideWithLayerMask);
            bw.WriteBoolean(m_isVolatile);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
