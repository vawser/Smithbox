// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiViewerNodesCharacterData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiViewerNodes.Character>
{
    public hkaiViewerNodesCharacterData(Havok.hk2019.hkaiViewerNodes.Character instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_index":
            case "index":
            {
                value = instance.m_index;
                return true;
            }
            case "m_enabledIndex":
            case "enabledIndex":
            {
                value = instance.m_enabledIndex;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                value = instance.m_radius;
                return true;
            }
            case "m_maximumSpeed":
            case "maximumSpeed":
            {
                value = instance.m_maximumSpeed;
                return true;
            }
            case "m_preferredSpeed":
            case "preferredSpeed":
            {
                value = instance.m_preferredSpeed;
                return true;
            }
            case "m_sensorSize":
            case "sensorSize":
            {
                value = instance.m_sensorSize;
                return true;
            }
            case "m_maximumAvoidanceCharacters":
            case "maximumAvoidanceCharacters":
            {
                value = instance.m_maximumAvoidanceCharacters;
                return true;
            }
            case "m_steeringProperties":
            case "steeringProperties":
            {
                value = instance.m_steeringProperties;
                return true;
            }
            case "m_position":
            case "position":
            {
                value = instance.m_position;
                return true;
            }
            case "m_currentVelocity":
            case "currentVelocity":
            {
                value = instance.m_currentVelocity;
                return true;
            }
            case "m_surfaceVelocity":
            case "surfaceVelocity":
            {
                value = instance.m_surfaceVelocity;
                return true;
            }
            case "m_steeringEnabled":
            case "steeringEnabled":
            {
                value = instance.m_steeringEnabled;
                return true;
            }
            case "m_desiredDirection":
            case "desiredDirection":
            {
                value = instance.m_desiredDirection;
                return true;
            }
            case "m_avoidanceVelocity":
            case "avoidanceVelocity":
            {
                value = instance.m_avoidanceVelocity;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
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
            case "m_index":
            case "index":
            {
                if (value is not int castValue) return false;
                instance.m_index = castValue;
                return true;
            }
            case "m_enabledIndex":
            case "enabledIndex":
            {
                if (value is not int castValue) return false;
                instance.m_enabledIndex = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (value is not float castValue) return false;
                instance.m_radius = castValue;
                return true;
            }
            case "m_maximumSpeed":
            case "maximumSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_maximumSpeed = castValue;
                return true;
            }
            case "m_preferredSpeed":
            case "preferredSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_preferredSpeed = castValue;
                return true;
            }
            case "m_sensorSize":
            case "sensorSize":
            {
                if (value is not Havok.hk2019.hkaiCollisionAvoidance.Character.SensorSize castValue) return false;
                instance.m_sensorSize = castValue;
                return true;
            }
            case "m_maximumAvoidanceCharacters":
            case "maximumAvoidanceCharacters":
            {
                if (value is not int castValue) return false;
                instance.m_maximumAvoidanceCharacters = castValue;
                return true;
            }
            case "m_steeringProperties":
            case "steeringProperties":
            {
                if (value is not Havok.hk2019.hkaiCollisionAvoidance.SteeringProperties castValue) return false;
                instance.m_steeringProperties = castValue;
                return true;
            }
            case "m_position":
            case "position":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_position = castValue;
                return true;
            }
            case "m_currentVelocity":
            case "currentVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_currentVelocity = castValue;
                return true;
            }
            case "m_surfaceVelocity":
            case "surfaceVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_surfaceVelocity = castValue;
                return true;
            }
            case "m_steeringEnabled":
            case "steeringEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_steeringEnabled = castValue;
                return true;
            }
            case "m_desiredDirection":
            case "desiredDirection":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_desiredDirection = castValue;
                return true;
            }
            case "m_avoidanceVelocity":
            case "avoidanceVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_avoidanceVelocity = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
