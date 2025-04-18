// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaSplineCompressedAnimationAnimationCompressionParamsData : HavokData<hkaSplineCompressedAnimation.AnimationCompressionParams> 
{
    public hkaSplineCompressedAnimationAnimationCompressionParamsData(HavokType type, hkaSplineCompressedAnimation.AnimationCompressionParams instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_maxFramesPerBlock":
            case "maxFramesPerBlock":
            {
                if (instance.m_maxFramesPerBlock is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableSampleSingleTracks":
            case "enableSampleSingleTracks":
            {
                if (instance.m_enableSampleSingleTracks is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
