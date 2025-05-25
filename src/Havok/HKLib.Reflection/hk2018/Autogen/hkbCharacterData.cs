// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbCharacterData : HavokData<hkbCharacter> 
{
    public hkbCharacterData(HavokType type, hkbCharacter instance) : base(type, instance) {}

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
            case "m_nearbyCharacters":
            case "nearbyCharacters":
            {
                if (instance.m_nearbyCharacters is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_currentLod":
            case "currentLod":
            {
                if (instance.m_currentLod is not TGet castValue) return false;
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
            case "m_setup":
            case "setup":
            {
                if (instance.m_setup is null)
                {
                    return true;
                }
                if (instance.m_setup is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_behaviorGraph":
            case "behaviorGraph":
            {
                if (instance.m_behaviorGraph is null)
                {
                    return true;
                }
                if (instance.m_behaviorGraph is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_projectData":
            case "projectData":
            {
                if (instance.m_projectData is null)
                {
                    return true;
                }
                if (instance.m_projectData is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_capabilities":
            case "capabilities":
            {
                if (instance.m_capabilities is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_effectiveCapabilities":
            case "effectiveCapabilities":
            {
                if (instance.m_effectiveCapabilities is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_deltaTime":
            case "deltaTime":
            {
                if (instance.m_deltaTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useCharactorDeltaTime":
            case "useCharactorDeltaTime":
            {
                if (instance.m_useCharactorDeltaTime is not TGet castValue) return false;
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
            case "m_nearbyCharacters":
            case "nearbyCharacters":
            {
                if (value is not List<hkbCharacter?> castValue) return false;
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
                if (value is hkbCharacterSetup castValue)
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
                if (value is hkbBehaviorGraph castValue)
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
                if (value is hkbProjectData castValue)
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
