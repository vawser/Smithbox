// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclClothStateData : HavokData<hclClothState> 
{
    public hclClothStateData(HavokType type, hclClothState instance) : base(type, instance) {}

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
            case "m_operators":
            case "operators":
            {
                if (instance.m_operators is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_usedBuffers":
            case "usedBuffers":
            {
                if (instance.m_usedBuffers is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_usedTransformSets":
            case "usedTransformSets":
            {
                if (instance.m_usedTransformSets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_usedSimCloths":
            case "usedSimCloths":
            {
                if (instance.m_usedSimCloths is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dependencyGraph":
            case "dependencyGraph":
            {
                if (instance.m_dependencyGraph is null)
                {
                    return true;
                }
                if (instance.m_dependencyGraph is TGet castValue)
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
            case "m_operators":
            case "operators":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_operators = castValue;
                return true;
            }
            case "m_usedBuffers":
            case "usedBuffers":
            {
                if (value is not List<hclClothState.BufferAccess> castValue) return false;
                instance.m_usedBuffers = castValue;
                return true;
            }
            case "m_usedTransformSets":
            case "usedTransformSets":
            {
                if (value is not List<hclClothState.TransformSetAccess> castValue) return false;
                instance.m_usedTransformSets = castValue;
                return true;
            }
            case "m_usedSimCloths":
            case "usedSimCloths":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_usedSimCloths = castValue;
                return true;
            }
            case "m_dependencyGraph":
            case "dependencyGraph":
            {
                if (value is null)
                {
                    instance.m_dependencyGraph = default;
                    return true;
                }
                if (value is hclStateDependencyGraph castValue)
                {
                    instance.m_dependencyGraph = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
