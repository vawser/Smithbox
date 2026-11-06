// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiCollisionAvoidanceSolverInputData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiCollisionAvoidance.Solver.Input>
{
    public hkaiCollisionAvoidanceSolverInputData(Havok.hk2018.hkaiCollisionAvoidance.Solver.Input instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_character":
            case "character":
            {
                value = instance.m_character;
                return true;
            }
            case "m_neighbors":
            case "neighbors":
            {
                value = instance.m_neighbors;
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
                if (value is Havok.hk2018.hkaiCollisionAvoidance.Character castValue)
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
                if (value is Havok.hk2018.hkaiCollisionAvoidance.NeighborCollector castValue)
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
