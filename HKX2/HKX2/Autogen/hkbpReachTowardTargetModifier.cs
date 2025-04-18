using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbpReachTowardTargetModifier : hkbModifier
    {
        public override uint Signature { get => 969236214; }
        
        public enum FadeState
        {
            FADE_IN = 0,
            FADE_OUT = 1,
        }
        
        public hkbpReachTowardTargetModifierHand m_leftHand;
        public hkbpReachTowardTargetModifierHand m_rightHand;
        public hkbpTarget m_targetIn;
        public float m_distanceBetweenHands;
        public float m_reachDistance;
        public float m_fadeInGainSpeed;
        public float m_fadeOutGainSpeed;
        public float m_fadeOutDuration;
        public float m_targetChangeSpeed;
        public bool m_holdTarget;
        public bool m_reachPastTarget;
        public bool m_giveUpIfNoTarget;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_leftHand = new hkbpReachTowardTargetModifierHand();
            m_leftHand.Read(des, br);
            m_rightHand = new hkbpReachTowardTargetModifierHand();
            m_rightHand.Read(des, br);
            m_targetIn = des.ReadClassPointer<hkbpTarget>(br);
            m_distanceBetweenHands = br.ReadSingle();
            m_reachDistance = br.ReadSingle();
            m_fadeInGainSpeed = br.ReadSingle();
            m_fadeOutGainSpeed = br.ReadSingle();
            m_fadeOutDuration = br.ReadSingle();
            m_targetChangeSpeed = br.ReadSingle();
            m_holdTarget = br.ReadBoolean();
            m_reachPastTarget = br.ReadBoolean();
            m_giveUpIfNoTarget = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_leftHand.Write(s, bw);
            m_rightHand.Write(s, bw);
            s.WriteClassPointer<hkbpTarget>(bw, m_targetIn);
            bw.WriteSingle(m_distanceBetweenHands);
            bw.WriteSingle(m_reachDistance);
            bw.WriteSingle(m_fadeInGainSpeed);
            bw.WriteSingle(m_fadeOutGainSpeed);
            bw.WriteSingle(m_fadeOutDuration);
            bw.WriteSingle(m_targetChangeSpeed);
            bw.WriteBoolean(m_holdTarget);
            bw.WriteBoolean(m_reachPastTarget);
            bw.WriteBoolean(m_giveUpIfNoTarget);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteByte(0);
        }
    }
}
