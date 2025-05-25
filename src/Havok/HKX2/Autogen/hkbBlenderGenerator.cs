using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum BlenderFlags
    {
        FLAG_SYNC = 1,
        FLAG_SMOOTH_GENERATOR_WEIGHTS = 4,
        FLAG_DONT_DEACTIVATE_CHILDREN_WITH_ZERO_WEIGHTS = 8,
        FLAG_PARAMETRIC_BLEND = 16,
        FLAG_IS_PARAMETRIC_BLEND_CYCLIC = 32,
        FLAG_FORCE_DENSE_POSE = 64,
        FLAG_BLEND_MOTION_OF_ADDITIVE_ANIMATIONS = 128,
        FLAG_USE_VELOCITY_SYNCHRONIZATION = 256,
    }
    
    public partial class hkbBlenderGenerator : hkbGenerator
    {
        public override uint Signature { get => 3460677768; }
        
        public float m_referencePoseWeightThreshold;
        public float m_blendParameter;
        public float m_minCyclicBlendParameter;
        public float m_maxCyclicBlendParameter;
        public short m_indexOfSyncMasterChild;
        public short m_flags;
        public bool m_subtractLastChild;
        public List<hkbBlenderGeneratorChild> m_children;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_referencePoseWeightThreshold = br.ReadSingle();
            m_blendParameter = br.ReadSingle();
            m_minCyclicBlendParameter = br.ReadSingle();
            m_maxCyclicBlendParameter = br.ReadSingle();
            m_indexOfSyncMasterChild = br.ReadInt16();
            m_flags = br.ReadInt16();
            m_subtractLastChild = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_children = des.ReadClassPointerArray<hkbBlenderGeneratorChild>(br);
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_referencePoseWeightThreshold);
            bw.WriteSingle(m_blendParameter);
            bw.WriteSingle(m_minCyclicBlendParameter);
            bw.WriteSingle(m_maxCyclicBlendParameter);
            bw.WriteInt16(m_indexOfSyncMasterChild);
            bw.WriteInt16(m_flags);
            bw.WriteBoolean(m_subtractLastChild);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            s.WriteClassPointerArray<hkbBlenderGeneratorChild>(bw, m_children);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
