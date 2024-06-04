// Automatically Generated

namespace HKLib.hk2018;

public class hkbDockingGenerator : hkbGenerator, hkbVerifiable
{
    public short m_dockingBone;

    public Vector4 m_translationOffset = new();

    public Quaternion m_rotationOffset = new();

    public hkbDockingGenerator.BlendType m_blendType;

    public hkbDockingGenerator.DockingFlagBits m_flags;

    public hkbGenerator? m_child;

    public int m_intervalStart;

    public int m_intervalEnd;


    [Flags]
    public enum DockingFlagBits : int
    {
        FLAG_NONE = 0,
        FLAG_DOCK_TO_FUTURE_POSITION = 1,
        FLAG_OVERRIDE_MOTION = 2
    }

    public enum BlendType : int
    {
        BLEND_TYPE_BLEND_IN = 0,
        BLEND_TYPE_FULL_ON = 1
    }

}

