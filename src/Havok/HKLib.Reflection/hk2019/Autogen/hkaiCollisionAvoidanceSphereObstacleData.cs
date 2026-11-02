// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiCollisionAvoidanceSphereObstacleData : HavokData<HKLib.hk2019.hkaiCollisionAvoidance.SphereObstacle> 
{
    public hkaiCollisionAvoidanceSphereObstacleData(HavokType type, HKLib.hk2019.hkaiCollisionAvoidance.SphereObstacle instance) : base(type, instance) {}

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
                if (value is not HKLib.hk2019.hkSphere castValue) return false;
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
