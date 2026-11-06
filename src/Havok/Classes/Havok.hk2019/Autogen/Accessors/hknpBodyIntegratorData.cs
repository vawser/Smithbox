// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpBodyIntegratorData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpBodyIntegrator>
{
    public hknpBodyIntegratorData(Havok.hk2019.hknpBodyIntegrator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bodyFlagsForcingNormalCollisions":
            case "bodyFlagsForcingNormalCollisions":
            {
                value = instance.m_bodyFlagsForcingNormalCollisions;
                return true;
            }
            case "m_typeForDynamicBodies":
            case "typeForDynamicBodies":
            {
                value = instance.m_typeForDynamicBodies;
                return true;
            }
            case "m_typeForTriggers":
            case "typeForTriggers":
            {
                value = instance.m_typeForTriggers;
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
            case "m_bodyFlagsForcingNormalCollisions":
            case "bodyFlagsForcingNormalCollisions":
            {
                if (value is Havok.hk2019.hknpCollisionFlags.Enum castValue)
                {
                    instance.m_bodyFlagsForcingNormalCollisions = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_bodyFlagsForcingNormalCollisions = (Havok.hk2019.hknpCollisionFlags.Enum)intValue;
                    return true;
                }
                return false;
            }
            case "m_typeForDynamicBodies":
            case "typeForDynamicBodies":
            {
                if (value is Havok.hk2019.hknpManifoldType.Enum castValue)
                {
                    instance.m_typeForDynamicBodies = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_typeForDynamicBodies = (Havok.hk2019.hknpManifoldType.Enum)intValue;
                    return true;
                }
                return false;
            }
            case "m_typeForTriggers":
            case "typeForTriggers":
            {
                if (value is Havok.hk2019.hknpManifoldType.Enum castValue)
                {
                    instance.m_typeForTriggers = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_typeForTriggers = (Havok.hk2019.hknpManifoldType.Enum)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
