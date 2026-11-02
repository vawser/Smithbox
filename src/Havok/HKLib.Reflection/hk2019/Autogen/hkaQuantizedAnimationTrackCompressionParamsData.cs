// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaQuantizedAnimationTrackCompressionParamsData : HavokData<HKLib.hk2019.hkaQuantizedAnimation.TrackCompressionParams> 
{
    public hkaQuantizedAnimationTrackCompressionParamsData(HavokType type, HKLib.hk2019.hkaQuantizedAnimation.TrackCompressionParams instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rotationTolerance":
            case "rotationTolerance":
            {
                if (instance.m_rotationTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_translationTolerance":
            case "translationTolerance":
            {
                if (instance.m_translationTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_scaleTolerance":
            case "scaleTolerance":
            {
                if (instance.m_scaleTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_floatingTolerance":
            case "floatingTolerance":
            {
                if (instance.m_floatingTolerance is not TGet castValue) return false;
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
            case "m_rotationTolerance":
            case "rotationTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_rotationTolerance = castValue;
                return true;
            }
            case "m_translationTolerance":
            case "translationTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_translationTolerance = castValue;
                return true;
            }
            case "m_scaleTolerance":
            case "scaleTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_scaleTolerance = castValue;
                return true;
            }
            case "m_floatingTolerance":
            case "floatingTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_floatingTolerance = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
