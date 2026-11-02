// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiCollisionAvoidanceSolverOutputData : HavokData<HKLib.hk2019.hkaiCollisionAvoidance.Solver.Output> 
{
    public hkaiCollisionAvoidanceSolverOutputData(HavokType type, HKLib.hk2019.hkaiCollisionAvoidance.Solver.Output instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_outputVelocity":
            case "outputVelocity":
            {
                if (instance.m_outputVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_penetrating":
            case "penetrating":
            {
                if (instance.m_penetrating is not TGet castValue) return false;
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
