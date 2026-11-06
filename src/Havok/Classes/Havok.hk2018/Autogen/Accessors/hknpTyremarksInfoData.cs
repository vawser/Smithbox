// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpTyremarksInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpTyremarksInfo>
{
    public hknpTyremarksInfoData(Havok.hk2018.hknpTyremarksInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_minTyremarkEnergy":
            case "minTyremarkEnergy":
            {
                value = instance.m_minTyremarkEnergy;
                return true;
            }
            case "m_maxTyremarkEnergy":
            case "maxTyremarkEnergy":
            {
                value = instance.m_maxTyremarkEnergy;
                return true;
            }
            case "m_tyremarksWheel":
            case "tyremarksWheel":
            {
                value = instance.m_tyremarksWheel;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
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
                if (value is not List<Havok.hk2018.hknpTyremarksWheel?> castValue) return false;
                instance.m_tyremarksWheel = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
