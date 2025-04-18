using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkSkinnedMeshShapeBoneSet : IHavokObject
    {
        public virtual uint Signature { get => 2858842445; }
        
        public ushort m_boneBufferOffset;
        public ushort m_numBones;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_boneBufferOffset = br.ReadUInt16();
            m_numBones = br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt16(m_boneBufferOffset);
            bw.WriteUInt16(m_numBones);
        }
    }
}
