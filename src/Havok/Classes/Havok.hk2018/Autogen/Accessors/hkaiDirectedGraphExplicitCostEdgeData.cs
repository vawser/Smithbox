// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiDirectedGraphExplicitCostEdgeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiDirectedGraphExplicitCost.Edge>
{
    public hkaiDirectedGraphExplicitCostEdgeData(Havok.hk2018.hkaiDirectedGraphExplicitCost.Edge instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_cost":
            case "cost":
            {
                value = instance.m_cost;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_target":
            case "target":
            {
                value = instance.m_target;
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
                if (value is Havok.hk2018.hkaiDirectedGraphExplicitCost.EdgeBits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_flags = (Havok.hk2018.hkaiDirectedGraphExplicitCost.EdgeBits)ushortValue;
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
