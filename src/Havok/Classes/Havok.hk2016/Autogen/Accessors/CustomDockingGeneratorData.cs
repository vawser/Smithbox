// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class CustomDockingGeneratorData : Havok.Formats.HavokFieldAccess<Havok.hk2016.CustomDockingGenerator>
{
    public CustomDockingGeneratorData(Havok.hk2016.CustomDockingGenerator instance) : base(instance) { }

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
            case "m_dockingBone":
            case "dockingBone":
            {
                value = instance.m_dockingBone;
                return true;
            }
            case "m_translationOffset":
            case "translationOffset":
            {
                value = instance.m_translationOffset;
                return true;
            }
            case "m_rotationOffset":
            case "rotationOffset":
            {
                value = instance.m_rotationOffset;
                return true;
            }
            case "m_blendType":
            case "blendType":
            {
                value = instance.m_blendType;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_child":
            case "child":
            {
                value = instance.m_child;
                return true;
            }
            case "m_intervalStart":
            case "intervalStart":
            {
                value = instance.m_intervalStart;
                return true;
            }
            case "m_intervalEnd":
            case "intervalEnd":
            {
                value = instance.m_intervalEnd;
                return true;
            }
            case "m_titleDokingType":
            case "titleDokingType":
            {
                value = instance.m_titleDokingType;
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
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
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
            case "m_dockingBone":
            case "dockingBone":
            {
                if (value is not short castValue) return false;
                instance.m_dockingBone = castValue;
                return true;
            }
            case "m_translationOffset":
            case "translationOffset":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_translationOffset = castValue;
                return true;
            }
            case "m_rotationOffset":
            case "rotationOffset":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_rotationOffset = castValue;
                return true;
            }
            case "m_blendType":
            case "blendType":
            {
                if (value is Havok.hk2016.hkbDockingGenerator.BlendType castValue)
                {
                    instance.m_blendType = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_blendType = (Havok.hk2016.hkbDockingGenerator.BlendType)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2016.hkbDockingGenerator.DockingFlagBits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_flags = (Havok.hk2016.hkbDockingGenerator.DockingFlagBits)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_child":
            case "child":
            {
                if (value is null)
                {
                    instance.m_child = default;
                    return true;
                }
                if (value is Havok.hk2016.hkbGenerator castValue)
                {
                    instance.m_child = castValue;
                    return true;
                }
                return false;
            }
            case "m_intervalStart":
            case "intervalStart":
            {
                if (value is not int castValue) return false;
                instance.m_intervalStart = castValue;
                return true;
            }
            case "m_intervalEnd":
            case "intervalEnd":
            {
                if (value is not int castValue) return false;
                instance.m_intervalEnd = castValue;
                return true;
            }
            case "m_titleDokingType":
            case "titleDokingType":
            {
                if (value is not int castValue) return false;
                instance.m_titleDokingType = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
