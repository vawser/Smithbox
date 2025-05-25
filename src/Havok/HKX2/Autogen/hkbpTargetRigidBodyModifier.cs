using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum EventModeTRBAM
    {
        EVENT_MODE_DO_NOT_SEND = 0,
        EVENT_MODE_SEND_ONCE = 1,
        EVENT_MODE_RESEND = 2,
    }
    
    public enum TargetMode
    {
        TARGET_MODE_CONE_AROUND_CHARACTER_FORWARD = 0,
        TARGET_MODE_CONE_AROUND_LOCAL_AXIS = 1,
        TARGET_MODE_RAYCAST_ALONG_CHARACTER_FORWARD = 2,
        TARGET_MODE_RAYCAST_ALONG_LOCAL_AXIS = 3,
        TARGET_MODE_ANY_DIRECTION = 4,
    }
    
    public enum ComputeTargetAngleMode
    {
        COMPUTE_ANGLE_USING_TARGET_COM = 0,
        COMPUTE_ANGLE_USING_TARGET_CONTACT_POINT = 1,
    }
    
    public enum ComputeTargetDistanceMode
    {
        COMPUTE_DISTANCE_USING_TARGET_POSITION = 0,
        COMPUTE_DISTANCE_USING_TARGET_CONTACT_POINT = 1,
    }
    
    public partial class hkbpTargetRigidBodyModifier : hkbModifier
    {
        public override uint Signature { get => 3805051629; }
        
        public hkbpTarget m_targetOut;
        public TargetMode m_targetMode;
        public int m_sensingLayer;
        public bool m_targetOnlyOnce;
        public bool m_ignoreMySystemGroup;
        public float m_maxTargetDistance;
        public float m_maxTargetHeightAboveSensor;
        public float m_closeToTargetDistanceThreshold;
        public ComputeTargetAngleMode m_targetAngleMode;
        public ComputeTargetDistanceMode m_targetDistanceMode;
        public float m_maxAngleToTarget;
        public short m_sensorRagdollBoneIndex;
        public short m_sensorAnimationBoneIndex;
        public short m_closeToTargetRagdollBoneIndex;
        public short m_closeToTargetAnimationBoneIndex;
        public Vector4 m_sensorOffsetInBoneSpace;
        public Vector4 m_closeToTargetOffsetInBoneSpace;
        public Vector4 m_sensorDirectionBS;
        public EventModeTRBAM m_eventMode;
        public uint m_sensingPropertyKey;
        public bool m_sensorInWS;
        public hkbEventProperty m_eventToSend;
        public hkbEventProperty m_eventToSendToTarget;
        public hkbEventProperty m_closeToTargetEvent;
        public bool m_useVelocityPrediction;
        public bool m_targetOnlySpheres;
        public bool m_isCloseToTargetOut;
        public sbyte m_targetPriority;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_targetOut = des.ReadClassPointer<hkbpTarget>(br);
            m_targetMode = (TargetMode)br.ReadSByte();
            br.ReadUInt16();
            br.ReadByte();
            m_sensingLayer = br.ReadInt32();
            m_targetOnlyOnce = br.ReadBoolean();
            m_ignoreMySystemGroup = br.ReadBoolean();
            br.ReadUInt16();
            m_maxTargetDistance = br.ReadSingle();
            m_maxTargetHeightAboveSensor = br.ReadSingle();
            m_closeToTargetDistanceThreshold = br.ReadSingle();
            m_targetAngleMode = (ComputeTargetAngleMode)br.ReadSByte();
            m_targetDistanceMode = (ComputeTargetDistanceMode)br.ReadSByte();
            br.ReadUInt16();
            m_maxAngleToTarget = br.ReadSingle();
            m_sensorRagdollBoneIndex = br.ReadInt16();
            m_sensorAnimationBoneIndex = br.ReadInt16();
            m_closeToTargetRagdollBoneIndex = br.ReadInt16();
            m_closeToTargetAnimationBoneIndex = br.ReadInt16();
            br.ReadUInt64();
            m_sensorOffsetInBoneSpace = des.ReadVector4(br);
            m_closeToTargetOffsetInBoneSpace = des.ReadVector4(br);
            m_sensorDirectionBS = des.ReadVector4(br);
            m_eventMode = (EventModeTRBAM)br.ReadSByte();
            br.ReadUInt16();
            br.ReadByte();
            m_sensingPropertyKey = br.ReadUInt32();
            m_sensorInWS = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
            m_eventToSend = new hkbEventProperty();
            m_eventToSend.Read(des, br);
            m_eventToSendToTarget = new hkbEventProperty();
            m_eventToSendToTarget.Read(des, br);
            m_closeToTargetEvent = new hkbEventProperty();
            m_closeToTargetEvent.Read(des, br);
            m_useVelocityPrediction = br.ReadBoolean();
            m_targetOnlySpheres = br.ReadBoolean();
            m_isCloseToTargetOut = br.ReadBoolean();
            m_targetPriority = br.ReadSByte();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkbpTarget>(bw, m_targetOut);
            bw.WriteSByte((sbyte)m_targetMode);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteInt32(m_sensingLayer);
            bw.WriteBoolean(m_targetOnlyOnce);
            bw.WriteBoolean(m_ignoreMySystemGroup);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_maxTargetDistance);
            bw.WriteSingle(m_maxTargetHeightAboveSensor);
            bw.WriteSingle(m_closeToTargetDistanceThreshold);
            bw.WriteSByte((sbyte)m_targetAngleMode);
            bw.WriteSByte((sbyte)m_targetDistanceMode);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_maxAngleToTarget);
            bw.WriteInt16(m_sensorRagdollBoneIndex);
            bw.WriteInt16(m_sensorAnimationBoneIndex);
            bw.WriteInt16(m_closeToTargetRagdollBoneIndex);
            bw.WriteInt16(m_closeToTargetAnimationBoneIndex);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_sensorOffsetInBoneSpace);
            s.WriteVector4(bw, m_closeToTargetOffsetInBoneSpace);
            s.WriteVector4(bw, m_sensorDirectionBS);
            bw.WriteSByte((sbyte)m_eventMode);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteUInt32(m_sensingPropertyKey);
            bw.WriteBoolean(m_sensorInWS);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            m_eventToSend.Write(s, bw);
            m_eventToSendToTarget.Write(s, bw);
            m_closeToTargetEvent.Write(s, bw);
            bw.WriteBoolean(m_useVelocityPrediction);
            bw.WriteBoolean(m_targetOnlySpheres);
            bw.WriteBoolean(m_isCloseToTargetOut);
            bw.WriteSByte(m_targetPriority);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
