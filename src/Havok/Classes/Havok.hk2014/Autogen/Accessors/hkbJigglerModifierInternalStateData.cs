// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbJigglerModifierInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbJigglerModifierInternalState>
{
    public hkbJigglerModifierInternalStateData(Havok.hk2014.hkbJigglerModifierInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_currentVelocitiesWS":
            case "currentVelocitiesWS":
            {
                value = instance.m_currentVelocitiesWS;
                return true;
            }
            case "m_currentPositions":
            case "currentPositions":
            {
                value = instance.m_currentPositions;
                return true;
            }
            case "m_timeStep":
            case "timeStep":
            {
                value = instance.m_timeStep;
                return true;
            }
            case "m_initNextModify":
            case "initNextModify":
            {
                value = instance.m_initNextModify;
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
            case "m_currentVelocitiesWS":
            case "currentVelocitiesWS":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_currentVelocitiesWS = castValue;
                return true;
            }
            case "m_currentPositions":
            case "currentPositions":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_currentPositions = castValue;
                return true;
            }
            case "m_timeStep":
            case "timeStep":
            {
                if (value is not float castValue) return false;
                instance.m_timeStep = castValue;
                return true;
            }
            case "m_initNextModify":
            case "initNextModify":
            {
                if (value is not bool castValue) return false;
                instance.m_initNextModify = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
