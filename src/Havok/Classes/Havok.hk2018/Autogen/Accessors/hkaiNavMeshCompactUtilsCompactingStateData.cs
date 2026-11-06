// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavMeshCompactUtilsCompactingStateData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavMeshCompactUtils.CompactingState>
{
    public hkaiNavMeshCompactUtilsCompactingStateData(Havok.hk2018.hkaiNavMeshCompactUtils.CompactingState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_section":
            case "section":
            {
                value = instance.m_section;
                return true;
            }
            case "m_phase":
            case "phase":
            {
                value = instance.m_phase;
                return true;
            }
            case "m_readIndex":
            case "readIndex":
            {
                value = instance.m_readIndex;
                return true;
            }
            case "m_writeIndex":
            case "writeIndex":
            {
                value = instance.m_writeIndex;
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
                if (value is Havok.hk2018.hkaiNavMeshCompactUtils.CompactingPhase castValue)
                {
                    instance.m_phase = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_phase = (Havok.hk2018.hkaiNavMeshCompactUtils.CompactingPhase)byteValue;
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
