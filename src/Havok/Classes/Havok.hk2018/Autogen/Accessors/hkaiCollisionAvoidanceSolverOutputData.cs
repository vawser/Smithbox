// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiCollisionAvoidanceSolverOutputData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiCollisionAvoidance.Solver.Output>
{
    public hkaiCollisionAvoidanceSolverOutputData(Havok.hk2018.hkaiCollisionAvoidance.Solver.Output instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_outputVelocity":
            case "outputVelocity":
            {
                value = instance.m_outputVelocity;
                return true;
            }
            case "m_penetrating":
            case "penetrating":
            {
                value = instance.m_penetrating;
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
            case "m_outputVelocity":
            case "outputVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_outputVelocity = castValue;
                return true;
            }
            case "m_penetrating":
            case "penetrating":
            {
                if (value is not bool castValue) return false;
                instance.m_penetrating = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
