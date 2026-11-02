// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiGatePathData : HavokData<HKLib.hk2019.hkaiGatePath> 
{
    public hkaiGatePathData(HavokType type, HKLib.hk2019.hkaiGatePath instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_gates":
            case "gates":
            {
                if (instance.m_gates is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_schedule":
            case "schedule":
            {
                if (instance.m_schedule is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_needsInitialSmooth":
            case "needsInitialSmooth":
            {
                if (instance.m_needsInitialSmooth is not TGet castValue) return false;
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
            case "m_gates":
            case "gates":
            {
                if (value is not List<HKLib.hk2019.hkaiGatePath.PathGate> castValue) return false;
                instance.m_gates = castValue;
                return true;
            }
            case "m_schedule":
            case "schedule":
            {
                if (value is not HKLib.hk2019.hkaiGatePathUtil.ExponentialSchedule castValue) return false;
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
