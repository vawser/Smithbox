// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbLayerData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbLayer>
{
    public hkbLayerData(Havok.hk2018.hkbLayer instance) : base(instance) { }

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
            case "m_useMotion":
            case "useMotion":
            {
                value = instance.m_useMotion;
                return true;
            }
            case "m_blendingControlData":
            case "blendingControlData":
            {
                value = instance.m_blendingControlData;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbVariableBindingSet castValue)
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
                if (value is Havok.hk2018.hkbGenerator castValue)
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
                if (value is Havok.hk2018.hkbBoneWeightArray castValue)
                {
                    instance.m_boneWeights = castValue;
                    return true;
                }
                return false;
            }
            case "m_useMotion":
            case "useMotion":
            {
                if (value is not bool castValue) return false;
                instance.m_useMotion = castValue;
                return true;
            }
            case "m_blendingControlData":
            case "blendingControlData":
            {
                if (value is not Havok.hk2018.hkbEventDrivenBlendingObject castValue) return false;
                instance.m_blendingControlData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
