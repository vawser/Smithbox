// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaSplineCompressedAnimationAnimationCompressionParamsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaSplineCompressedAnimation.AnimationCompressionParams>
{
    public hkaSplineCompressedAnimationAnimationCompressionParamsData(Havok.hk2019.hkaSplineCompressedAnimation.AnimationCompressionParams instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_maxFramesPerBlock":
            case "maxFramesPerBlock":
            {
                value = instance.m_maxFramesPerBlock;
                return true;
            }
            case "m_enableSampleSingleTracks":
            case "enableSampleSingleTracks":
            {
                value = instance.m_enableSampleSingleTracks;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_maxFramesPerBlock":
            case "maxFramesPerBlock":
            {
                if (value is not ushort castValue) return false;
                instance.m_maxFramesPerBlock = castValue;
                return true;
            }
            case "m_enableSampleSingleTracks":
            case "enableSampleSingleTracks":
            {
                if (value is not bool castValue) return false;
                instance.m_enableSampleSingleTracks = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
