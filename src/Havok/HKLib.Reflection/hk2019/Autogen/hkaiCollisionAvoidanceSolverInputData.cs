// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiCollisionAvoidanceSolverInputData : HavokData<HKLib.hk2019.hkaiCollisionAvoidance.Solver.Input> 
{
    public hkaiCollisionAvoidanceSolverInputData(HavokType type, HKLib.hk2019.hkaiCollisionAvoidance.Solver.Input instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_up":
            case "up":
            {
                if (instance.m_up is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_character":
            case "character":
            {
                if (instance.m_character is null)
                {
                    return true;
                }
                if (instance.m_character is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_neighbors":
            case "neighbors":
            {
                if (instance.m_neighbors is null)
                {
                    return true;
                }
                if (instance.m_neighbors is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_character":
            case "character":
            {
                if (value is null)
                {
                    instance.m_character = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiCollisionAvoidance.Character castValue)
                {
                    instance.m_character = castValue;
                    return true;
                }
                return false;
            }
            case "m_neighbors":
            case "neighbors":
            {
                if (value is null)
                {
                    instance.m_neighbors = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiCollisionAvoidance.NeighborCollector castValue)
                {
                    instance.m_neighbors = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
