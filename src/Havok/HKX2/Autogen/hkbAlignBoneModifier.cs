using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum AlignModeABAM
    {
        ALIGN_MODE_CHARACTER_WORLD_FROM_MODEL = 0,
        ALIGN_MODE_ANIMATION_SKELETON_BONE = 1,
    }
    
    public enum AlignTargetMode
    {
        ALIGN_TARGET_MODE_CHARACTER_WORLD_FROM_MODEL = 0,
        ALIGN_TARGET_MODE_RAGDOLL_SKELETON_BONE = 1,
        ALIGN_TARGET_MODE_ANIMATION_SKELETON_BONE = 2,
        ALIGN_TARGET_MODE_USER_SPECIFIED_FRAME_OF_REFERENCE = 3,
    }
    
    public partial class hkbAlignBoneModifier : hkbModifier
    {
        public override uint Signature { get => 2587847552; }
        
        public AlignModeABAM m_alignMode;
        public AlignTargetMode m_alignTargetMode;
        public bool m_alignSingleAxis;
        public Vector4 m_alignAxis;
        public Vector4 m_alignTargetAxis;
        public Quaternion m_frameOfReference;
        public float m_duration;
        public int m_alignModeIndex;
        public int m_alignTargetModeIndex;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_alignMode = (AlignModeABAM)br.ReadSByte();
            m_alignTargetMode = (AlignTargetMode)br.ReadSByte();
            m_alignSingleAxis = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadByte();
            m_alignAxis = des.ReadVector4(br);
            m_alignTargetAxis = des.ReadVector4(br);
            m_frameOfReference = des.ReadQuaternion(br);
            m_duration = br.ReadSingle();
            m_alignModeIndex = br.ReadInt32();
            m_alignTargetModeIndex = br.ReadInt32();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSByte((sbyte)m_alignMode);
            bw.WriteSByte((sbyte)m_alignTargetMode);
            bw.WriteBoolean(m_alignSingleAxis);
            bw.WriteUInt32(0);
            bw.WriteByte(0);
            s.WriteVector4(bw, m_alignAxis);
            s.WriteVector4(bw, m_alignTargetAxis);
            s.WriteQuaternion(bw, m_frameOfReference);
            bw.WriteSingle(m_duration);
            bw.WriteInt32(m_alignModeIndex);
            bw.WriteInt32(m_alignTargetModeIndex);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
