// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiWorldCharacterStepSerializableContextData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiWorld.CharacterStepSerializableContext>
{
    public hkaiWorldCharacterStepSerializableContextData(Havok.hk2018.hkaiWorld.CharacterStepSerializableContext instance) : base(instance) { }

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
            case "m_callbackType":
            case "callbackType":
            {
                value = instance.m_callbackType;
                return true;
            }
            case "m_timestep":
            case "timestep":
            {
                value = instance.m_timestep;
                return true;
            }
            case "m_characters":
            case "characters":
            {
                value = instance.m_characters;
                return true;
            }
            case "m_localSteeringInputs":
            case "localSteeringInputs":
            {
                value = instance.m_localSteeringInputs;
                return true;
            }
            case "m_obstacleGenerators":
            case "obstacleGenerators":
            {
                value = instance.m_obstacleGenerators;
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
            case "m_callbackType":
            case "callbackType":
            {
                if (value is Havok.hk2018.hkaiWorld.CharacterCallbackType castValue)
                {
                    instance.m_callbackType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_callbackType = (Havok.hk2018.hkaiWorld.CharacterCallbackType)byteValue;
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
                if (value is not List<Havok.hk2018.hkaiCharacter?> castValue) return false;
                instance.m_characters = castValue;
                return true;
            }
            case "m_localSteeringInputs":
            case "localSteeringInputs":
            {
                if (value is not List<Havok.hk2018.hkaiLocalSteeringInput> castValue) return false;
                instance.m_localSteeringInputs = castValue;
                return true;
            }
            case "m_obstacleGenerators":
            case "obstacleGenerators":
            {
                if (value is not List<Havok.hk2018.hkaiObstacleGenerator?> castValue) return false;
                instance.m_obstacleGenerators = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
