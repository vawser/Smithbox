using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiSilhouetteRecorderVolumeUnloadedEvent : hkaiSilhouetteRecorderReplayEvent
    {
        public override uint Signature { get => 2391714391; }
        
        public uint m_sectionUid;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_sectionUid = br.ReadUInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt32(m_sectionUid);
            bw.WriteUInt32(0);
        }
    }
}
