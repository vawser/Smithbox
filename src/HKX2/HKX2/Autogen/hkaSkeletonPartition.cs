using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaSkeletonPartition : IHavokObject
    {
        public virtual uint Signature { get => 2089708117; }
        
        public string m_name;
        public short m_startBoneIndex;
        public short m_numBones;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_name = des.ReadStringPointer(br);
            m_startBoneIndex = br.ReadInt16();
            m_numBones = br.ReadInt16();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_name);
            bw.WriteInt16(m_startBoneIndex);
            bw.WriteInt16(m_numBones);
            bw.WriteUInt32(0);
        }
    }
}
