// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbSetBehaviorCommandData : HavokData<hkbSetBehaviorCommand> 
{
    public hkbSetBehaviorCommandData(HavokType type, hkbSetBehaviorCommand instance) : base(type, instance) {}

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
            case "m_characterId":
            case "characterId":
            {
                if (instance.m_characterId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_behavior":
            case "behavior":
            {
                if (instance.m_behavior is null)
                {
                    return true;
                }
                if (instance.m_behavior is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_rootGenerator":
            case "rootGenerator":
            {
                if (instance.m_rootGenerator is null)
                {
                    return true;
                }
                if (instance.m_rootGenerator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_referencedBehaviors":
            case "referencedBehaviors":
            {
                if (instance.m_referencedBehaviors is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_startStateIndex":
            case "startStateIndex":
            {
                if (instance.m_startStateIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_randomizeSimulation":
            case "randomizeSimulation":
            {
                if (instance.m_randomizeSimulation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_padding":
            case "padding":
            {
                if (instance.m_padding is not TGet castValue) return false;
                value = castValue;
                return true;
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
            case "m_characterId":
            case "characterId":
            {
                if (value is not ulong castValue) return false;
                instance.m_characterId = castValue;
                return true;
            }
            case "m_behavior":
            case "behavior":
            {
                if (value is null)
                {
                    instance.m_behavior = default;
                    return true;
                }
                if (value is hkbBehaviorGraph castValue)
                {
                    instance.m_behavior = castValue;
                    return true;
                }
                return false;
            }
            case "m_rootGenerator":
            case "rootGenerator":
            {
                if (value is null)
                {
                    instance.m_rootGenerator = default;
                    return true;
                }
                if (value is hkbGenerator castValue)
                {
                    instance.m_rootGenerator = castValue;
                    return true;
                }
                return false;
            }
            case "m_referencedBehaviors":
            case "referencedBehaviors":
            {
                if (value is not List<hkbBehaviorGraph?> castValue) return false;
                instance.m_referencedBehaviors = castValue;
                return true;
            }
            case "m_startStateIndex":
            case "startStateIndex":
            {
                if (value is not int castValue) return false;
                instance.m_startStateIndex = castValue;
                return true;
            }
            case "m_randomizeSimulation":
            case "randomizeSimulation":
            {
                if (value is not bool castValue) return false;
                instance.m_randomizeSimulation = castValue;
                return true;
            }
            case "m_padding":
            case "padding":
            {
                if (value is not int castValue) return false;
                instance.m_padding = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
