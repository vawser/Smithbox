using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxMaterialTextureStage : IHavokObject
    {
        public virtual uint Signature { get => 3688529851; }
        
        public hkReferencedObject m_texture;
        public TextureType m_usageHint;
        public int m_tcoordChannel;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_texture = des.ReadClassPointer<hkReferencedObject>(br);
            m_usageHint = (TextureType)br.ReadInt32();
            m_tcoordChannel = br.ReadInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassPointer<hkReferencedObject>(bw, m_texture);
            bw.WriteInt32((int)m_usageHint);
            bw.WriteInt32(m_tcoordChannel);
        }
    }
}
