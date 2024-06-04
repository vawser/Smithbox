// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbContextData : HavokData<hkbContext> 
{
    public hkbContextData(HavokType type, hkbContext instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rootBehavior":
            case "rootBehavior":
            {
                if (instance.m_rootBehavior is null)
                {
                    return true;
                }
                if (instance.m_rootBehavior is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_generatorOutputListener":
            case "generatorOutputListener":
            {
                if (instance.m_generatorOutputListener is null)
                {
                    return true;
                }
                if (instance.m_generatorOutputListener is TGet castValue)
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
            case "m_rootBehavior":
            case "rootBehavior":
            {
                if (value is null)
                {
                    instance.m_rootBehavior = default;
                    return true;
                }
                if (value is hkbBehaviorGraph castValue)
                {
                    instance.m_rootBehavior = castValue;
                    return true;
                }
                return false;
            }
            case "m_generatorOutputListener":
            case "generatorOutputListener":
            {
                if (value is null)
                {
                    instance.m_generatorOutputListener = default;
                    return true;
                }
                if (value is hkbGeneratorOutputListener castValue)
                {
                    instance.m_generatorOutputListener = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
