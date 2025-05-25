using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbNamedRealEventPayload : hkbNamedEventPayload
    {
        public override uint Signature { get => 898628493; }
        
        public float m_data;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_data = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_data);
            bw.WriteUInt32(0);
        }
    }
}
