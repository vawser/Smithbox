// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbProjectStringDataData : HavokData<hkbProjectStringData> 
{
    public hkbProjectStringDataData(HavokType type, hkbProjectStringData instance) : base(type, instance) {}

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
            case "m_behaviorFilenames":
            case "behaviorFilenames":
            {
                if (instance.m_behaviorFilenames is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characterFilenames":
            case "characterFilenames":
            {
                if (instance.m_characterFilenames is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_eventNames":
            case "eventNames":
            {
                if (instance.m_eventNames is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_animationPath":
            case "animationPath":
            {
                if (instance.m_animationPath is null)
                {
                    return true;
                }
                if (instance.m_animationPath is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_behaviorPath":
            case "behaviorPath":
            {
                if (instance.m_behaviorPath is null)
                {
                    return true;
                }
                if (instance.m_behaviorPath is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_characterPath":
            case "characterPath":
            {
                if (instance.m_characterPath is null)
                {
                    return true;
                }
                if (instance.m_characterPath is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_scriptsPath":
            case "scriptsPath":
            {
                if (instance.m_scriptsPath is null)
                {
                    return true;
                }
                if (instance.m_scriptsPath is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_fullPathToSource":
            case "fullPathToSource":
            {
                if (instance.m_fullPathToSource is null)
                {
                    return true;
                }
                if (instance.m_fullPathToSource is TGet castValue)
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
            case "m_behaviorFilenames":
            case "behaviorFilenames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_behaviorFilenames = castValue;
                return true;
            }
            case "m_characterFilenames":
            case "characterFilenames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_characterFilenames = castValue;
                return true;
            }
            case "m_eventNames":
            case "eventNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_eventNames = castValue;
                return true;
            }
            case "m_animationPath":
            case "animationPath":
            {
                if (value is null)
                {
                    instance.m_animationPath = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_animationPath = castValue;
                    return true;
                }
                return false;
            }
            case "m_behaviorPath":
            case "behaviorPath":
            {
                if (value is null)
                {
                    instance.m_behaviorPath = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_behaviorPath = castValue;
                    return true;
                }
                return false;
            }
            case "m_characterPath":
            case "characterPath":
            {
                if (value is null)
                {
                    instance.m_characterPath = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_characterPath = castValue;
                    return true;
                }
                return false;
            }
            case "m_scriptsPath":
            case "scriptsPath":
            {
                if (value is null)
                {
                    instance.m_scriptsPath = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_scriptsPath = castValue;
                    return true;
                }
                return false;
            }
            case "m_fullPathToSource":
            case "fullPathToSource":
            {
                if (value is null)
                {
                    instance.m_fullPathToSource = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_fullPathToSource = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
