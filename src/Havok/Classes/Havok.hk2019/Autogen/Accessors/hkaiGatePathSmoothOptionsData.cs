// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiGatePathSmoothOptionsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiGatePath.SmoothOptions>
{
    public hkaiGatePathSmoothOptionsData(Havok.hk2019.hkaiGatePath.SmoothOptions instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_minRounds":
            case "minRounds":
            {
                value = instance.m_minRounds;
                return true;
            }
            case "m_maxRounds":
            case "maxRounds":
            {
                value = instance.m_maxRounds;
                return true;
            }
            case "m_initialMinRounds":
            case "initialMinRounds":
            {
                value = instance.m_initialMinRounds;
                return true;
            }
            case "m_initialMaxRounds":
            case "initialMaxRounds":
            {
                value = instance.m_initialMaxRounds;
                return true;
            }
            case "m_quiescenceDistance":
            case "quiescenceDistance":
            {
                value = instance.m_quiescenceDistance;
                return true;
            }
            case "m_quiescenceSinAngle":
            case "quiescenceSinAngle":
            {
                value = instance.m_quiescenceSinAngle;
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
            case "m_minRounds":
            case "minRounds":
            {
                if (value is not int castValue) return false;
                instance.m_minRounds = castValue;
                return true;
            }
            case "m_maxRounds":
            case "maxRounds":
            {
                if (value is not int castValue) return false;
                instance.m_maxRounds = castValue;
                return true;
            }
            case "m_initialMinRounds":
            case "initialMinRounds":
            {
                if (value is not int castValue) return false;
                instance.m_initialMinRounds = castValue;
                return true;
            }
            case "m_initialMaxRounds":
            case "initialMaxRounds":
            {
                if (value is not int castValue) return false;
                instance.m_initialMaxRounds = castValue;
                return true;
            }
            case "m_quiescenceDistance":
            case "quiescenceDistance":
            {
                if (value is not float castValue) return false;
                instance.m_quiescenceDistance = castValue;
                return true;
            }
            case "m_quiescenceSinAngle":
            case "quiescenceSinAngle":
            {
                if (value is not float castValue) return false;
                instance.m_quiescenceSinAngle = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
