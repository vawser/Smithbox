using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaQuantizedAnimation : hkaAnimation
    {
        public override uint Signature { get => 827906510; }
        
        public List<byte> m_data;
        public uint m_endian;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_data = des.ReadByteArray(br);
            m_endian = br.ReadUInt32();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteByteArray(bw, m_data);
            bw.WriteUInt32(m_endian);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
