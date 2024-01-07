using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbpCatchFallModifier : hkbModifier
    {
        public override uint Signature { get => 2837857129; }
        
        public enum FadeState
        {
            FADE_IN = 0,
            FADE_OUT = 1,
        }
        
        public Vector4 m_directionOfFallForwardLS;
        public Vector4 m_directionOfFallRightLS;
        public Vector4 m_directionOfFallUpLS;
        public hkbBoneIndexArray m_spineIndices;
        public hkbpCatchFallModifierHand m_leftHand;
        public hkbpCatchFallModifierHand m_rightHand;
        public hkbEventProperty m_catchFallDoneEvent;
        public float m_spreadHandsMultiplier;
        public float m_radarRange;
        public float m_previousTargetBlendWeight;
        public float m_handsBendDistance;
        public float m_maxReachDistanceForward;
        public float m_maxReachDistanceBackward;
        public float m_fadeInReachGainSpeed;
        public float m_fadeOutReachGainSpeed;
        public float m_fadeOutDuration;
        public float m_fadeInTwistSpeed;
        public float m_fadeOutTwistSpeed;
        public short m_raycastLayer;
        public short m_velocityRagdollBoneIndex;
        public short m_directionOfFallRagdollBoneIndex;
        public bool m_orientHands;
        public Vector4 m_catchFallPosInBS_0;
        public Vector4 m_catchFallPosInBS_1;
        public float m_currentReachGain_0;
        public float m_currentReachGain_1;
        public float m_timeSinceLastModify;
        public float m_currentTwistGain;
        public short m_currentTwistDirection;
        public bool m_catchFallPosIsValid_0;
        public bool m_catchFallPosIsValid_1;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_directionOfFallForwardLS = des.ReadVector4(br);
            m_directionOfFallRightLS = des.ReadVector4(br);
            m_directionOfFallUpLS = des.ReadVector4(br);
            m_spineIndices = des.ReadClassPointer<hkbBoneIndexArray>(br);
            m_leftHand = new hkbpCatchFallModifierHand();
            m_leftHand.Read(des, br);
            m_rightHand = new hkbpCatchFallModifierHand();
            m_rightHand.Read(des, br);
            br.ReadUInt32();
            m_catchFallDoneEvent = new hkbEventProperty();
            m_catchFallDoneEvent.Read(des, br);
            m_spreadHandsMultiplier = br.ReadSingle();
            m_radarRange = br.ReadSingle();
            m_previousTargetBlendWeight = br.ReadSingle();
            m_handsBendDistance = br.ReadSingle();
            m_maxReachDistanceForward = br.ReadSingle();
            m_maxReachDistanceBackward = br.ReadSingle();
            m_fadeInReachGainSpeed = br.ReadSingle();
            m_fadeOutReachGainSpeed = br.ReadSingle();
            m_fadeOutDuration = br.ReadSingle();
            m_fadeInTwistSpeed = br.ReadSingle();
            m_fadeOutTwistSpeed = br.ReadSingle();
            m_raycastLayer = br.ReadInt16();
            m_velocityRagdollBoneIndex = br.ReadInt16();
            m_directionOfFallRagdollBoneIndex = br.ReadInt16();
            m_orientHands = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadByte();
            m_catchFallPosInBS_0 = des.ReadVector4(br);
            m_catchFallPosInBS_1 = des.ReadVector4(br);
            m_currentReachGain_0 = br.ReadSingle();
            m_currentReachGain_1 = br.ReadSingle();
            m_timeSinceLastModify = br.ReadSingle();
            m_currentTwistGain = br.ReadSingle();
            m_currentTwistDirection = br.ReadInt16();
            m_catchFallPosIsValid_0 = br.ReadBoolean();
            m_catchFallPosIsValid_1 = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_directionOfFallForwardLS);
            s.WriteVector4(bw, m_directionOfFallRightLS);
            s.WriteVector4(bw, m_directionOfFallUpLS);
            s.WriteClassPointer<hkbBoneIndexArray>(bw, m_spineIndices);
            m_leftHand.Write(s, bw);
            m_rightHand.Write(s, bw);
            bw.WriteUInt32(0);
            m_catchFallDoneEvent.Write(s, bw);
            bw.WriteSingle(m_spreadHandsMultiplier);
            bw.WriteSingle(m_radarRange);
            bw.WriteSingle(m_previousTargetBlendWeight);
            bw.WriteSingle(m_handsBendDistance);
            bw.WriteSingle(m_maxReachDistanceForward);
            bw.WriteSingle(m_maxReachDistanceBackward);
            bw.WriteSingle(m_fadeInReachGainSpeed);
            bw.WriteSingle(m_fadeOutReachGainSpeed);
            bw.WriteSingle(m_fadeOutDuration);
            bw.WriteSingle(m_fadeInTwistSpeed);
            bw.WriteSingle(m_fadeOutTwistSpeed);
            bw.WriteInt16(m_raycastLayer);
            bw.WriteInt16(m_velocityRagdollBoneIndex);
            bw.WriteInt16(m_directionOfFallRagdollBoneIndex);
            bw.WriteBoolean(m_orientHands);
            bw.WriteUInt32(0);
            bw.WriteByte(0);
            s.WriteVector4(bw, m_catchFallPosInBS_0);
            s.WriteVector4(bw, m_catchFallPosInBS_1);
            bw.WriteSingle(m_currentReachGain_0);
            bw.WriteSingle(m_currentReachGain_1);
            bw.WriteSingle(m_timeSinceLastModify);
            bw.WriteSingle(m_currentTwistGain);
            bw.WriteInt16(m_currentTwistDirection);
            bw.WriteBoolean(m_catchFallPosIsValid_0);
            bw.WriteBoolean(m_catchFallPosIsValid_1);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
