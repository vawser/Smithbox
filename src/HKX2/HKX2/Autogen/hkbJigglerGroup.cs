using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbJigglerGroup : hkbBindable
    {
        public override uint Signature { get => 2578532705; }
        
        public hkbBoneIndexArray m_boneIndices;
        public float m_mass;
        public float m_stiffness;
        public float m_damping;
        public float m_maxElongation;
        public float m_maxCompression;
        public bool m_propagateToChildren;
        public bool m_affectSiblings;
        public bool m_rotateBonesForSkinning;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_boneIndices = des.ReadClassPointer<hkbBoneIndexArray>(br);
            m_mass = br.ReadSingle();
            m_stiffness = br.ReadSingle();
            m_damping = br.ReadSingle();
            m_maxElongation = br.ReadSingle();
            m_maxCompression = br.ReadSingle();
            m_propagateToChildren = br.ReadBoolean();
            m_affectSiblings = br.ReadBoolean();
            m_rotateBonesForSkinning = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkbBoneIndexArray>(bw, m_boneIndices);
            bw.WriteSingle(m_mass);
            bw.WriteSingle(m_stiffness);
            bw.WriteSingle(m_damping);
            bw.WriteSingle(m_maxElongation);
            bw.WriteSingle(m_maxCompression);
            bw.WriteBoolean(m_propagateToChildren);
            bw.WriteBoolean(m_affectSiblings);
            bw.WriteBoolean(m_rotateBonesForSkinning);
            bw.WriteUInt64(0);
            bw.WriteByte(0);
        }
    }
}
