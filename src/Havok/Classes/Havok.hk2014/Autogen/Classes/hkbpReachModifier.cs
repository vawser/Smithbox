// Automatically Generated

namespace Havok.hk2014;

public class hkbpReachModifier : hkbModifier
{
    public List<hkbpReachModifierHand> m_hands = new();

    public float m_newTargetGain;

    public float m_noTargetGain;

    public float m_targetGain;

    public float m_fadeOutDuration;

    public int m_raycastLayer;

    public uint m_sensingPropertyKey;

    public hkbpReachModifier.ReachMode m_reachMode;

    public bool m_ignoreMySystemGroup;

    public float m_extrapolate;


    public enum ReachMode : int
    {
        REACH_MODE_TERRAIN = 0,
        REACH_MODE_WORLD_POSITION = 1,
        REACH_MODE_MODEL_POSITION = 2,
        REACH_MODE_BONE_POSITION = 3
    }

}
