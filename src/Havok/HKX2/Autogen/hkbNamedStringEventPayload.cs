using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbNamedStringEventPayload : hkbNamedEventPayload
    {
        public override uint Signature { get => 890430323; }
        
        public string m_data;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_data = des.ReadStringPointer(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_data);
        }
    }
}
