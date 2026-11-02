// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpBodyIntegratorData : HavokData<HKLib.hk2019.hknpBodyIntegrator> 
{
    public hknpBodyIntegratorData(HavokType type, HKLib.hk2019.hknpBodyIntegrator instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bodyFlagsForcingNormalCollisions":
            case "bodyFlagsForcingNormalCollisions":
            {
                if (instance.m_bodyFlagsForcingNormalCollisions is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_bodyFlagsForcingNormalCollisions is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_typeForDynamicBodies":
            case "typeForDynamicBodies":
            {
                if (instance.m_typeForDynamicBodies is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_typeForDynamicBodies is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_typeForTriggers":
            case "typeForTriggers":
            {
                if (instance.m_typeForTriggers is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_typeForTriggers is TGet intValue)
                {
                    value = intValue;
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
            case "m_bodyFlagsForcingNormalCollisions":
            case "bodyFlagsForcingNormalCollisions":
            {
                if (value is HKLib.hk2019.hknpCollisionFlags.Enum castValue)
                {
                    instance.m_bodyFlagsForcingNormalCollisions = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_bodyFlagsForcingNormalCollisions = (HKLib.hk2019.hknpCollisionFlags.Enum)intValue;
                    return true;
                }
                return false;
            }
            case "m_typeForDynamicBodies":
            case "typeForDynamicBodies":
            {
                if (value is HKLib.hk2019.hknpManifoldType.Enum castValue)
                {
                    instance.m_typeForDynamicBodies = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_typeForDynamicBodies = (HKLib.hk2019.hknpManifoldType.Enum)intValue;
                    return true;
                }
                return false;
            }
            case "m_typeForTriggers":
            case "typeForTriggers":
            {
                if (value is HKLib.hk2019.hknpManifoldType.Enum castValue)
                {
                    instance.m_typeForTriggers = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_typeForTriggers = (HKLib.hk2019.hknpManifoldType.Enum)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
