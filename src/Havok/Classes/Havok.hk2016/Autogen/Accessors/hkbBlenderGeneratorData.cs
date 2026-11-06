// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbBlenderGeneratorData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbBlenderGenerator>
{
    public hkbBlenderGeneratorData(Havok.hk2016.hkbBlenderGenerator instance) : base(instance) { }

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
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_referencePoseWeightThreshold":
            case "referencePoseWeightThreshold":
            {
                value = instance.m_referencePoseWeightThreshold;
                return true;
            }
            case "m_blendParameter":
            case "blendParameter":
            {
                value = instance.m_blendParameter;
                return true;
            }
            case "m_minCyclicBlendParameter":
            case "minCyclicBlendParameter":
            {
                value = instance.m_minCyclicBlendParameter;
                return true;
            }
            case "m_maxCyclicBlendParameter":
            case "maxCyclicBlendParameter":
            {
                value = instance.m_maxCyclicBlendParameter;
                return true;
            }
            case "m_indexOfSyncMasterChild":
            case "indexOfSyncMasterChild":
            {
                value = instance.m_indexOfSyncMasterChild;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_subtractLastChild":
            case "subtractLastChild":
            {
                value = instance.m_subtractLastChild;
                return true;
            }
            case "m_children":
            case "children":
            {
                value = instance.m_children;
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
                if (value is not List<Havok.hk2016.hkbBlenderGeneratorChild?> castValue) return false;
                instance.m_children = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
