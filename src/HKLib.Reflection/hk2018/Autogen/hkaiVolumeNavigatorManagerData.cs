// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiVolumeNavigatorManagerData : HavokData<hkaiVolumeNavigatorManager> 
{
    public hkaiVolumeNavigatorManagerData(HavokType type, hkaiVolumeNavigatorManager instance) : base(type, instance) {}

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
            case "m_world":
            case "world":
            {
                if (instance.m_world is null)
                {
                    return true;
                }
                if (instance.m_world is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_navigators":
            case "navigators":
            {
                if (instance.m_navigators is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_modifiedSections":
            case "modifiedSections":
            {
                if (instance.m_modifiedSections is null)
                {
                    return true;
                }
                if (instance.m_modifiedSections is TGet castValue)
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
            case "m_world":
            case "world":
            {
                if (value is null)
                {
                    instance.m_world = default;
                    return true;
                }
                if (value is hkaiWorld castValue)
                {
                    instance.m_world = castValue;
                    return true;
                }
                return false;
            }
            case "m_navigators":
            case "navigators":
            {
                if (value is not List<hkaiVolumeNavigator?> castValue) return false;
                instance.m_navigators = castValue;
                return true;
            }
            case "m_modifiedSections":
            case "modifiedSections":
            {
                if (value is null)
                {
                    instance.m_modifiedSections = default;
                    return true;
                }
                if (value is hkaiModifiedSections castValue)
                {
                    instance.m_modifiedSections = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
