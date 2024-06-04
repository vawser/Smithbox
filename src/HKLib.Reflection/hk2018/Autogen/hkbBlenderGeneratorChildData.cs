// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbBlenderGeneratorChildData : HavokData<hkbBlenderGeneratorChild> 
{
    public hkbBlenderGeneratorChildData(HavokType type, hkbBlenderGeneratorChild instance) : base(type, instance) {}

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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (instance.m_variableBindingSet is null)
                {
                    return true;
                }
                if (instance.m_variableBindingSet is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_generator":
            case "generator":
            {
                if (instance.m_generator is null)
                {
                    return true;
                }
                if (instance.m_generator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneWeights":
            case "boneWeights":
            {
                if (instance.m_boneWeights is null)
                {
                    return true;
                }
                if (instance.m_boneWeights is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_weight":
            case "weight":
            {
                if (instance.m_weight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_worldFromModelWeight":
            case "worldFromModelWeight":
            {
                if (instance.m_worldFromModelWeight is not TGet castValue) return false;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is hkbVariableBindingSet castValue)
                {
                    instance.m_variableBindingSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_generator":
            case "generator":
            {
                if (value is null)
                {
                    instance.m_generator = default;
                    return true;
                }
                if (value is hkbGenerator castValue)
                {
                    instance.m_generator = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneWeights":
            case "boneWeights":
            {
                if (value is null)
                {
                    instance.m_boneWeights = default;
                    return true;
                }
                if (value is hkbBoneWeightArray castValue)
                {
                    instance.m_boneWeights = castValue;
                    return true;
                }
                return false;
            }
            case "m_weight":
            case "weight":
            {
                if (value is not float castValue) return false;
                instance.m_weight = castValue;
                return true;
            }
            case "m_worldFromModelWeight":
            case "worldFromModelWeight":
            {
                if (value is not float castValue) return false;
                instance.m_worldFromModelWeight = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
