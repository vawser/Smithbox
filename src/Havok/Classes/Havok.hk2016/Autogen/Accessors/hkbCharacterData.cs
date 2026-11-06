// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbCharacterData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbCharacter>
{
    public hkbCharacterData(Havok.hk2016.hkbCharacter instance) : base(instance) { }

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
            case "m_nearbyCharacters":
            case "nearbyCharacters":
            {
                value = instance.m_nearbyCharacters;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_currentLod":
            case "currentLod":
            {
                value = instance.m_currentLod;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_setup":
            case "setup":
            {
                value = instance.m_setup;
                return true;
            }
            case "m_behaviorGraph":
            case "behaviorGraph":
            {
                value = instance.m_behaviorGraph;
                return true;
            }
            case "m_projectData":
            case "projectData":
            {
                value = instance.m_projectData;
                return true;
            }
            case "m_capabilities":
            case "capabilities":
            {
                value = instance.m_capabilities;
                return true;
            }
            case "m_effectiveCapabilities":
            case "effectiveCapabilities":
            {
                value = instance.m_effectiveCapabilities;
                return true;
            }
            case "m_deltaTime":
            case "deltaTime":
            {
                value = instance.m_deltaTime;
                return true;
            }
            case "m_useCharactorDeltaTime":
            case "useCharactorDeltaTime":
            {
                value = instance.m_useCharactorDeltaTime;
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
            case "m_nearbyCharacters":
            case "nearbyCharacters":
            {
                if (value is not List<Havok.hk2016.hkbCharacter?> castValue) return false;
                instance.m_nearbyCharacters = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_currentLod":
            case "currentLod":
            {
                if (value is not short castValue) return false;
                instance.m_currentLod = castValue;
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
            case "m_setup":
            case "setup":
            {
                if (value is null)
                {
                    instance.m_setup = default;
                    return true;
                }
                if (value is Havok.hk2016.hkbCharacterSetup castValue)
                {
                    instance.m_setup = castValue;
                    return true;
                }
                return false;
            }
            case "m_behaviorGraph":
            case "behaviorGraph":
            {
                if (value is null)
                {
                    instance.m_behaviorGraph = default;
                    return true;
                }
                if (value is Havok.hk2016.hkbBehaviorGraph castValue)
                {
                    instance.m_behaviorGraph = castValue;
                    return true;
                }
                return false;
            }
            case "m_projectData":
            case "projectData":
            {
                if (value is null)
                {
                    instance.m_projectData = default;
                    return true;
                }
                if (value is Havok.hk2016.hkbProjectData castValue)
                {
                    instance.m_projectData = castValue;
                    return true;
                }
                return false;
            }
            case "m_capabilities":
            case "capabilities":
            {
                if (value is not int castValue) return false;
                instance.m_capabilities = castValue;
                return true;
            }
            case "m_effectiveCapabilities":
            case "effectiveCapabilities":
            {
                if (value is not int castValue) return false;
                instance.m_effectiveCapabilities = castValue;
                return true;
            }
            case "m_deltaTime":
            case "deltaTime":
            {
                if (value is not float castValue) return false;
                instance.m_deltaTime = castValue;
                return true;
            }
            case "m_useCharactorDeltaTime":
            case "useCharactorDeltaTime":
            {
                if (value is not bool castValue) return false;
                instance.m_useCharactorDeltaTime = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
