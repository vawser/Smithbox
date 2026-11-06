// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaQuantizedAnimationTrackCompressionParamsData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaQuantizedAnimation.TrackCompressionParams>
{
    public hkaQuantizedAnimationTrackCompressionParamsData(Havok.hk2014.hkaQuantizedAnimation.TrackCompressionParams instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rotationTolerance":
            case "rotationTolerance":
            {
                value = instance.m_rotationTolerance;
                return true;
            }
            case "m_translationTolerance":
            case "translationTolerance":
            {
                value = instance.m_translationTolerance;
                return true;
            }
            case "m_scaleTolerance":
            case "scaleTolerance":
            {
                value = instance.m_scaleTolerance;
                return true;
            }
            case "m_floatingTolerance":
            case "floatingTolerance":
            {
                value = instance.m_floatingTolerance;
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
