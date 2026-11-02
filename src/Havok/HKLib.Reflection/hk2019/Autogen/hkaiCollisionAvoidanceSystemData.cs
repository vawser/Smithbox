// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiCollisionAvoidanceSystemData : HavokData<HKLib.hk2019.hkaiCollisionAvoidance.System> 
{
    public hkaiCollisionAvoidanceSystemData(HavokType type, HKLib.hk2019.hkaiCollisionAvoidance.System instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_up":
            case "up":
            {
                if (instance.m_up is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characters":
            case "characters":
            {
                if (instance.m_characters is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enabledCharacters":
            case "enabledCharacters":
            {
                if (instance.m_enabledCharacters is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_obstacleGenerators":
            case "obstacleGenerators":
            {
                if (instance.m_obstacleGenerators is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_filter":
            case "filter":
            {
                if (instance.m_filter is null)
                {
                    return true;
                }
                if (instance.m_filter is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
            case "m_characters":
            case "characters":
            {
                if (value is not List<HKLib.hk2019.hkaiCollisionAvoidance.Character?> castValue) return false;
                instance.m_characters = castValue;
                return true;
            }
            case "m_enabledCharacters":
            case "enabledCharacters":
            {
                if (value is not List<HKLib.hk2019.hkaiCollisionAvoidance.Character?> castValue) return false;
                instance.m_enabledCharacters = castValue;
                return true;
            }
            case "m_obstacleGenerators":
            case "obstacleGenerators":
            {
                if (value is not List<HKLib.hk2019.hkaiCollisionAvoidance.ObstacleGenerator?> castValue) return false;
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
                if (value is HKLib.hk2019.hkaiCollisionAvoidance.Filter castValue)
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
