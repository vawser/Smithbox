using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbClipTriggerArray : hkReferencedObject
    {
        public override uint Signature { get => 4149726566; }
        
        public List<hkbClipTrigger> m_triggers;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_triggers = des.ReadClassArray<hkbClipTrigger>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkbClipTrigger>(bw, m_triggers);
        }
    }
}
