using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclSkinOperatorBoneInfluence : IHavokObject
    {
        public virtual uint Signature { get => 1963099810; }
        
        public byte m_boneIndex;
        public byte m_weight;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_boneIndex = br.ReadByte();
            m_weight = br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteByte(m_boneIndex);
            bw.WriteByte(m_weight);
        }
    }
}
