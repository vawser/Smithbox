using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum PivotPlacement
    {
        PIVOT_MIDWAY_BETWEEN_CENTROIDS = 0,
        PIVOT_AT_TARGET_CONTACT_POINT = 1,
        PIVOT_MIDWAY_BETWEEN_TARGET_SHAPE_CENTROID_AND_BODY_TO_CONSTRAIN_CENTROID = 2,
    }
    
    public enum BoneToConstrainPlacement
    {
        BTCP_AT_CURRENT_POSITION = 0,
        BTCP_ALIGN_COM_AND_PIVOT = 1,
    }
    
    public partial class hkbpConstrainRigidBodyModifier : hkbModifier
    {
        public override uint Signature { get => 3685860762; }
        
        public hkbpTarget m_targetIn;
        public float m_breakThreshold;
        public short m_ragdollBoneToConstrain;
        public bool m_breakable;
        public PivotPlacement m_pivotPlacement;
        public BoneToConstrainPlacement m_boneToConstrainPlacement;
        public ConstraintType m_constraintType;
        public bool m_clearTargetData;
        public bool m_isConstraintHinge;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_targetIn = des.ReadClassPointer<hkbpTarget>(br);
            m_breakThreshold = br.ReadSingle();
            m_ragdollBoneToConstrain = br.ReadInt16();
            m_breakable = br.ReadBoolean();
            m_pivotPlacement = (PivotPlacement)br.ReadSByte();
            m_boneToConstrainPlacement = (BoneToConstrainPlacement)br.ReadSByte();
            m_constraintType = (ConstraintType)br.ReadSByte();
            m_clearTargetData = br.ReadBoolean();
            m_isConstraintHinge = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkbpTarget>(bw, m_targetIn);
            bw.WriteSingle(m_breakThreshold);
            bw.WriteInt16(m_ragdollBoneToConstrain);
            bw.WriteBoolean(m_breakable);
            bw.WriteSByte((sbyte)m_pivotPlacement);
            bw.WriteSByte((sbyte)m_boneToConstrainPlacement);
            bw.WriteSByte((sbyte)m_constraintType);
            bw.WriteBoolean(m_clearTargetData);
            bw.WriteBoolean(m_isConstraintHinge);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
