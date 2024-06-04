// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclMoveFixedParticlesSetupObjectData : HavokData<hclMoveFixedParticlesSetupObject> 
{
    public hclMoveFixedParticlesSetupObjectData(HavokType type, hclMoveFixedParticlesSetupObject instance) : base(type, instance) {}

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
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_simClothSetupObject":
            case "simClothSetupObject":
            {
                if (instance.m_simClothSetupObject is null)
                {
                    return true;
                }
                if (instance.m_simClothSetupObject is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_displayBufferSetup":
            case "displayBufferSetup":
            {
                if (instance.m_displayBufferSetup is null)
                {
                    return true;
                }
                if (instance.m_displayBufferSetup is TGet castValue)
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_simClothSetupObject":
            case "simClothSetupObject":
            {
                if (value is null)
                {
                    instance.m_simClothSetupObject = default;
                    return true;
                }
                if (value is hclSimClothSetupObject castValue)
                {
                    instance.m_simClothSetupObject = castValue;
                    return true;
                }
                return false;
            }
            case "m_displayBufferSetup":
            case "displayBufferSetup":
            {
                if (value is null)
                {
                    instance.m_displayBufferSetup = default;
                    return true;
                }
                if (value is hclBufferSetupObject castValue)
                {
                    instance.m_displayBufferSetup = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
