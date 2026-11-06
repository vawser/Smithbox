// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpBroadPhaseConfigLayerData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpBroadPhaseConfig.Layer>
{
    public hknpBroadPhaseConfigLayerData(Havok.hk2014.hknpBroadPhaseConfig.Layer instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_collideWithLayerMask":
            case "collideWithLayerMask":
            {
                value = instance.m_collideWithLayerMask;
                return true;
            }
            case "m_isVolatile":
            case "isVolatile":
            {
                value = instance.m_isVolatile;
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
            case "m_collideWithLayerMask":
            case "collideWithLayerMask":
            {
                if (value is not uint castValue) return false;
                instance.m_collideWithLayerMask = castValue;
                return true;
            }
            case "m_isVolatile":
            case "isVolatile":
            {
                if (value is not bool castValue) return false;
                instance.m_isVolatile = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
