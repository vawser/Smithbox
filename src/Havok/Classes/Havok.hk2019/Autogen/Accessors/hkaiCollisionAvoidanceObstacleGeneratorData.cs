// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiCollisionAvoidanceObstacleGeneratorData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiCollisionAvoidance.ObstacleGenerator>
{
    public hkaiCollisionAvoidanceObstacleGeneratorData(Havok.hk2019.hkaiCollisionAvoidance.ObstacleGenerator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
