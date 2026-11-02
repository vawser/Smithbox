// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiViewerNodesCharacterData : HavokData<HKLib.hk2019.hkaiViewerNodes.Character> 
{
    public hkaiViewerNodesCharacterData(HavokType type, HKLib.hk2019.hkaiViewerNodes.Character instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_index":
            case "index":
            {
                if (instance.m_index is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enabledIndex":
            case "enabledIndex":
            {
                if (instance.m_enabledIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (instance.m_radius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maximumSpeed":
            case "maximumSpeed":
            {
                if (instance.m_maximumSpeed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_preferredSpeed":
            case "preferredSpeed":
            {
                if (instance.m_preferredSpeed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sensorSize":
            case "sensorSize":
            {
                if (instance.m_sensorSize is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maximumAvoidanceCharacters":
            case "maximumAvoidanceCharacters":
            {
                if (instance.m_maximumAvoidanceCharacters is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_steeringProperties":
            case "steeringProperties":
            {
                if (instance.m_steeringProperties is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_position":
            case "position":
            {
                if (instance.m_position is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_currentVelocity":
            case "currentVelocity":
            {
                if (instance.m_currentVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_surfaceVelocity":
            case "surfaceVelocity":
            {
                if (instance.m_surfaceVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_steeringEnabled":
            case "steeringEnabled":
            {
                if (instance.m_steeringEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_desiredDirection":
            case "desiredDirection":
            {
                if (instance.m_desiredDirection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_avoidanceVelocity":
            case "avoidanceVelocity":
            {
                if (instance.m_avoidanceVelocity is not TGet castValue) return false;
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
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
                if (value is not HKLib.hk2019.hkaiCollisionAvoidance.Character.SensorSize castValue) return false;
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
                if (value is not HKLib.hk2019.hkaiCollisionAvoidance.SteeringProperties castValue) return false;
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
