// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkcdPlanarSolidData : HavokData<hkcdPlanarSolid> 
{
    public hkcdPlanarSolidData(HavokType type, hkcdPlanarSolid instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nodes":
            case "nodes":
            {
                if (instance.m_nodes is null)
                {
                    return true;
                }
                if (instance.m_nodes is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_planes":
            case "planes":
            {
                if (instance.m_planes is null)
                {
                    return true;
                }
                if (instance.m_planes is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_rootNodeId":
            case "rootNodeId":
            {
                if (instance.m_rootNodeId is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_nodes":
            case "nodes":
            {
                if (value is null)
                {
                    instance.m_nodes = default;
                    return true;
                }
                if (value is hkcdPlanarSolid.NodeStorage castValue)
                {
                    instance.m_nodes = castValue;
                    return true;
                }
                return false;
            }
            case "m_planes":
            case "planes":
            {
                if (value is null)
                {
                    instance.m_planes = default;
                    return true;
                }
                if (value is hkcdPlanarEntity.PlanesCollection castValue)
                {
                    instance.m_planes = castValue;
                    return true;
                }
                return false;
            }
            case "m_rootNodeId":
            case "rootNodeId":
            {
                if (value is not uint castValue) return false;
                instance.m_rootNodeId = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
