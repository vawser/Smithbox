// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpTyremarksInfoData : HavokData<hknpTyremarksInfo> 
{
    public hknpTyremarksInfoData(HavokType type, hknpTyremarksInfo instance) : base(type, instance) {}

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
            case "m_minTyremarkEnergy":
            case "minTyremarkEnergy":
            {
                if (instance.m_minTyremarkEnergy is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxTyremarkEnergy":
            case "maxTyremarkEnergy":
            {
                if (instance.m_maxTyremarkEnergy is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tyremarksWheel":
            case "tyremarksWheel":
            {
                if (instance.m_tyremarksWheel is not TGet castValue) return false;
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
            case "m_minTyremarkEnergy":
            case "minTyremarkEnergy":
            {
                if (value is not float castValue) return false;
                instance.m_minTyremarkEnergy = castValue;
                return true;
            }
            case "m_maxTyremarkEnergy":
            case "maxTyremarkEnergy":
            {
                if (value is not float castValue) return false;
                instance.m_maxTyremarkEnergy = castValue;
                return true;
            }
            case "m_tyremarksWheel":
            case "tyremarksWheel":
            {
                if (value is not List<hknpTyremarksWheel?> castValue) return false;
                instance.m_tyremarksWheel = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
