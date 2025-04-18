// Automatically Generated

namespace HKLib.hk2018;

public class hkbAlignBoneModifier : hkbModifier, hkbVerifiable
{
    public hkbAlignBoneModifier.AlignModeABAM m_alignMode;

    public hkbAlignBoneModifier.AlignTargetMode m_alignTargetMode;

    public bool m_alignSingleAxis;

    public Vector4 m_alignAxis = new();

    public Vector4 m_alignTargetAxis = new();

    public Quaternion m_frameOfReference = new();

    public float m_duration;

    public int m_alignModeIndex;

    public int m_alignTargetModeIndex;


    public enum AlignTargetMode : int
    {
        ALIGN_TARGET_MODE_CHARACTER_WORLD_FROM_MODEL = 0,
        ALIGN_TARGET_MODE_RAGDOLL_SKELETON_BONE = 1,
        ALIGN_TARGET_MODE_ANIMATION_SKELETON_BONE = 2,
        ALIGN_TARGET_MODE_USER_SPECIFIED_FRAME_OF_REFERENCE = 3
    }

    public enum AlignModeABAM : int
    {
        ALIGN_MODE_CHARACTER_WORLD_FROM_MODEL = 0,
        ALIGN_MODE_ANIMATION_SKELETON_BONE = 1
    }

}

