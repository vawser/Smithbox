using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum AttachmentType
    {
        ATTACHMENT_TYPE_KEYFRAME_RIGID_BODY = 0,
        ATTACHMENT_TYPE_BALL_SOCKET_CONSTRAINT = 1,
        ATTACHMENT_TYPE_RAGDOLL_CONSTRAINT = 2,
        ATTACHMENT_TYPE_SET_WORLD_FROM_MODEL = 3,
        ATTACHMENT_TYPE_NONE = 4,
    }
    
    public partial class hkbAttachmentSetup : hkReferencedObject
    {
        public override uint Signature { get => 3482710003; }
        
        public float m_blendInTime;
        public float m_moveAttacherFraction;
        public float m_gain;
        public float m_extrapolationTimeStep;
        public float m_fixUpGain;
        public float m_maxLinearDistance;
        public float m_maxAngularDistance;
        public AttachmentType m_attachmentType;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_blendInTime = br.ReadSingle();
            m_moveAttacherFraction = br.ReadSingle();
            m_gain = br.ReadSingle();
            m_extrapolationTimeStep = br.ReadSingle();
            m_fixUpGain = br.ReadSingle();
            m_maxLinearDistance = br.ReadSingle();
            m_maxAngularDistance = br.ReadSingle();
            m_attachmentType = (AttachmentType)br.ReadSByte();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_blendInTime);
            bw.WriteSingle(m_moveAttacherFraction);
            bw.WriteSingle(m_gain);
            bw.WriteSingle(m_extrapolationTimeStep);
            bw.WriteSingle(m_fixUpGain);
            bw.WriteSingle(m_maxLinearDistance);
            bw.WriteSingle(m_maxAngularDistance);
            bw.WriteSByte((sbyte)m_attachmentType);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
