// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbPinBonesGeneratorData : HavokData<hkbPinBonesGenerator> 
{
    public hkbPinBonesGeneratorData(HavokType type, hkbPinBonesGenerator instance) : base(type, instance) {}

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
            case "m_referenceFrameGenerator":
            case "referenceFrameGenerator":
            {
                if (instance.m_referenceFrameGenerator is null)
                {
                    return true;
                }
                if (instance.m_referenceFrameGenerator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_pinnedGenerator":
            case "pinnedGenerator":
            {
                if (instance.m_pinnedGenerator is null)
                {
                    return true;
                }
                if (instance.m_pinnedGenerator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneIndices":
            case "boneIndices":
            {
                if (instance.m_boneIndices is null)
                {
                    return true;
                }
                if (instance.m_boneIndices is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_fraction":
            case "fraction":
            {
                if (instance.m_fraction is not TGet castValue) return false;
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
            case "m_referenceFrameGenerator":
            case "referenceFrameGenerator":
            {
                if (value is null)
                {
                    instance.m_referenceFrameGenerator = default;
                    return true;
                }
                if (value is hkbGenerator castValue)
                {
                    instance.m_referenceFrameGenerator = castValue;
                    return true;
                }
                return false;
            }
            case "m_pinnedGenerator":
            case "pinnedGenerator":
            {
                if (value is null)
                {
                    instance.m_pinnedGenerator = default;
                    return true;
                }
                if (value is hkbGenerator castValue)
                {
                    instance.m_pinnedGenerator = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneIndices":
            case "boneIndices":
            {
                if (value is null)
                {
                    instance.m_boneIndices = default;
                    return true;
                }
                if (value is hkbBoneIndexArray castValue)
                {
                    instance.m_boneIndices = castValue;
                    return true;
                }
                return false;
            }
            case "m_fraction":
            case "fraction":
            {
                if (value is not float castValue) return false;
                instance.m_fraction = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
