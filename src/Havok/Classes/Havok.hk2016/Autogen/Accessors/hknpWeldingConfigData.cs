// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hknpWeldingConfigData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hknpWeldingConfig>
{
    public hknpWeldingConfigData(Havok.hk2016.hknpWeldingConfig instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_relativeGhostPlaneThreshold":
            case "relativeGhostPlaneThreshold":
            {
                value = instance.m_relativeGhostPlaneThreshold;
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
