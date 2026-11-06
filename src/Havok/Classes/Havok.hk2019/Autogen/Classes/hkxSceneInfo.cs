// Automatically Generated

namespace Havok.hk2019;

public class hkxSceneInfo : IHavokObject
{
    public List<hkxSceneInfo.AnimationStack> m_animationStacks = new();


    public class AnimationStack : IHavokObject
    {
        public string? m_name;

        public hkxSceneInfo.TimeRange m_globalTime = new();

        public List<hkxSceneInfo.Layer> m_layers = new();

    }

    public class Layer : IHavokObject
    {
        public string? m_name;

        public hkxSceneInfo.TimeRange m_timeRange = new();

    }

    public class TimeRange : IHavokObject
    {
        public float m_start;

        public float m_end;

        public float m_fps;

    }

}
