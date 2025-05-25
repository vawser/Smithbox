using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpSerializedTrack1nInfo : IHavokObject
    {
        public virtual uint Signature { get => 4046276825; }
        
        public List<hkpAgent1nSector> m_sectors;
        public List<hkpSerializedSubTrack1nInfo> m_subTracks;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_sectors = des.ReadClassPointerArray<hkpAgent1nSector>(br);
            m_subTracks = des.ReadClassPointerArray<hkpSerializedSubTrack1nInfo>(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassPointerArray<hkpAgent1nSector>(bw, m_sectors);
            s.WriteClassPointerArray<hkpSerializedSubTrack1nInfo>(bw, m_subTracks);
        }
    }
}
