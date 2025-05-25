using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdStaticTreeCodec3Axis5 : hkcdStaticTreeCodec3Axis
    {
        public override uint Signature { get => 316044371; }
        
        public byte m_hiData;
        public byte m_loData;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_hiData = br.ReadByte();
            m_loData = br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteByte(m_hiData);
            bw.WriteByte(m_loData);
        }
    }
}
