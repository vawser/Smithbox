using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbEventPayloadList : hkbEventPayload
    {
        public override uint Signature { get => 4015043109; }
        
        public List<hkbEventPayload> m_payloads;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_payloads = des.ReadClassPointerArray<hkbEventPayload>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkbEventPayload>(bw, m_payloads);
        }
    }
}
