// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbRocketboxCharacterControllerInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbRocketboxCharacterControllerInternalState>
{
    public hkbRocketboxCharacterControllerInternalStateData(Havok.hk2014.hkbRocketboxCharacterControllerInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rapidTurnRequest":
            case "rapidTurnRequest":
            {
                value = instance.m_rapidTurnRequest;
                return true;
            }
            case "m_currPose":
            case "currPose":
            {
                value = instance.m_currPose;
                return true;
            }
            case "m_prevPose":
            case "prevPose":
            {
                value = instance.m_prevPose;
                return true;
            }
            case "m_noVelocityTimer":
            case "noVelocityTimer":
            {
                value = instance.m_noVelocityTimer;
                return true;
            }
            case "m_linearSpeedModifier":
            case "linearSpeedModifier":
            {
                value = instance.m_linearSpeedModifier;
                return true;
            }
            case "m_characterAngle":
            case "characterAngle":
            {
                value = instance.m_characterAngle;
                return true;
            }
            case "m_plantedFootIdx":
            case "plantedFootIdx":
            {
                value = instance.m_plantedFootIdx;
                return true;
            }
            case "m_timeStep":
            case "timeStep":
            {
                value = instance.m_timeStep;
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
            case "m_rapidTurnRequest":
            case "rapidTurnRequest":
            {
                if (value is not bool castValue) return false;
                instance.m_rapidTurnRequest = castValue;
                return true;
            }
            case "m_currPose":
            case "currPose":
            {
                if (value is not int castValue) return false;
                instance.m_currPose = castValue;
                return true;
            }
            case "m_prevPose":
            case "prevPose":
            {
                if (value is not int castValue) return false;
                instance.m_prevPose = castValue;
                return true;
            }
            case "m_noVelocityTimer":
            case "noVelocityTimer":
            {
                if (value is not float castValue) return false;
                instance.m_noVelocityTimer = castValue;
                return true;
            }
            case "m_linearSpeedModifier":
            case "linearSpeedModifier":
            {
                if (value is not float castValue) return false;
                instance.m_linearSpeedModifier = castValue;
                return true;
            }
            case "m_characterAngle":
            case "characterAngle":
            {
                if (value is not float castValue) return false;
                instance.m_characterAngle = castValue;
                return true;
            }
            case "m_plantedFootIdx":
            case "plantedFootIdx":
            {
                if (value is not int castValue) return false;
                instance.m_plantedFootIdx = castValue;
                return true;
            }
            case "m_timeStep":
            case "timeStep":
            {
                if (value is not float castValue) return false;
                instance.m_timeStep = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
