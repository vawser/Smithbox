using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ReachMode
    {
        REACH_MODE_TERRAIN = 0,
        REACH_MODE_WORLD_POSITION = 1,
        REACH_MODE_MODEL_POSITION = 2,
        REACH_MODE_BONE_POSITION = 3,
    }
    
    public partial class hkbpReachModifier : hkbModifier
    {
        public override uint Signature { get => 742004561; }
        
        public List<hkbpReachModifierHand> m_hands;
        public float m_newTargetGain;
        public float m_noTargetGain;
        public float m_targetGain;
        public float m_fadeOutDuration;
        public int m_raycastLayer;
        public uint m_sensingPropertyKey;
        public ReachMode m_reachMode;
        public bool m_ignoreMySystemGroup;
        public float m_extrapolate;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_hands = des.ReadClassArray<hkbpReachModifierHand>(br);
            m_newTargetGain = br.ReadSingle();
            m_noTargetGain = br.ReadSingle();
            m_targetGain = br.ReadSingle();
            m_fadeOutDuration = br.ReadSingle();
            m_raycastLayer = br.ReadInt32();
            m_sensingPropertyKey = br.ReadUInt32();
            m_reachMode = (ReachMode)br.ReadSByte();
            m_ignoreMySystemGroup = br.ReadBoolean();
            br.ReadUInt16();
            m_extrapolate = br.ReadSingle();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkbpReachModifierHand>(bw, m_hands);
            bw.WriteSingle(m_newTargetGain);
            bw.WriteSingle(m_noTargetGain);
            bw.WriteSingle(m_targetGain);
            bw.WriteSingle(m_fadeOutDuration);
            bw.WriteInt32(m_raycastLayer);
            bw.WriteUInt32(m_sensingPropertyKey);
            bw.WriteSByte((sbyte)m_reachMode);
            bw.WriteBoolean(m_ignoreMySystemGroup);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_extrapolate);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
