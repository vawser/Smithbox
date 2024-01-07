using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbRocketboxCharacterController : hkbModifier
    {
        public override uint Signature { get => 956945900; }
        
        public enum MovementSpeedsEnum
        {
            SL_Walk = 0,
            SL_WalkFast = 1,
            SL_RunSlow = 2,
            SL_Run = 3,
        }
        
        public hkbGenerator m_child;
        public bool m_autoTurnsAllowed;
        public int m_desiredAIMovementMode;
        public float m_effectiveLinearSpeed;
        public float m_effectiveAngularSpeed;
        public float m_effectiveHorizontalAim;
        public float m_effectiveVerticalAim;
        public float m_torsoTiltAngle;
        public float m_desiredAIMovementSpeed;
        public float m_currentMaximumSpeed;
        public bool m_stopRequest;
        public int m_idleToMoveAnimIdx;
        public float m_linearSpeed;
        public float m_angularSpeed;
        public float m_horizontalAim;
        public float m_verticalAim;
        public bool m_rotationAllowed;
        public bool m_poseChangeAllowed;
        public bool m_modifyLinearSpeed;
        public int m_poseIdx;
        public hkbEventProperty m_leftFootDownEvent;
        public hkbEventProperty m_rightFootDownEvent;
        public hkbEventProperty m_immediateStopEvent;
        public hkbEventProperty m_rapidTurnEvent;
        public hkbEventProperty m_changeMovementDirectionEvent;
        public hkbEventProperty m_changePoseEvent;
        public hkbEventProperty m_moveEvent;
        public hkbEventProperty m_stopEvent;
        public hkbEventProperty m_characterHasHalted;
        public List<float> m_moveVelocities;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_child = des.ReadClassPointer<hkbGenerator>(br);
            m_autoTurnsAllowed = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_desiredAIMovementMode = br.ReadInt32();
            m_effectiveLinearSpeed = br.ReadSingle();
            m_effectiveAngularSpeed = br.ReadSingle();
            m_effectiveHorizontalAim = br.ReadSingle();
            m_effectiveVerticalAim = br.ReadSingle();
            m_torsoTiltAngle = br.ReadSingle();
            m_desiredAIMovementSpeed = br.ReadSingle();
            m_currentMaximumSpeed = br.ReadSingle();
            m_stopRequest = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_idleToMoveAnimIdx = br.ReadInt32();
            m_linearSpeed = br.ReadSingle();
            m_angularSpeed = br.ReadSingle();
            m_horizontalAim = br.ReadSingle();
            m_verticalAim = br.ReadSingle();
            m_rotationAllowed = br.ReadBoolean();
            m_poseChangeAllowed = br.ReadBoolean();
            m_modifyLinearSpeed = br.ReadBoolean();
            br.ReadByte();
            m_poseIdx = br.ReadInt32();
            br.ReadUInt32();
            m_leftFootDownEvent = new hkbEventProperty();
            m_leftFootDownEvent.Read(des, br);
            m_rightFootDownEvent = new hkbEventProperty();
            m_rightFootDownEvent.Read(des, br);
            m_immediateStopEvent = new hkbEventProperty();
            m_immediateStopEvent.Read(des, br);
            m_rapidTurnEvent = new hkbEventProperty();
            m_rapidTurnEvent.Read(des, br);
            m_changeMovementDirectionEvent = new hkbEventProperty();
            m_changeMovementDirectionEvent.Read(des, br);
            m_changePoseEvent = new hkbEventProperty();
            m_changePoseEvent.Read(des, br);
            m_moveEvent = new hkbEventProperty();
            m_moveEvent.Read(des, br);
            m_stopEvent = new hkbEventProperty();
            m_stopEvent.Read(des, br);
            m_characterHasHalted = new hkbEventProperty();
            m_characterHasHalted.Read(des, br);
            m_moveVelocities = des.ReadSingleArray(br);
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkbGenerator>(bw, m_child);
            bw.WriteBoolean(m_autoTurnsAllowed);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteInt32(m_desiredAIMovementMode);
            bw.WriteSingle(m_effectiveLinearSpeed);
            bw.WriteSingle(m_effectiveAngularSpeed);
            bw.WriteSingle(m_effectiveHorizontalAim);
            bw.WriteSingle(m_effectiveVerticalAim);
            bw.WriteSingle(m_torsoTiltAngle);
            bw.WriteSingle(m_desiredAIMovementSpeed);
            bw.WriteSingle(m_currentMaximumSpeed);
            bw.WriteBoolean(m_stopRequest);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteInt32(m_idleToMoveAnimIdx);
            bw.WriteSingle(m_linearSpeed);
            bw.WriteSingle(m_angularSpeed);
            bw.WriteSingle(m_horizontalAim);
            bw.WriteSingle(m_verticalAim);
            bw.WriteBoolean(m_rotationAllowed);
            bw.WriteBoolean(m_poseChangeAllowed);
            bw.WriteBoolean(m_modifyLinearSpeed);
            bw.WriteByte(0);
            bw.WriteInt32(m_poseIdx);
            bw.WriteUInt32(0);
            m_leftFootDownEvent.Write(s, bw);
            m_rightFootDownEvent.Write(s, bw);
            m_immediateStopEvent.Write(s, bw);
            m_rapidTurnEvent.Write(s, bw);
            m_changeMovementDirectionEvent.Write(s, bw);
            m_changePoseEvent.Write(s, bw);
            m_moveEvent.Write(s, bw);
            m_stopEvent.Write(s, bw);
            m_characterHasHalted.Write(s, bw);
            s.WriteSingleArray(bw, m_moveVelocities);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
