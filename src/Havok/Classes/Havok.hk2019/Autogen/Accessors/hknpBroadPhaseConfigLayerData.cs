// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpBroadPhaseConfigLayerData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpBroadPhaseConfig.Layer>
{
    public hknpBroadPhaseConfigLayerData(Havok.hk2019.hknpBroadPhaseConfig.Layer instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_filterMask":
            case "filterMask":
            {
                value = instance.m_filterMask;
                return true;
            }
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
            case "m_recollideOnDirty":
            case "recollideOnDirty":
            {
                value = instance.m_recollideOnDirty;
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
            case "m_filterMask":
            case "filterMask":
            {
                if (value is Havok.hk2019.hknpBroadPhaseFilter.Category castValue)
                {
                    instance.m_filterMask = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_filterMask = (Havok.hk2019.hknpBroadPhaseFilter.Category)byteValue;
                    return true;
                }
                return false;
            }
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
            case "m_recollideOnDirty":
            case "recollideOnDirty":
            {
                if (value is not bool castValue) return false;
                instance.m_recollideOnDirty = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
