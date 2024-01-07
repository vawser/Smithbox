using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbBlendingTransitionEffectInternalState : hkReferencedObject
    {
        public override uint Signature { get => 2067425219; }
        
        public Vector4 m_fromPos;
        public Quaternion m_fromRot;
        public Vector4 m_toPos;
        public Quaternion m_toRot;
        public Vector4 m_lastPos;
        public Quaternion m_lastRot;
        public List<Matrix4x4> m_characterPoseAtBeginningOfTransition;
        public float m_timeRemaining;
        public float m_timeInTransition;
        public SelfTransitionMode m_toGeneratorSelfTranstitionMode;
        public bool m_initializeCharacterPose;
        public bool m_alignThisFrame;
        public bool m_alignmentFinished;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_fromPos = des.ReadVector4(br);
            m_fromRot = des.ReadQuaternion(br);
            m_toPos = des.ReadVector4(br);
            m_toRot = des.ReadQuaternion(br);
            m_lastPos = des.ReadVector4(br);
            m_lastRot = des.ReadQuaternion(br);
            m_characterPoseAtBeginningOfTransition = des.ReadQSTransformArray(br);
            m_timeRemaining = br.ReadSingle();
            m_timeInTransition = br.ReadSingle();
            m_toGeneratorSelfTranstitionMode = (SelfTransitionMode)br.ReadSByte();
            m_initializeCharacterPose = br.ReadBoolean();
            m_alignThisFrame = br.ReadBoolean();
            m_alignmentFinished = br.ReadBoolean();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_fromPos);
            s.WriteQuaternion(bw, m_fromRot);
            s.WriteVector4(bw, m_toPos);
            s.WriteQuaternion(bw, m_toRot);
            s.WriteVector4(bw, m_lastPos);
            s.WriteQuaternion(bw, m_lastRot);
            s.WriteQSTransformArray(bw, m_characterPoseAtBeginningOfTransition);
            bw.WriteSingle(m_timeRemaining);
            bw.WriteSingle(m_timeInTransition);
            bw.WriteSByte((sbyte)m_toGeneratorSelfTranstitionMode);
            bw.WriteBoolean(m_initializeCharacterPose);
            bw.WriteBoolean(m_alignThisFrame);
            bw.WriteBoolean(m_alignmentFinished);
            bw.WriteUInt32(0);
        }
    }
}
