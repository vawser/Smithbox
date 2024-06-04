// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiGatePathUtil;

namespace HKLib.Reflection.hk2018;

internal class hkaiGatePathData : HavokData<hkaiGatePath> 
{
    public hkaiGatePathData(HavokType type, hkaiGatePath instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_gates":
            case "gates":
            {
                if (instance.m_gates is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_startPoint":
            case "startPoint":
            {
                if (instance.m_startPoint is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_gates":
            case "gates":
            {
                if (value is not List<hkaiGatePath.PathGate> castValue) return false;
                instance.m_gates = castValue;
                return true;
            }
            case "m_startPoint":
            case "startPoint":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_startPoint = castValue;
                return true;
            }
            case "m_schedule":
            case "schedule":
            {
                if (value is not ExponentialSchedule castValue) return false;
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
