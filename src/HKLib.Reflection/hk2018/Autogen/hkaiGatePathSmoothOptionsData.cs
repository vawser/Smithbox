// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiGatePathSmoothOptionsData : HavokData<hkaiGatePath.SmoothOptions> 
{
    public hkaiGatePathSmoothOptionsData(HavokType type, hkaiGatePath.SmoothOptions instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_minRounds":
            case "minRounds":
            {
                if (instance.m_minRounds is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxRounds":
            case "maxRounds":
            {
                if (instance.m_maxRounds is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_initialMinRounds":
            case "initialMinRounds":
            {
                if (instance.m_initialMinRounds is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_initialMaxRounds":
            case "initialMaxRounds":
            {
                if (instance.m_initialMaxRounds is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_quiescenceDistance":
            case "quiescenceDistance":
            {
                if (instance.m_quiescenceDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_quiescenceSinAngle":
            case "quiescenceSinAngle":
            {
                if (instance.m_quiescenceSinAngle is not TGet castValue) return false;
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
