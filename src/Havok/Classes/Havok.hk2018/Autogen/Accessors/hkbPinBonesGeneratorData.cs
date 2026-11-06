// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbPinBonesGeneratorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbPinBonesGenerator>
{
    public hkbPinBonesGeneratorData(Havok.hk2018.hkbPinBonesGenerator instance) : base(instance) { }

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
            case "m_referenceFrameGenerator":
            case "referenceFrameGenerator":
            {
                value = instance.m_referenceFrameGenerator;
                return true;
            }
            case "m_pinnedGenerator":
            case "pinnedGenerator":
            {
                value = instance.m_pinnedGenerator;
                return true;
            }
            case "m_boneIndices":
            case "boneIndices":
            {
                value = instance.m_boneIndices;
                return true;
            }
            case "m_fraction":
            case "fraction":
            {
                value = instance.m_fraction;
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
                if (value is Havok.hk2018.hkbGenerator castValue)
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
                if (value is Havok.hk2018.hkbGenerator castValue)
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
                if (value is Havok.hk2018.hkbBoneIndexArray castValue)
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
