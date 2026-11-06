// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbProjectStringDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbProjectStringData>
{
    public hkbProjectStringDataData(Havok.hk2018.hkbProjectStringData instance) : base(instance) { }

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
            case "m_behaviorFilenames":
            case "behaviorFilenames":
            {
                value = instance.m_behaviorFilenames;
                return true;
            }
            case "m_characterFilenames":
            case "characterFilenames":
            {
                value = instance.m_characterFilenames;
                return true;
            }
            case "m_eventNames":
            case "eventNames":
            {
                value = instance.m_eventNames;
                return true;
            }
            case "m_animationPath":
            case "animationPath":
            {
                value = instance.m_animationPath;
                return true;
            }
            case "m_behaviorPath":
            case "behaviorPath":
            {
                value = instance.m_behaviorPath;
                return true;
            }
            case "m_characterPath":
            case "characterPath":
            {
                value = instance.m_characterPath;
                return true;
            }
            case "m_scriptsPath":
            case "scriptsPath":
            {
                value = instance.m_scriptsPath;
                return true;
            }
            case "m_fullPathToSource":
            case "fullPathToSource":
            {
                value = instance.m_fullPathToSource;
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
