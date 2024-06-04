// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshCompactUtilsCompactingStateData : HavokData<hkaiNavMeshCompactUtils.CompactingState> 
{
    public hkaiNavMeshCompactUtilsCompactingStateData(HavokType type, hkaiNavMeshCompactUtils.CompactingState instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_section":
            case "section":
            {
                if (instance.m_section is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_phase":
            case "phase":
            {
                if (instance.m_phase is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_phase is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_readIndex":
            case "readIndex":
            {
                if (instance.m_readIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_writeIndex":
            case "writeIndex":
            {
                if (instance.m_writeIndex is not TGet castValue) return false;
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
            case "m_section":
            case "section":
            {
                if (value is not int castValue) return false;
                instance.m_section = castValue;
                return true;
            }
            case "m_phase":
            case "phase":
            {
                if (value is hkaiNavMeshCompactUtils.CompactingPhase castValue)
                {
                    instance.m_phase = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_phase = (hkaiNavMeshCompactUtils.CompactingPhase)byteValue;
                    return true;
                }
                return false;
            }
            case "m_readIndex":
            case "readIndex":
            {
                if (value is not int castValue) return false;
                instance.m_readIndex = castValue;
                return true;
            }
            case "m_writeIndex":
            case "writeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_writeIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
