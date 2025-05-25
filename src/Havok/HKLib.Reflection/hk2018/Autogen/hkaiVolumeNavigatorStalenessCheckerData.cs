// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkAtomic;

namespace HKLib.Reflection.hk2018;

internal class hkaiVolumeNavigatorStalenessCheckerData : HavokData<hkaiVolumeNavigatorStalenessChecker> 
{
    public hkaiVolumeNavigatorStalenessCheckerData(HavokType type, hkaiVolumeNavigatorStalenessChecker instance) : base(type, instance) {}

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
            case "m_navigator":
            case "navigator":
            {
                if (instance.m_navigator is null)
                {
                    return true;
                }
                if (instance.m_navigator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_startPoint":
            case "startPoint":
            {
                if (instance.m_startPoint is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_goals":
            case "goals":
            {
                if (instance.m_goals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_settings":
            case "settings":
            {
                if (instance.m_settings is null)
                {
                    return true;
                }
                if (instance.m_settings is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_pathCellKeys":
            case "pathCellKeys":
            {
                if (instance.m_pathCellKeys is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_trailingCellIdx":
            case "trailingCellIdx":
            {
                if (instance.m_trailingCellIdx is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_state":
            case "state":
            {
                if (instance.m_state is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pathRequest":
            case "pathRequest":
            {
                if (instance.m_pathRequest is null)
                {
                    return true;
                }
                if (instance.m_pathRequest is TGet castValue)
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
            case "m_navigator":
            case "navigator":
            {
                if (value is null)
                {
                    instance.m_navigator = default;
                    return true;
                }
                if (value is hkaiVolumeNavigator castValue)
                {
                    instance.m_navigator = castValue;
                    return true;
                }
                return false;
            }
            case "m_startPoint":
            case "startPoint":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_startPoint = castValue;
                return true;
            }
            case "m_goals":
            case "goals":
            {
                if (value is not List<hkaiVolumeNavigator.Goal> castValue) return false;
                instance.m_goals = castValue;
                return true;
            }
            case "m_settings":
            case "settings":
            {
                if (value is null)
                {
                    instance.m_settings = default;
                    return true;
                }
                if (value is hkaiVolumeNavigator.NavigatorSettings castValue)
                {
                    instance.m_settings = castValue;
                    return true;
                }
                return false;
            }
            case "m_pathCellKeys":
            case "pathCellKeys":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_pathCellKeys = castValue;
                return true;
            }
            case "m_trailingCellIdx":
            case "trailingCellIdx":
            {
                if (value is not int castValue) return false;
                instance.m_trailingCellIdx = castValue;
                return true;
            }
            case "m_state":
            case "state":
            {
                if (value is not Variable<hkaiVolumeNavigatorStalenessChecker.State> castValue) return false;
                instance.m_state = castValue;
                return true;
            }
            case "m_pathRequest":
            case "pathRequest":
            {
                if (value is null)
                {
                    instance.m_pathRequest = default;
                    return true;
                }
                if (value is hkaiVolumeNavigator.PathRequest castValue)
                {
                    instance.m_pathRequest = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
