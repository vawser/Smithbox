using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkGeometryTriangle : IHavokObject
    {
        public virtual uint Signature { get => 2525450555; }
        
        public int m_a;
        public int m_b;
        public int m_c;
        public int m_material;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_a = br.ReadInt32();
            m_b = br.ReadInt32();
            m_c = br.ReadInt32();
            m_material = br.ReadInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt32(m_a);
            bw.WriteInt32(m_b);
            bw.WriteInt32(m_c);
            bw.WriteInt32(m_material);
        }
    }
}
