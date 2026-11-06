// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclSimClothDataOverridableSimulationInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclSimClothData.OverridableSimulationInfo>
{
    public hclSimClothDataOverridableSimulationInfoData(Havok.hk2018.hclSimClothData.OverridableSimulationInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_gravity":
            case "gravity":
            {
                value = instance.m_gravity;
                return true;
            }
            case "m_globalDampingPerSecond":
            case "globalDampingPerSecond":
            {
                value = instance.m_globalDampingPerSecond;
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
            case "m_gravity":
            case "gravity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_gravity = castValue;
                return true;
            }
            case "m_globalDampingPerSecond":
            case "globalDampingPerSecond":
            {
                if (value is not float castValue) return false;
                instance.m_globalDampingPerSecond = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
