// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpWeldingConfigData : HavokData<hknpWeldingConfig> 
{
    public hknpWeldingConfigData(HavokType type, hknpWeldingConfig instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_relativeGhostPlaneThreshold":
            case "relativeGhostPlaneThreshold":
            {
                if (instance.m_relativeGhostPlaneThreshold is not TGet castValue) return false;
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
            case "m_relativeGhostPlaneThreshold":
            case "relativeGhostPlaneThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_relativeGhostPlaneThreshold = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
