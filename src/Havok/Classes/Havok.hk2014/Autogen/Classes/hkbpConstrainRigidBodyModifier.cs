// Automatically Generated

namespace Havok.hk2014;

public class hkbpConstrainRigidBodyModifier : hkbModifier
{
    public hkbpTarget? m_targetIn;

    public float m_breakThreshold;

    public short m_ragdollBoneToConstrain;

    public bool m_breakable;

    public hkbpConstrainRigidBodyModifier.PivotPlacement m_pivotPlacement;

    public hkbpConstrainRigidBodyModifier.BoneToConstrainPlacement m_boneToConstrainPlacement;

    public hkpConstraintData.ConstraintType m_constraintType;

    public bool m_clearTargetData;

    public bool m_isConstraintHinge;


    public enum PivotPlacement : int
    {
        PIVOT_MIDWAY_BETWEEN_CENTROIDS = 0,
        PIVOT_AT_TARGET_CONTACT_POINT = 1,
        PIVOT_MIDWAY_BETWEEN_TARGET_SHAPE_CENTROID_AND_BODY_TO_CONSTRAIN_CENTROID = 2
    }

    public enum BoneToConstrainPlacement : int
    {
        BTCP_AT_CURRENT_POSITION = 0,
        BTCP_ALIGN_COM_AND_PIVOT = 1
    }

}
