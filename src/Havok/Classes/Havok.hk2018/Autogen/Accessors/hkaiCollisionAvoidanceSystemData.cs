// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiCollisionAvoidanceSystemData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiCollisionAvoidance.System>
{
    public hkaiCollisionAvoidanceSystemData(Havok.hk2018.hkaiCollisionAvoidance.System instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_characters":
            case "characters":
            {
                value = instance.m_characters;
                return true;
            }
            case "m_enabledCharacters":
            case "enabledCharacters":
            {
                value = instance.m_enabledCharacters;
                return true;
            }
            case "m_obstacleGenerators":
            case "obstacleGenerators":
            {
                value = instance.m_obstacleGenerators;
                return true;
            }
            case "m_filter":
            case "filter":
            {
                value = instance.m_filter;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_characters":
            case "characters":
            {
                if (value is not List<Havok.hk2018.hkaiCollisionAvoidance.Character?> castValue) return false;
                instance.m_characters = castValue;
                return true;
            }
            case "m_enabledCharacters":
            case "enabledCharacters":
            {
                if (value is not List<Havok.hk2018.hkaiCollisionAvoidance.Character?> castValue) return false;
                instance.m_enabledCharacters = castValue;
                return true;
            }
            case "m_obstacleGenerators":
            case "obstacleGenerators":
            {
                if (value is not List<Havok.hk2018.hkaiCollisionAvoidance.ObstacleGenerator?> castValue) return false;
                instance.m_obstacleGenerators = castValue;
                return true;
            }
            case "m_filter":
            case "filter":
            {
                if (value is null)
                {
                    instance.m_filter = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaiCollisionAvoidance.Filter castValue)
                {
                    instance.m_filter = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
