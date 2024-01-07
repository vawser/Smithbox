using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum TargetModeMBTT
    {
        TARGET_POSITION = 0,
        TARGET_COM = 1,
        TARGET_CONTACT_POINT = 2,
        TARGET_SHAPE_CENTROID = 3,
    }
    
    public enum AlignModeBits
    {
        ALIGN_AXES = 1,
        ALIGN_BONE_AXIS_WITH_CONTACT_NORMAL = 2,
        ALIGN_WITH_CHARACTER_FORWARD = 4,
    }
    
    public partial class hkbpMoveBoneTowardTargetModifier : hkbModifier
    {
        public override uint Signature { get => 4205169225; }
        
        public hkbpTarget m_targetIn;
        public Vector4 m_offsetInBoneSpace;
        public Vector4 m_alignAxisBS;
        public Vector4 m_targetAlignAxisTS;
        public Vector4 m_alignWithCharacterForwardBS;
        public Vector4 m_currentBonePositionOut;
        public Quaternion m_currentBoneRotationOut;
        public hkbEventProperty m_eventToSendWhenTargetReached;
        public hkbGenerator m_childGenerator;
        public float m_duration;
        public short m_ragdollBoneIndex;
        public short m_animationBoneIndex;
        public TargetModeMBTT m_targetMode;
        public sbyte m_alignMode;
        public bool m_useVelocityPrediction;
        public bool m_affectOrientation;
        public bool m_currentBoneIsValidOut;
        public Vector4 m_finalAnimBonePositionMS;
        public Vector4 m_initialAnimBonePositionMS;
        public Quaternion m_finalAnimBoneOrientationMS;
        public Quaternion m_animationFromRagdoll;
        public Matrix4x4 m_totalMotion;
        public Matrix4x4 m_accumulatedMotion;
        public bool m_useAnimationData;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_targetIn = des.ReadClassPointer<hkbpTarget>(br);
            m_offsetInBoneSpace = des.ReadVector4(br);
            m_alignAxisBS = des.ReadVector4(br);
            m_targetAlignAxisTS = des.ReadVector4(br);
            m_alignWithCharacterForwardBS = des.ReadVector4(br);
            m_currentBonePositionOut = des.ReadVector4(br);
            m_currentBoneRotationOut = des.ReadQuaternion(br);
            m_eventToSendWhenTargetReached = new hkbEventProperty();
            m_eventToSendWhenTargetReached.Read(des, br);
            m_childGenerator = des.ReadClassPointer<hkbGenerator>(br);
            m_duration = br.ReadSingle();
            m_ragdollBoneIndex = br.ReadInt16();
            m_animationBoneIndex = br.ReadInt16();
            m_targetMode = (TargetModeMBTT)br.ReadSByte();
            m_alignMode = br.ReadSByte();
            m_useVelocityPrediction = br.ReadBoolean();
            m_affectOrientation = br.ReadBoolean();
            m_currentBoneIsValidOut = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt16();
            br.ReadByte();
            m_finalAnimBonePositionMS = des.ReadVector4(br);
            m_initialAnimBonePositionMS = des.ReadVector4(br);
            m_finalAnimBoneOrientationMS = des.ReadQuaternion(br);
            m_animationFromRagdoll = des.ReadQuaternion(br);
            m_totalMotion = des.ReadQSTransform(br);
            m_accumulatedMotion = des.ReadQSTransform(br);
            m_useAnimationData = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkbpTarget>(bw, m_targetIn);
            s.WriteVector4(bw, m_offsetInBoneSpace);
            s.WriteVector4(bw, m_alignAxisBS);
            s.WriteVector4(bw, m_targetAlignAxisTS);
            s.WriteVector4(bw, m_alignWithCharacterForwardBS);
            s.WriteVector4(bw, m_currentBonePositionOut);
            s.WriteQuaternion(bw, m_currentBoneRotationOut);
            m_eventToSendWhenTargetReached.Write(s, bw);
            s.WriteClassPointer<hkbGenerator>(bw, m_childGenerator);
            bw.WriteSingle(m_duration);
            bw.WriteInt16(m_ragdollBoneIndex);
            bw.WriteInt16(m_animationBoneIndex);
            bw.WriteSByte((sbyte)m_targetMode);
            bw.WriteSByte(m_alignMode);
            bw.WriteBoolean(m_useVelocityPrediction);
            bw.WriteBoolean(m_affectOrientation);
            bw.WriteBoolean(m_currentBoneIsValidOut);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            s.WriteVector4(bw, m_finalAnimBonePositionMS);
            s.WriteVector4(bw, m_initialAnimBonePositionMS);
            s.WriteQuaternion(bw, m_finalAnimBoneOrientationMS);
            s.WriteQuaternion(bw, m_animationFromRagdoll);
            s.WriteQSTransform(bw, m_totalMotion);
            s.WriteQSTransform(bw, m_accumulatedMotion);
            bw.WriteBoolean(m_useAnimationData);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
