// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpTyremarksInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpTyremarksInfo>
{
    public hknpTyremarksInfoData(Havok.hk2014.hknpTyremarksInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
                if (value is not List<Havok.hk2014.hknpTyremarksWheel?> castValue) return false;
                instance.m_tyremarksWheel = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
