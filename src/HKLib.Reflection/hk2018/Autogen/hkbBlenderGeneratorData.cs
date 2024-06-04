// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbBlenderGeneratorData : HavokData<hkbBlenderGenerator> 
{
    public hkbBlenderGeneratorData(HavokType type, hkbBlenderGenerator instance) : base(type, instance) {}

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
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
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
            case "m_referencePoseWeightThreshold":
            case "referencePoseWeightThreshold":
            {
                if (instance.m_referencePoseWeightThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blendParameter":
            case "blendParameter":
            {
                if (instance.m_blendParameter is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minCyclicBlendParameter":
            case "minCyclicBlendParameter":
            {
                if (instance.m_minCyclicBlendParameter is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxCyclicBlendParameter":
            case "maxCyclicBlendParameter":
            {
                if (instance.m_maxCyclicBlendParameter is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_indexOfSyncMasterChild":
            case "indexOfSyncMasterChild":
            {
                if (instance.m_indexOfSyncMasterChild is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_subtractLastChild":
            case "subtractLastChild":
            {
                if (instance.m_subtractLastChild is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_children":
            case "children":
            {
                if (instance.m_children is not TGet castValue) return false;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
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
            case "m_referencePoseWeightThreshold":
            case "referencePoseWeightThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_referencePoseWeightThreshold = castValue;
                return true;
            }
            case "m_blendParameter":
            case "blendParameter":
            {
                if (value is not float castValue) return false;
                instance.m_blendParameter = castValue;
                return true;
            }
            case "m_minCyclicBlendParameter":
            case "minCyclicBlendParameter":
            {
                if (value is not float castValue) return false;
                instance.m_minCyclicBlendParameter = castValue;
                return true;
            }
            case "m_maxCyclicBlendParameter":
            case "maxCyclicBlendParameter":
            {
                if (value is not float castValue) return false;
                instance.m_maxCyclicBlendParameter = castValue;
                return true;
            }
            case "m_indexOfSyncMasterChild":
            case "indexOfSyncMasterChild":
            {
                if (value is not short castValue) return false;
                instance.m_indexOfSyncMasterChild = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is not short castValue) return false;
                instance.m_flags = castValue;
                return true;
            }
            case "m_subtractLastChild":
            case "subtractLastChild":
            {
                if (value is not bool castValue) return false;
                instance.m_subtractLastChild = castValue;
                return true;
            }
            case "m_children":
            case "children":
            {
                if (value is not List<hkbBlenderGeneratorChild?> castValue) return false;
                instance.m_children = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
