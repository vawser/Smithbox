// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiGatePathData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiGatePath>
{
    public hkaiGatePathData(Havok.hk2019.hkaiGatePath instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_gates":
            case "gates":
            {
                value = instance.m_gates;
                return true;
            }
            case "m_schedule":
            case "schedule":
            {
                value = instance.m_schedule;
                return true;
            }
            case "m_needsInitialSmooth":
            case "needsInitialSmooth":
            {
                value = instance.m_needsInitialSmooth;
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
            case "m_gates":
            case "gates":
            {
                if (value is not List<Havok.hk2019.hkaiGatePath.PathGate> castValue) return false;
                instance.m_gates = castValue;
                return true;
            }
            case "m_schedule":
            case "schedule":
            {
                if (value is not Havok.hk2019.hkaiGatePathUtil.ExponentialSchedule castValue) return false;
                instance.m_schedule = castValue;
                return true;
            }
            case "m_needsInitialSmooth":
            case "needsInitialSmooth":
            {
                if (value is not bool castValue) return false;
                instance.m_needsInitialSmooth = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
