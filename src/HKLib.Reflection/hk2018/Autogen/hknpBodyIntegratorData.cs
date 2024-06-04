// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hknpManifoldType;
using Enum = HKLib.hk2018.hknpManifoldType.Enum;

namespace HKLib.Reflection.hk2018;

internal class hknpBodyIntegratorData : HavokData<hknpBodyIntegrator> 
{
    public hknpBodyIntegratorData(HavokType type, hknpBodyIntegrator instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bodyFlagsForcingNormalCollisions":
            case "bodyFlagsForcingNormalCollisions":
            {
                if (instance.m_bodyFlagsForcingNormalCollisions is not TGet castValue) return false;
                value = castValue;
                return true;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_bodyFlagsForcingNormalCollisions":
            case "bodyFlagsForcingNormalCollisions":
            {
                if (value is not uint castValue) return false;
                instance.m_bodyFlagsForcingNormalCollisions = castValue;
                return true;
            }
            case "m_typeForDynamicBodies":
            case "typeForDynamicBodies":
            {
                if (value is Enum castValue)
                {
                    instance.m_typeForDynamicBodies = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_typeForDynamicBodies = (Enum)intValue;
                    return true;
                }
                return false;
            }
            case "m_typeForTriggers":
            case "typeForTriggers":
            {
                if (value is Enum castValue)
                {
                    instance.m_typeForTriggers = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_typeForTriggers = (Enum)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
