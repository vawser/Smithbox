// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiWorldCharacterStepSerializableContextData : HavokData<hkaiWorld.CharacterStepSerializableContext> 
{
    public hkaiWorldCharacterStepSerializableContextData(HavokType type, hkaiWorld.CharacterStepSerializableContext instance) : base(type, instance) {}

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
            case "m_callbackType":
            case "callbackType":
            {
                if (instance.m_callbackType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_callbackType is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_timestep":
            case "timestep":
            {
                if (instance.m_timestep is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characters":
            case "characters":
            {
                if (instance.m_characters is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localSteeringInputs":
            case "localSteeringInputs":
            {
                if (instance.m_localSteeringInputs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_obstacleGenerators":
            case "obstacleGenerators":
            {
                if (instance.m_obstacleGenerators is not TGet castValue) return false;
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
            case "m_callbackType":
            case "callbackType":
            {
                if (value is hkaiWorld.CharacterCallbackType castValue)
                {
                    instance.m_callbackType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_callbackType = (hkaiWorld.CharacterCallbackType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_timestep":
            case "timestep":
            {
                if (value is not float castValue) return false;
                instance.m_timestep = castValue;
                return true;
            }
            case "m_characters":
            case "characters":
            {
                if (value is not List<hkaiCharacter?> castValue) return false;
                instance.m_characters = castValue;
                return true;
            }
            case "m_localSteeringInputs":
            case "localSteeringInputs":
            {
                if (value is not List<hkaiLocalSteeringInput> castValue) return false;
                instance.m_localSteeringInputs = castValue;
                return true;
            }
            case "m_obstacleGenerators":
            case "obstacleGenerators":
            {
                if (value is not List<hkaiObstacleGenerator?> castValue) return false;
                instance.m_obstacleGenerators = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
