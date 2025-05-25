// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiAvoidanceSolver;

namespace HKLib.Reflection.hk2018;

internal class hkaiAvoidanceSolverSphereObstacleData : HavokData<SphereObstacle> 
{
    public hkaiAvoidanceSolverSphereObstacleData(HavokType type, SphereObstacle instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sphere":
            case "sphere":
            {
                if (instance.m_sphere is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_velocity":
            case "velocity":
            {
                if (instance.m_velocity is not TGet castValue) return false;
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
            case "m_sphere":
            case "sphere":
            {
                if (value is not hkSphere castValue) return false;
                instance.m_sphere = castValue;
                return true;
            }
            case "m_velocity":
            case "velocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_velocity = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
