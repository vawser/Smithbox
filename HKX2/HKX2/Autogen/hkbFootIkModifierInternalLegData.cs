using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbFootIkModifierInternalLegData : IHavokObject
    {
        public virtual uint Signature { get => 3855234679; }
        
        public Vector4 m_groundPosition;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_groundPosition = des.ReadVector4(br);
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_groundPosition);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
