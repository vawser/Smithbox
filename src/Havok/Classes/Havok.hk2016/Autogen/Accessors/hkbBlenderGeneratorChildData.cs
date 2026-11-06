// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbBlenderGeneratorChildData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbBlenderGeneratorChild>
{
    public hkbBlenderGeneratorChildData(Havok.hk2016.hkbBlenderGeneratorChild instance) : base(instance) { }

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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                value = instance.m_variableBindingSet;
                return true;
            }
            case "m_generator":
            case "generator":
            {
                value = instance.m_generator;
                return true;
            }
            case "m_boneWeights":
            case "boneWeights":
            {
                value = instance.m_boneWeights;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                value = instance.m_weight;
                return true;
            }
            case "m_worldFromModelWeight":
            case "worldFromModelWeight":
            {
                value = instance.m_worldFromModelWeight;
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
                if (value is not List<Havok.hk2016.hkDefaultPropertyBag> castValue) return false;
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
                if (value is Havok.hk2016.hkbVariableBindingSet castValue)
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
                if (value is Havok.hk2016.hkbGenerator castValue)
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
                if (value is Havok.hk2016.hkbBoneWeightArray castValue)
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
