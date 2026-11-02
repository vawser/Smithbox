// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpBroadPhaseConfigLayerData : HavokData<HKLib.hk2019.hknpBroadPhaseConfig.Layer> 
{
    public hknpBroadPhaseConfigLayerData(HavokType type, HKLib.hk2019.hknpBroadPhaseConfig.Layer instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_filterMask":
            case "filterMask":
            {
                if (instance.m_filterMask is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_filterMask is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_collideWithLayerMask":
            case "collideWithLayerMask":
            {
                if (instance.m_collideWithLayerMask is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isVolatile":
            case "isVolatile":
            {
                if (instance.m_isVolatile is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_recollideOnDirty":
            case "recollideOnDirty":
            {
                if (instance.m_recollideOnDirty is not TGet castValue) return false;
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
            case "m_filterMask":
            case "filterMask":
            {
                if (value is HKLib.hk2019.hknpBroadPhaseFilter.Category castValue)
                {
                    instance.m_filterMask = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_filterMask = (HKLib.hk2019.hknpBroadPhaseFilter.Category)byteValue;
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
