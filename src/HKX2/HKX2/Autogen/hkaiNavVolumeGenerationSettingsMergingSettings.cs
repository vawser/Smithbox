using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiNavVolumeGenerationSettingsMergingSettings : IHavokObject
    {
        public virtual uint Signature { get => 3148671488; }
        
        public float m_nodeWeight;
        public float m_edgeWeight;
        public bool m_estimateNewEdges;
        public int m_iterationsStabilizationThreshold;
        public float m_slopeThreshold;
        public int m_maxMergingIterations;
        public int m_randomSeed;
        public float m_multiplier;
        public bool m_useSimpleFirstMergePass;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_nodeWeight = br.ReadSingle();
            m_edgeWeight = br.ReadSingle();
            m_estimateNewEdges = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_iterationsStabilizationThreshold = br.ReadInt32();
            m_slopeThreshold = br.ReadSingle();
            m_maxMergingIterations = br.ReadInt32();
            m_randomSeed = br.ReadInt32();
            m_multiplier = br.ReadSingle();
            m_useSimpleFirstMergePass = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_nodeWeight);
            bw.WriteSingle(m_edgeWeight);
            bw.WriteBoolean(m_estimateNewEdges);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteInt32(m_iterationsStabilizationThreshold);
            bw.WriteSingle(m_slopeThreshold);
            bw.WriteInt32(m_maxMergingIterations);
            bw.WriteInt32(m_randomSeed);
            bw.WriteSingle(m_multiplier);
            bw.WriteBoolean(m_useSimpleFirstMergePass);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
