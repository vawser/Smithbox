using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkSkinnedMeshShapeBoneSection : IHavokObject
    {
        public virtual uint Signature { get => 565496218; }
        
        public hkMeshShape m_meshBuffer;
        public ushort m_startBoneSetId;
        public short m_numBoneSets;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_meshBuffer = des.ReadClassPointer<hkMeshShape>(br);
            m_startBoneSetId = br.ReadUInt16();
            m_numBoneSets = br.ReadInt16();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassPointer<hkMeshShape>(bw, m_meshBuffer);
            bw.WriteUInt16(m_startBoneSetId);
            bw.WriteInt16(m_numBoneSets);
            bw.WriteUInt32(0);
        }
    }
}
