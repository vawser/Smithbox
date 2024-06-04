// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbDockingGeneratorData : HavokData<hkbDockingGenerator> 
{
    public hkbDockingGeneratorData(HavokType type, hkbDockingGenerator instance) : base(type, instance) {}

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
            case "m_dockingBone":
            case "dockingBone":
            {
                if (instance.m_dockingBone is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_translationOffset":
            case "translationOffset":
            {
                if (instance.m_translationOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rotationOffset":
            case "rotationOffset":
            {
                if (instance.m_rotationOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blendType":
            case "blendType":
            {
                if (instance.m_blendType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_blendType is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_flags is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_child":
            case "child":
            {
                if (instance.m_child is null)
                {
                    return true;
                }
                if (instance.m_child is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_intervalStart":
            case "intervalStart":
            {
                if (instance.m_intervalStart is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_intervalEnd":
            case "intervalEnd":
            {
                if (instance.m_intervalEnd is not TGet castValue) return false;
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
                if (value is hkbDockingGenerator.BlendType castValue)
                {
                    instance.m_blendType = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_blendType = (hkbDockingGenerator.BlendType)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_flags":
            case "flags":
            {
                if (value is hkbDockingGenerator.DockingFlagBits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_flags = (hkbDockingGenerator.DockingFlagBits)ushortValue;
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
                if (value is hkbGenerator castValue)
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
            default:
            return false;
        }
    }

}
