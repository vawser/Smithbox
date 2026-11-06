// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkpPhysicsDataData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkpPhysicsData>
{
    public hkpPhysicsDataData(Havok.hk55.hkpPhysicsData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_worldCinfo":
            case "worldCinfo":
            {
                value = instance.m_worldCinfo;
                return true;
            }
            case "m_systems":
            case "systems":
            {
                value = instance.m_systems;
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
            case "m_worldCinfo":
            case "worldCinfo":
            {
                if (value is null)
                {
                    instance.m_worldCinfo = default;
                    return true;
                }
                if (value is Havok.hk55.hkpWorldCinfo castValue)
                {
                    instance.m_worldCinfo = castValue;
                    return true;
                }
                return false;
            }
            case "m_systems":
            case "systems":
            {
                if (value is not List<Havok.hk55.hkpPhysicsSystem?> castValue) return false;
                instance.m_systems = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
