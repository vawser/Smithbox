// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiDirectedGraphExplicitCostEdgeData : HavokData<hkaiDirectedGraphExplicitCost.Edge> 
{
    public hkaiDirectedGraphExplicitCostEdgeData(HavokType type, hkaiDirectedGraphExplicitCost.Edge instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_cost":
            case "cost":
            {
                if (instance.m_cost is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_flags is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_target":
            case "target":
            {
                if (instance.m_target is not TGet castValue) return false;
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
            case "m_cost":
            case "cost":
            {
                if (value is not float castValue) return false;
                instance.m_cost = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is hkaiDirectedGraphExplicitCost.EdgeBits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_flags = (hkaiDirectedGraphExplicitCost.EdgeBits)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_target":
            case "target":
            {
                if (value is not uint castValue) return false;
                instance.m_target = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
