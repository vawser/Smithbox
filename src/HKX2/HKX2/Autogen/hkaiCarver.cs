using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiCarver : hkReferencedObject
    {
        public override uint Signature { get => 337708303; }
        
        public enum FlagBits
        {
            CARVER_ERODE_EDGES = 1,
        }
        
        public hkaiVolume m_volume;
        public uint m_flags;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_volume = des.ReadClassPointer<hkaiVolume>(br);
            m_flags = br.ReadUInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkaiVolume>(bw, m_volume);
            bw.WriteUInt32(m_flags);
            bw.WriteUInt32(0);
        }
    }
}
