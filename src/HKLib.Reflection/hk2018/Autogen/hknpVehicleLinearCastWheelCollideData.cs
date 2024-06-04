// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpVehicleLinearCastWheelCollideData : HavokData<hknpVehicleLinearCastWheelCollide> 
{
    public hknpVehicleLinearCastWheelCollideData(HavokType type, hknpVehicleLinearCastWheelCollide instance) : base(type, instance) {}

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
            case "m_alreadyUsed":
            case "alreadyUsed":
            {
                if (instance.m_alreadyUsed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wheelStates":
            case "wheelStates":
            {
                if (instance.m_wheelStates is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxExtraPenetration":
            case "maxExtraPenetration":
            {
                if (instance.m_maxExtraPenetration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_startPointTolerance":
            case "startPointTolerance":
            {
                if (instance.m_startPointTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_chassisBody":
            case "chassisBody":
            {
                if (instance.m_chassisBody is not TGet castValue) return false;
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
            case "m_alreadyUsed":
            case "alreadyUsed":
            {
                if (value is not bool castValue) return false;
                instance.m_alreadyUsed = castValue;
                return true;
            }
            case "m_wheelStates":
            case "wheelStates":
            {
                if (value is not List<hknpVehicleLinearCastWheelCollide.WheelState> castValue) return false;
                instance.m_wheelStates = castValue;
                return true;
            }
            case "m_maxExtraPenetration":
            case "maxExtraPenetration":
            {
                if (value is not float castValue) return false;
                instance.m_maxExtraPenetration = castValue;
                return true;
            }
            case "m_startPointTolerance":
            case "startPointTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_startPointTolerance = castValue;
                return true;
            }
            case "m_chassisBody":
            case "chassisBody":
            {
                if (value is not hknpBodyId castValue) return false;
                instance.m_chassisBody = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
