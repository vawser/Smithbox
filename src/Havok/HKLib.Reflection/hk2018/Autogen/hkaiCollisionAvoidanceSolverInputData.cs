// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkaiCollisionAvoidance;
using HKLib.hk2018.hkaiCollisionAvoidance.Solver;

namespace HKLib.Reflection.hk2018;

internal class hkaiCollisionAvoidanceSolverInputData : HavokData<Input> 
{
    public hkaiCollisionAvoidanceSolverInputData(HavokType type, Input instance) : base(type, instance) {}

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
                if (value is Character castValue)
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
                if (value is NeighborCollector castValue)
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
