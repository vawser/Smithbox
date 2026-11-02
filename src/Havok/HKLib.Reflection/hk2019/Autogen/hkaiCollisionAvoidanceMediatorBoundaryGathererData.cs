// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiCollisionAvoidanceMediatorBoundaryGathererData : HavokData<HKLib.hk2019.hkaiCollisionAvoidance.MediatorBoundaryGatherer> 
{
    public hkaiCollisionAvoidanceMediatorBoundaryGathererData(HavokType type, HKLib.hk2019.hkaiCollisionAvoidance.MediatorBoundaryGatherer instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_world":
            case "world":
            {
                if (instance.m_world is null)
                {
                    return true;
                }
                if (instance.m_world is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_layerIndex":
            case "layerIndex":
            {
                if (instance.m_layerIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_filterInfo":
            case "filterInfo":
            {
                if (instance.m_filterInfo is not TGet castValue) return false;
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
            case "m_world":
            case "world":
            {
                if (value is null)
                {
                    instance.m_world = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiWorld castValue)
                {
                    instance.m_world = castValue;
                    return true;
                }
                return false;
            }
            case "m_layerIndex":
            case "layerIndex":
            {
                if (value is not int castValue) return false;
                instance.m_layerIndex = castValue;
                return true;
            }
            case "m_filterInfo":
            case "filterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_filterInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
