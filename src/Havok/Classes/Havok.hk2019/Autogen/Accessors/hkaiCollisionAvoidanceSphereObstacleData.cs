// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiCollisionAvoidanceSphereObstacleData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiCollisionAvoidance.SphereObstacle>
{
    public hkaiCollisionAvoidanceSphereObstacleData(Havok.hk2019.hkaiCollisionAvoidance.SphereObstacle instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sphere":
            case "sphere":
            {
                value = instance.m_sphere;
                return true;
            }
            case "m_velocity":
            case "velocity":
            {
                value = instance.m_velocity;
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
            case "m_sphere":
            case "sphere":
            {
                if (value is not Havok.hk2019.hkSphere castValue) return false;
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
