using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaSkeletonLocalFrameOnBone : IHavokObject
    {
        public virtual uint Signature { get => 3910475484; }
        
        public hkLocalFrame m_localFrame;
        public short m_boneIndex;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_localFrame = des.ReadClassPointer<hkLocalFrame>(br);
            m_boneIndex = br.ReadInt16();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassPointer<hkLocalFrame>(bw, m_localFrame);
            bw.WriteInt16(m_boneIndex);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}
