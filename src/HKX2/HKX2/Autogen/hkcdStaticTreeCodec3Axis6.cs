using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdStaticTreeCodec3Axis6 : hkcdStaticTreeCodec3Axis
    {
        public override uint Signature { get => 1453022594; }
        
        public byte m_hiData;
        public ushort m_loData;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_hiData = br.ReadByte();
            m_loData = br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteByte(m_hiData);
            bw.WriteUInt16(m_loData);
        }
    }
}
