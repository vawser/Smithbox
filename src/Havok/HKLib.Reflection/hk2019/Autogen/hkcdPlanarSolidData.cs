// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkcdPlanarSolidData : HavokData<HKLib.hk2019.hkcdPlanarSolid> 
{
    public hkcdPlanarSolidData(HavokType type, HKLib.hk2019.hkcdPlanarSolid instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_nodes":
            case "nodes":
            {
                if (value is null)
                {
                    instance.m_nodes = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkcdPlanarSolid.NodeStorage castValue)
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
                if (value is HKLib.hk2019.hkcdPlanarEntity.PlanesCollection castValue)
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
