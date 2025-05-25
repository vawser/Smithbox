using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdStaticTreeCodec3Axis : IHavokObject
    {
        public virtual uint Signature { get => 1255292721; }
        
        public byte m_xyz_0;
        public byte m_xyz_1;
        public byte m_xyz_2;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_xyz_0 = br.ReadByte();
            m_xyz_1 = br.ReadByte();
            m_xyz_2 = br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteByte(m_xyz_0);
            bw.WriteByte(m_xyz_1);
            bw.WriteByte(m_xyz_2);
        }
    }
}
