// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbLeanRocketboxCharacterControllerInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbLeanRocketboxCharacterControllerInternalState>
{
    public hkbLeanRocketboxCharacterControllerInternalStateData(Havok.hk2014.hkbLeanRocketboxCharacterControllerInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
