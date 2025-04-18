using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxMesh : hkReferencedObject
    {
        public override uint Signature { get => 3235576879; }
        
        public List<hkxMeshSection> m_sections;
        public List<hkxMeshUserChannelInfo> m_userChannelInfos;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_sections = des.ReadClassPointerArray<hkxMeshSection>(br);
            m_userChannelInfos = des.ReadClassPointerArray<hkxMeshUserChannelInfo>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkxMeshSection>(bw, m_sections);
            s.WriteClassPointerArray<hkxMeshUserChannelInfo>(bw, m_userChannelInfos);
        }
    }
}
