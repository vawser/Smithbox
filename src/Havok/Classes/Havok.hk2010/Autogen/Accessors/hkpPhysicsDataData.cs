// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkpPhysicsDataData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkpPhysicsData>
{
    public hkpPhysicsDataData(Havok.hk2010.hkpPhysicsData instance) : base(instance) { }

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
                if (value is Havok.hk2010.hkpWorldCinfo castValue)
                {
                    instance.m_worldCinfo = castValue;
                    return true;
                }
                return false;
            }
            case "m_systems":
            case "systems":
            {
                if (value is not List<Havok.hk2010.hkpPhysicsSystem?> castValue) return false;
                instance.m_systems = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
